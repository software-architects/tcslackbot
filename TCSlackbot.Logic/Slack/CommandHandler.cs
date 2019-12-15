﻿using Microsoft.AspNetCore.DataProtection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TCSlackbot.Logic.Resources;
using TCSlackbot.Logic.Utils;

namespace TCSlackbot.Logic.Slack
{
    public class CommandHandler
    {
        private const string CollectionId = "slack_users";
        private readonly IDataProtector _protector;
        private readonly ICosmosManager _cosmosManager;
        private readonly ISecretManager _secretManager;

        public CommandHandler(IDataProtector protector, ICosmosManager cosmosManager, ISecretManager secretManager)
        {
            _protector = protector;
            _cosmosManager = cosmosManager;
            _secretManager = secretManager;
        }

        /// <summary>
        /// The user wants to get the duration of the current work session.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> GetWorktimeAsync(SlackEvent slackEvent)
        {
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

            // TODO: Maybe show a duration instead
            return "Started at: " + user.StartTime;
        }

        /// <summary>
        /// The user wants to start working.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> StartWorkingAsync(SlackEvent slackEvent)
        {
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

            //
            // Start working
            //
            user.IsWorking = true;

            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);

            return BotResponses.StartedWorking;

        }

        /// <summary>
        /// The user wants to stop working.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> StopWorkingAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (userId is null)
            {
                return BotResponses.NotLoggedIn;
            }

            if (!user.IsWorking)
            {
                return BotResponses.NotWorking;
            }

            //
            // Send the request to the TimeCockpit API
            //
            user.EndTime = DateTime.Now;

            // TODO: Send the request

            //
            // Stop working (reset the start and end time)
            //
            user.IsWorking = false;
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);

            //
            // This will maybe be done with Slack Modal 
            //
            // TODO: Implement
            var message = slackEvent.Text.Split(" ");
            try
            {
                // user.Project = message[1];
                if (slackEvent.Text.Split(" ").Length > 2)
                {
                    // user.Description = message[2];
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                return "Wrong Syntax";
            }

            return BotResponses.StoppedWorking;
        }

        /// <summary>
        /// The user wants to resume working.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> ResumeWorktimeAsync(SlackEvent slackEvent)
        {
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

            // TODO: Implement

            user.TotalBreakTime = (DateTime.Now.Minute - user.BreakTime.Value.Minute);
            user.BreakTime = null;
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);
            return "Break has ended. Total Break Time: " + user.TotalBreakTime + "min"; // No
        }

        /// <summary>
        /// The user wants a list of objects.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> FilterObjectsAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                return BotResponses.NotLoggedIn;
            }

            var text = slackEvent.Text.ToLower().Trim().Split(" ");

            switch (text.ElementAtOrDefault(1))
            {
                case "projects":
                case "project":
                    // Send request to the TimeCockpit API
                    // Return the list with the data

                    break;

                case "tasks":
                case "task":
                    // Send request to the TimeCockpit API
                    // Return the list with the data

                    break;

                default:
                    return BotResponses.FilterObjectNotFound;
            }

            return BotResponses.FilterObjectNotFound;
        }

        /// <summary>
        /// The user wants a break. 
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns>The bot response message</returns>
        public async Task<string> PauseWorktimeAsync(SlackEvent slackEvent)
        {
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

            // TODO: Set break time? Maybe with a list of breaks?

            user.BreakTime = DateTime.Now;
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);

            return BotResponses.StartedBreak;
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

        /// <summary>
        /// Generates a login link for the specified slack user.
        /// </summary>
        /// <param name="slackEvent">The event containing the user id</param>
        /// <returns>The login link as a string</returns>
        public string GetLoginLink(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;

            //
            // Check if already logged in
            //
            if (IsLoggedIn(userId))
            {
                return BotResponses.AlreadyLoggedIn;
            }

            //
            // Send the login link
            //
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return "<https://localhost:6001/auth/link/?uuid=" + _protector.Protect(slackEvent.User) + "|Link TimeCockpit Account>";
            }
            else
            {
                return "<https://tcslackbot.azurewebsites.net/auth/link/?uuid=" + _protector.Protect(slackEvent.User) + "|Link TimeCockpit Account>";
            }
        }

        /// <summary>
        /// Returns the user for the specified userId or creates it.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>The object of the slack user</returns>
        public async Task<SlackUser> GetSlackUserAsync(string userId)
        {
            //
            // Check if the user is logged in
            //
            if (!IsLoggedIn(userId))
            {
                return default;
            }

            //
            // Create a new user if not found
            //
            var user = await _cosmosManager.GetDocumentAsync<SlackUser>(CollectionId, userId);
            if (user is null)
            {
                user = await _cosmosManager.CreateDocumentAsync(CollectionId, new SlackUser { UserId = userId });
            }

            //
            // Check for a tampered userid
            //
            if (user.UserId != userId)
            {
                user = default;
            }

            return user;
        }
    }
}
