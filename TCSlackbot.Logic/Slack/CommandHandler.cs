using Microsoft.AspNetCore.DataProtection;
using System;
using System.Threading.Tasks;
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
                return "Something went wrong. Please login again.";
            }

            if (!IsLoggedIn(userId))
            {
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }

            if (user.IsWorking)
            {
                return "You are already working.";
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
                return "Something went wrong. Please login again.";
            }

            if (!IsLoggedIn(userId))
            {
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }

            if (!user.IsWorking)
            {
                return "You have to be working.";
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

            return "You stopped working.";
        }

        //NOT TESTED YET
        public async Task<string> ResumeWorktimeAsync(SlackEvent slackEvent)
        {
            var userId = slackEvent.User;
            var user = await GetSlackUserAsync(userId);

            if (!IsLoggedIn(userId))
            {
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }

            if (!user.IsWorking)
            {
                return "You are not working at the moment. Did you forget to type start?";
            }

            if (!user.IsOnBreak)
            {
                return "You are not on break. Did you forget to pause?";
            }
            user.TotalBreakTime = (DateTime.Now.Minute - user.BreakTime.Value.Minute);
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);
            return "Break has ended." + user.BreakTime; // No
        }

        public string GetLoginLink(SlackEvent slackEvent)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return "<https://localhost:6001/auth/link/?uuid=" + _protector.Protect(slackEvent.User) + "|Link TimeCockpit Account>";
            }
            else
            {
                return "<https://tcslackbot.azurewebsites.net/auth/link/?uuid=" + _protector.Protect(slackEvent.User) + "|Link TimeCockpit Account>";
            }
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
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }

            if (!user.IsWorking)
            {
                return "You are not working at the moment. Did you forget to type start?";
            }

            if (user.IsOnBreak)
            {
                return "You are already on break. Did you forget to unpause?";
            }

            user.BreakTime = DateTime.Now;
            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);
            return "Break has been set. You can now relax.";


        }

        public bool IsLoggedIn(string userId)
        {
            return _secretManager.GetSecret(userId) != null;
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
