using Microsoft.AspNetCore.DataProtection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic.Authentication;
using TCSlackbot.Logic.Authentication.Exceptions;
using TCSlackbot.Logic.Cosmos;
using TCSlackbot.Logic.Resources;
using TCSlackbot.Logic.TimeCockpit;
using TCSlackbot.Logic.TimeCockpit.Objects;
using TCSlackbot.Logic.Utils;

namespace TCSlackbot.Logic.Slack
{
    public class CommandHandler
    {
        private readonly IDataProtector _protector;
        private readonly ICosmosManager _cosmosManager;
        private readonly ISecretManager _secretManager;
        private readonly ITokenManager _tokenManager;
        private readonly ITCManager _tcDataManager;

        public CommandHandler(IDataProtector protector,
            ICosmosManager cosmosManager,
            ISecretManager secretManager,
            ITokenManager tokenManager,
            ITCManager tcDataManager)
        {
            _protector = protector;
            _cosmosManager = cosmosManager;
            _secretManager = secretManager;
            _tokenManager = tokenManager;
            _tcDataManager = tcDataManager;
        }

        /// <summary>
        /// The user wants to get the duration of the current work session.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> GetWorktimeAsync(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            if (!user.IsWorking)
            {
                return BotResponses.NotWorking;
            }

            var timeSpan = user.TotalWorkTime();
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;

            var hourString = hours > 1 ? "hours" : "hour";
            var minuteString = minutes > 1 ? "minutes" : "minute";

            return $"You have been working for {((hours > 0) ? string.Format("{0} {1} {2} {3}", hours, hourString, minutes, minuteString) : string.Format("{0} {1}", minutes, minuteString))}";
        }

        /// <summary>
        /// The user wants to start working.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> StartWorkingAsync(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            if (user.IsWorking)
            {
                return BotResponses.AlreadyWorking;
            }

            if (user.IsOnBreak)
            {
                return BotResponses.ErrorOnBreak;
            }

            //
            // Start working
            //
            user.IsWorking = true;
            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

