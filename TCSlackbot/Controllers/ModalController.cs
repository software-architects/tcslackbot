﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TCSlackbot.Logic.TimeCockpit.Objects;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Authentication.Exceptions;
using TCSlackbot.Logic.Cosmos;
using TCSlackbot.Logic.Resources;
using TCSlackbot.Logic.Slack;
using TCSlackbot.Logic.Slack.Requests;
using TCSlackbot.Logic.Utils;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace TCSlackbot.Controllers
{
    [ApiController]
    [Route("modal")]
    public class ModalController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _protector;
        private readonly ISecretManager _secretManager;
        private readonly ICosmosManager _cosmosManager;
        private readonly HttpClient _httpClient;
        private readonly ITokenManager _tokenManager;
        private readonly ITCManager _tcDataManager;
        private readonly CommandHandler _commandHandler;

        public ModalController(
            IConfiguration configuration,
            IHttpClientFactory factory,
            IDataProtectionProvider provider,
            ISecretManager secretManager,
            ICosmosManager cosmosManager,
            ITokenManager tokenManager,
            ITCManager dataManager
            )
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            if (provider is null)
            {
                var message = "IDataProtectionProvider";
                throw new ArgumentNullException(message);
            }

            if (factory is null)
            {
                throw new ArgumentNullException("IHttpClientFactory");
            }

            if (secretManager is null)
            {
                throw new ArgumentNullException("ISecretManager");
            }
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            _configuration = configuration;
            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
            _cosmosManager = cosmosManager;
            _httpClient = factory.CreateClient("BotClient");
            _tokenManager = tokenManager;
            _tcDataManager = dataManager;

            _commandHandler = new CommandHandler(_protector, _cosmosManager, _secretManager, _tokenManager, _tcDataManager);
        }

        /// <summary>
        /// Returns the data that is needed for the 'external select'.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("data")]
        public async Task<IActionResult> GetExternalData()
        {
            var payload = Serializer.Deserialize<AppActionPayload>(HttpContext.Request.Form["payload"]);
            if (payload is null || payload.User is null)
            {
                return BadRequest();
            }

            try
            {
                var accessToken = await _tokenManager.GetAccessTokenAsync(payload.User.Id);
                if (accessToken == null)
                {
                    return BadRequest();
                }

                var projects = await _tcDataManager.GetFilteredProjects(accessToken, payload.Value);

                // Create the list of projects
                string json = "{\"options\": [";
                foreach (var project in projects)
                {
                    json += "{\"text\": {\"type\": \"plain_text\",  \"text\": \"" + project.ProjectName + "\"},\"value\": \"" + project.ProjectName + "\" },";
                }
                json = json.Remove(json.Length - 1) + "]}";

                return Content(json, "application/json");
            }
            catch (LoggedOutException)
            {
                return BadRequest(BotResponses.ErrorLoggedOut);
            }
        }

        /// <summary>
        /// Handles the incoming requests (only if they have a valid slack signature).
        /// </summary>
        /// <param name="body">The dynamic request body</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> HandleRequestAsync()
        {
            var payload = Serializer.Deserialize<AppActionPayload>(HttpContext.Request.Form["payload"]);
            if (payload is null || payload.User is null || payload?.Team is null) // || payload?.Channel is null
            {
                return BadRequest();
            }
            if(payload.Channel == null)
            {
                payload.Channel = new UserChannel();
                payload.Channel.Id = "";
            }
            var replyData = new Dictionary<string, string>
            {
                ["user"] = payload.User.Id,
                ["channel"] = payload.Channel.Id,
            };

            //
            // Ignore unecessary requests
            //
            if (payload.Type == "block_actions" || payload.Type == "view_closed")
                return Ok();

            //
            // Check if user is logged in, working
            //
            var user = await _commandHandler.GetSlackUserAsync(payload.User.Id);
            if (user == null)
            {
                replyData["text"] = BotResponses.NotLoggedIn;
                await SendReplyAsync(payload.Team.Id, replyData, true);

                return Ok();
            }

            if (!user.IsWorking)
            {
                replyData["text"] = BotResponses.NotWorking;
                await SendReplyAsync(payload.Team.Id, replyData, true);

                return Ok();
            }

            switch (payload.Type)
            {
                case "message_action":
                    return await ViewModalAsync(payload);

                case "view_submission":
                    return await ProcessModalDataAsync(user);

                default:
                    Console.WriteLine($"Received unhandled request: {payload.Type}.");
                    break;
            }

            return Ok();
        }

        /// <summary>
        /// Called whenever the user wants to open a modal.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<IActionResult> ViewModalAsync(AppActionPayload payload)
        {
            if (payload is null)
            {
                return BadRequest();
            }

            var payloadTeamId = payload?.Team?.Id;
            var payloadUserId = payload?.User?.Id;
            var payloadTriggerId = payload?.TriggerId;
            var payloadCallbackId = payload?.CallbackId;
            if (payloadTeamId is null || payloadUserId is null || payloadTriggerId is null || payloadCallbackId is null)
            {
                return BadRequest();
            }
            
            var user = await _commandHandler.GetSlackUserAsync(payloadUserId);
            if (user == null)
            {
                return BadRequest();
            }

            //
            // Load the modal from the file and set the dynamic values by replacing the placeholders in the json:
            // A placeholder could look like this: '"REPALCE_TEST"' -> you need the double quotes here.
            //
            var json = await System.IO.File.ReadAllTextAsync("Json/StopModal.json");

            json = json.Replace("REPLACE_TRIGGER_ID", payloadTriggerId, StringComparison.Ordinal);
            json = json.Replace("REPLACE_CALLBACK_ID", payloadCallbackId, StringComparison.Ordinal);

            var date = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            json = json.Replace("REPLACE_DATE", date, StringComparison.Ordinal);

            var startTime = user?.Worktime?.Start != null ? user.Worktime.Start.Value.ToString("HH:mm", CultureInfo.InvariantCulture) : string.Empty;
            json = json.Replace("REPLACE_START", startTime, StringComparison.Ordinal);

            var endTime = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
            json = json.Replace("REPLACE_END", endTime, StringComparison.Ordinal);

            var initialOptionString = user?.DefaultProject != null
                ? "\"initial_option\":{\"text\":{\"type\":\"plain_text\",\"text\":\"REPLACE_PROJECT\",\"emoji\":true},\"value\":\"REPLACE_PROJECT\"},"
                    .Replace("REPLACE_PROJECT", user.DefaultProject.ProjectName, StringComparison.Ordinal)
                : string.Empty;
            json = json.Replace("INITIAL_OPTION", initialOptionString, StringComparison.Ordinal);

            //
            // Send the response
            //
            await OpenModalAsync(payloadTeamId, json);

            return Ok(json);
        }

        /// <summary>
        /// Handle the submission of the modal.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessModalDataAsync(SlackUser user)
        {
            if (user is null)
            {
                return BadRequest();
            }

            var payload = Serializer.Deserialize<SlackViewSubmission>(HttpContext.Request.Form["payload"]);
            var payloadStart = payload?.View?.State?.Values?.Starttime?.StartTime?.Value;
            var payloadEnd = payload?.View?.State?.Values?.Endtime?.EndTime?.Value;
            var payloadDate = payload?.View?.State?.Values?.Date?.Date?.Day;
            var payloadUserId = payload?.User?.Id;
            var payloadDescription = payload?.View?.State?.Values?.Description?.Description?.Value;
            var payloadProjectName = payload?.View?.State?.Values?.Project?.Project?.SelectedOption?.Value;
            if (payloadEnd is null || payloadStart is null || payloadDate is null || payloadUserId is null || payloadDescription is null || payloadProjectName is null)
            {
                return BadRequest();
            }

            //
            // Generate the error response (if there are any)
            //
            string errorMessage = "{ \"response_action\": \"errors\", \"errors\": {";
            if (!TimeSpan.TryParseExact(payloadStart, "h\\:mm", CultureInfo.InvariantCulture, out TimeSpan startTime))
            {
                errorMessage += "\"starttime\": \"Please use a valid time format! (eg. '08:00')\",";
            }
            if (!TimeSpan.TryParseExact(payloadEnd, "h\\:mm", CultureInfo.InvariantCulture, out TimeSpan endTime))
            {
                errorMessage += "\"endtime\": \"Please use a valid time format! (eg. '18:00')\",";
            }
            if (payload?.View?.State?.Values?.Project == null)
            {
                errorMessage += "\"project\": \"Please select a project!\",";
            }
            else if (endTime.CompareTo(startTime) != 1)
            {
                errorMessage += "\"endtime\": \"End Time has to be after Start Time!\",";
            }

            //
            // Check if there was an error and return it
            //
            if (errorMessage.EndsWith(",", StringComparison.CurrentCulture))
            {
                errorMessage = errorMessage.Remove(errorMessage.Length - 1) + "}}";

                return Content(errorMessage, "application/json", Encoding.UTF8);
            }

            // 
            // Request the access token
            // 
            var accessToken = await _tokenManager.GetAccessTokenAsync(payloadUserId);
            if (accessToken == null)
            {
                return BadRequest();
            }

            //
            // Set and save the values
            //
            user.IsWorking = false;
            user.Worktime = new Duration(payloadDate + startTime, payloadDate + endTime);

            //
            // Send the request
            //
            
            // Get the project
            var project = await _tcDataManager.GetProjectAsync(accessToken, payloadProjectName);
            if (project == null)
            {
                return BadRequest();
            }

            // Get the user details
            var userDetail = await _tcDataManager.GetCurrentUserDetailsAsync(accessToken);
            if (userDetail == null)
            {
                return BadRequest();
            }

            // Send each session
            foreach (var session in user.GetWorkSessions())
            {
                var timesheet = new Timesheet
                {
                    BeginTime = session.Start.GetValueOrDefault(),
                    EndTime = session.End.GetValueOrDefault(),
                    UserDetailUuid = userDetail.UserDetailUuid,
                    ProjectUuid = project.ProjectUuid,
                    Description = payloadDescription
                };

                await _tcDataManager.CreateObjectAsync(accessToken, timesheet);
            }

            //
            // Reset the data
            //
            user.ResetWorktime();
            user.Breaks?.Clear();
            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

            //
            // Send the reply
            //
            //var channel = await CommandController.GetIMChannelFromUserAsync(_httpClient, payloadUserId);
            //if (channel is null)
            //{
            //    return BadRequest();
            //}

            var replyData = new Dictionary<string, string>
            {
                ["user"] = payloadUserId,
                ["channel"] = await GetIMChannelFromUserAsync(_httpClient, user.UserId, payload?.Team.Id),
                ["text"] = "Your time has been saved"
            };

            using var replyForm = new FormUrlEncodedContent(replyData);
            // _ = await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, "chat.postEphemeral"), replyForm);

             await SendReplyAsync(payload.Team.Id, replyData, true);

            return Ok();
        }

        public bool SetAuthToken(String teamId)
        {
            //
            // Set the correct token
            //
            var token = _configuration[teamId];
            if (token is null)
            {
                return false;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        public async Task<string?> GetIMChannelFromUserAsync(HttpClient client, string user, string teamId)
        {
            if (client is null)
            {
                return default;
            }

            if (!SetAuthToken(teamId))
            {
                return default;
            }

            var response = await client.GetAsync(new Uri("https://slack.com/api/conversations.list?types=im"));
            var content = await response.Content.ReadAsStringAsync();
            var conversationChannels = Serializer.Deserialize<ConversationsList>(content)?.Channels;
            if (conversationChannels is null)
            {
                return default;
            }

            foreach (var channel in conversationChannels)
            {
                if (channel.User == user)
                {
                    return channel.Id;
                }
            }

            return default;
        }

        /// <summary>
        /// Sends a reply either in the group channel or via direct message.
        /// </summary>
        /// <param name="replyData">The data of the reply (message, channel, ...)</param>
        /// <param name="directMessage">True when it should be sent via direct message</param>
        /// <returns></returns>
        public async Task SendReplyAsync(string teamId, Dictionary<string, string> replyData, bool directMessage)
        {
            string requestUri = "chat.postMessage";

            if (!SetAuthToken(teamId))
            {
                return ;
            }

            //
            // Use a different uri for the direct message
            //
            if (directMessage)
            {
                requestUri = "chat.postEphemeral";
            }

            using var content = new FormUrlEncodedContent(replyData);
            _ = await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, requestUri), content);

            // 
            // Reset the authorization header
            //
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Opens the modal by sending a request to views.open.
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="jsonContent"></param>
        /// <returns></returns>
        public async Task OpenModalAsync(string teamId, string jsonContent)
        {
            //
            // Set the correct token
            //
            var token = _configuration[teamId];
            if (token is null)
            {
                return;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
            {
                await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, "views.open"), content);
            }

            // 
            // Reset the authorization header
            //
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}