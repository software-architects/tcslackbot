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


        public async Task<string> GetWorktimeAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;
            var user = await GetSlackUserAsync(userId);

            if (user.IsWorking && user.StartTime != null)
            {
                // TODO: Maybe show a duration instead
                return "Started at: " + user.StartTime;
            }

            return "You are not working!";
        }

        public async Task<string> StartWorkingAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                // User already logged in but no user in the database -> Should never happen
                return BotResponses.UserNotFoundError;
            }

            if (!IsLoggedIn(userId))
            {
                return BotResponses.HaveToLogin;
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
            return "You started working.";

        }

        public async Task<string> StopWorkingAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (userId is null)
            {
                // User already logged in but no user in the database -> Should never happen
                return BotResponses.UserNotFoundError;
            }

            if (!IsLoggedIn(userId))
            {
                return BotResponses.HaveToLogin;
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
            // This will maybe be done with Slack Modal 
            //
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
            user.IsWorking = false;
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);

            return "You stopped working.";
        }

        //NOT TESTED YET
        public async Task<string> ResumeWorktimeAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;
            var user = await GetSlackUserAsync(userId);

            if (!IsLoggedIn(userId))
            {
                return BotResponses.HaveToLogin;
            }

            if (!user.IsWorking)
            {
                return BotResponses.NotWorking;
            }

            if (!user.IsOnBreak)
            {
                return BotResponses.NotOnBreak;
            }
            Console.WriteLine(user.BreakTime.Value);
            user.TotalBreakTime = (DateTime.Now.Minute - user.BreakTime.Value.Minute);
            user.BreakTime = null;
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);
            return "Break has ended. Total Break Time: " + user.TotalBreakTime + "min"; // No
        }

       

        public string FilterObjects(SlackEvent slackEvent)
        {
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

        // TODO: Set break time? Maybe with a list of breaks?
        public async Task<string> PauseWorktimeAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;

            var user = await GetSlackUserAsync(userId);
            if (user is null)
            {
                // User already logged in but no user in the database -> Should never happen
                return "Something went wrong. Please login again.";
            }

            if (!IsLoggedIn(userId))
            {
                return BotResponses.HaveToLogin;
            }

            if (!user.IsWorking)
            {
                return BotResponses.NotWorking;
            }

            if (user.IsOnBreak)
            {
                return BotResponses.AlreadyOnBreak;
            }

            user.BreakTime = DateTime.Now;
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);
            return "Break has been set. You can now relax.";


        }

        public bool IsLoggedIn(string userId)
        {
            return _secretManager.GetSecret(userId) != null;
        }

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
        private async Task<SlackUser> GetSlackUserAsync(string userId)
        {
            var user = await _cosmosManager.GetDocumentAsync<SlackUser>(CollectionId, userId);

            //
            // Create a new user if it's not found
            //
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