            return BotResponses.StartedWorking;

        }

        /// <summary>
        /// The user wants to stop working.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> StopWorkingAsync(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            if (!user.IsWorking)
            {
                return BotResponses.NotWorking;
            }

            if (user.IsOnBreak)
            {
                return BotResponses.ErrorOnBreak;
            }

            // stop <description> <project>
            // stop <description> -> Uses the default project
            var text = slackEvent.Text.ToLower().Trim().Split(" ");
            if (text.Length != 2 && text.Length != 3)
            {
                return BotResponses.InvalidParameter + " You can use it like this: 'stop <description> <optional_project>'";
            }

            try
            {
                var accessToken = await _tokenManager.GetAccessTokenAsync(userId);
                if (accessToken == null)
                {
                    return BotResponses.InvalidAccessToken;
                }

                // 
                // Extract the parameters
                // 
                var description = text.ElementAtOrDefault(1);
                var project = text.Length == 3 ? await _tcDataManager.GetProjectAsync(accessToken, text.ElementAtOrDefault(2)) : user.DefaultProject;
                if (project == null)
                {
                    return BotResponses.ProjectNotFound;
                }

                //
                // Get the data needed to the request
                //
                user.IsWorking = false;
                var sessions = user.GetWorkSessions();

                var userDetail = await _tcDataManager.GetCurrentUserDetailsAsync(accessToken);
                if (userDetail == null)
                {
                    return BotResponses.ObjectNotFound;
                }

                //
                // Send each session
                //
                foreach (var session in sessions)
                {
                    var timesheet = new Timesheet
                    {
                        BeginTime = session.Start.GetValueOrDefault(),
                        EndTime = session.End.GetValueOrDefault(),
                        UserDetailUuid = userDetail.UserDetailUuid,
                        ProjectUuid = project.ProjectUuid,
                        Description = description
                    };
                    Console.WriteLine(timesheet);
                    var response = await _tcDataManager.CreateObjectAsync(accessToken, timesheet);

                    Console.WriteLine(response);
                }
            }
            catch (LoggedOutException)
            {
                return BotResponses.ErrorLoggedOut;
            }

            //
            // Reset and save the user data
            //
            user.ResetWorktime();
            user.Breaks?.Clear();
            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

            return BotResponses.StoppedWorking;
        }

        /// <summary>
        /// The user wants to resume working.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> ResumeWorktimeAsync(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            if (!user.IsWorking)
            {
                return BotResponses.NotWorking;
            }

            if (!user.IsOnBreak)
            {
                return BotResponses.NotOnBreak;
            }

            if (!user.StopBreak(DateTime.Now))
            {
                return BotResponses.EndBreakFailure;
            }

            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

            return BotResponses.BreakEnded;
        }

        /// <summary>
        /// The user wants a break. 
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> PauseWorktimeAsync(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            if (!user.IsWorking)
            {
                return BotResponses.NotWorking;
            }

            if (user.IsOnBreak)
            {
                return BotResponses.AlreadyOnBreak;
            }

            if (!user.StartBreak(DateTime.Now))
            {
                return BotResponses.StartBreakFailure;
            }

            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

            return BotResponses.StartedBreak;
        }

        /// <summary>
        /// The user wants a list of objects.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> FilterObjectsAsync(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            // filter <object> <filter_text>
            var text = slackEvent.Text.ToLower().Trim().Split(" ");
            if (text.Length != 3)
            {
                return BotResponses.InvalidParameter;
            }

            switch (text.ElementAtOrDefault(1))
            {
                case "projects":
                case "project":
                    try
                    {
                        var accessToken = await _tokenManager.GetAccessTokenAsync(userId);
                        if (accessToken == null)
                        {
                            return BotResponses.InvalidAccessToken;
                        }

                        var data = await _tcDataManager.GetFilteredProjects(accessToken, text.ElementAtOrDefault(2));
                        return data.Any() ? string.Join('\n', data.Take(10).Select(element => $"- {element.ProjectName}")) : BotResponses.NoObjectsFound;
                    }
                    catch (LoggedOutException)
                    {
                        return BotResponses.ErrorLoggedOut;
                    }

                case "tasks":
                case "task":
                    // Send request to the TimeCockpit API
                    // Return the list with the data

                    return BotResponses.NoObjectsFound;
            }

            return BotResponses.FilterObjectNotFound;
        }

        /// <summary>
        /// Sets the default project for the user who executed the command.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> SetDefaultProject(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            // project <project_name>
            var text = slackEvent.Text.ToLower().Trim().Split(" ");
            if (text.Length != 2)
            {
                return BotResponses.InvalidParameter;
            }

            try
            {
                var accessToken = await _tokenManager.GetAccessTokenAsync(userId);
                if (accessToken == null)
                {
                    return BotResponses.InvalidAccessToken;
                }

                var data = await _tcDataManager.GetFilteredProjects(accessToken, text.ElementAtOrDefault(1));
                
                if (data.Count() == 1)
                {
                    user.DefaultProject = data.FirstOrDefault();
                    await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

                    return BotResponses.DefaultProjectSet;
                }
            }
            catch (LoggedOutException)
            {
                return BotResponses.ErrorLoggedOut;
            }

            return BotResponses.ObjectNotFound;
        }

        /// <summary>
        /// Logs the user out and deletes his refresh token.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> Logout(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            if (user.IsWorking)
            {
                return BotResponses.UnlinkWhileWorking;
            }

            if (user.IsOnBreak)
            {
                return BotResponses.UnlinkWhileOnBreak;
            }

            await _secretManager.DeleteSecretAsync(slackEvent.User);

            return BotResponses.Unlinked;
        }

        /// <summary>
        /// Generates a login link for the specified slack user.
        /// </summary>
        /// <param name="slackEvent">The event containing the user id</param>
        /// <returns>The login link as a string</returns>
        public string GetLoginLink(SlackEvent slackEvent)
        {
            if (slackEvent is null)
            {
                return BotResponses.Error;
            }

            var userId = slackEvent.User;

            //
            // Check if already logged in
            //
            if (IsLoggedIn(userId))
            {
                return BotResponses.AlreadyLoggedIn;
            }

            //
            // Create the data
            //
            var linkData = new LinkData
            {
                UserId = userId,
                ValidUntil = DateTime.Now.AddHours(1)
            };
            var jsonData = JsonSerializer.Serialize(linkData);

            //
            // Send the login link
            //
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return "<https://localhost:6001/auth/link/?data=" + _protector.Protect(jsonData) + "|Link Account>";
            }
            else
            {
                return "<https://tcslackbot.azurewebsites.net/auth/link/?data=" + _protector.Protect(jsonData) + "|Link Account>";
            }
        }

        /// <summary>
        /// Returns the user for the specified userId or creates it.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>The object of the slack user</returns>
        public async Task<SlackUser?> GetSlackUserAsync(string userId)
        {
            //
            // Check if the user is logged in
            //
            if (!IsLoggedIn(userId))
            {
                return null;
            }

            //
            // Create a new user if not found
            //
            SlackUser? user = await _cosmosManager.GetDocumentAsync<SlackUser>(Collection.Users, userId);
            if (user is null)
            {
                user = await _cosmosManager.CreateDocumentAsync(Collection.Users, new SlackUser (userId));
            }

            //
            // Check for a tampered userid
            //
            if (user != null && user.UserId != userId)
            {
                user = null;
            }

            return user;
        }

        /// <summary>
        /// Checks whether the specified user is logged in.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>True if already logged in</returns>
        public bool IsLoggedIn(string userId)
        {
            return _secretManager.GetSecret(userId) != null;
        }
    }
}
