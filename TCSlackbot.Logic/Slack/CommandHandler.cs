using System;
using System.Threading.Tasks;
using TCSlackbot.Logic.Utils;

namespace TCSlackbot.Logic.Slack
{
    public class CommandHandler
    {
        private const string CollectionId = "slack_users";

        private readonly ICosmosManager _cosmosManager;
        private readonly ISecretManager _secretManager;

        public CommandHandler(ICosmosManager cosmosManager, ISecretManager secretManager)
        {
            _cosmosManager = cosmosManager;
            _secretManager = secretManager;
        }

        public async Task<string> GetWorktimeAsync(SlackEventCallbackRequest request)
        {
            var user = request.Event.User;
            var slackUser = await GetSlackUserAsync(user);

            if (slackUser.IsWorking)
            {
                return "Started at: " + slackUser?.StartTime;
            }

            return "You are not working!";
        }

        //TODO
        public async Task<string> ResumeWorktimeAsync(SlackEventCallbackRequest request)
        {
            var user = await GetSlackUserAsync(request.Event.User);

            return "Break has ended." + user.BreakTime; // No
        }

        public async Task<string> PauseWorktimeAsync(SlackEventCallbackRequest request)
        {
            var user = request.Event.User;
            var slackUser = await GetSlackUserAsync(user);
            
            //
            if (!IsLoggedIn(user))
            {
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }
            //
            if (!slackUser.IsWorking)
            {
                return "You are not working at the moment. Did you forget to type start?";
            }
            //
            if (slackUser.IsOnBreak)
            {
                return "You are already on break. Did you forget to unpause?";
            }
            
            var curBreakTime = slackUser.IsOnBreak;

            return "Break has been set. You can now relax.";
            
            
        }

        public async Task<string> StartWorktimeAsync(SlackEventCallbackRequest request)
        {
            var user = request.Event.User;
            var slackUser = await GetSlackUserAsync(user);
            if (!IsLoggedIn(user))
            {
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }
            
            if (slackUser.IsWorking)
            {
                return "You are already working.";
            }

            //
            // Get the user from the database
            //
            if (user is null)
            {
                // User already logged in but no user in the database -> Should never happen
                return "Something went wrong. Please login again.";
            }

            //
            // Tampered userid detected
            //
            if (slackUser.UserId != user)
            {
                return "Something went wrong. Please login again.";
            }

            //
            // LoggedIn && !IsWorking
            //
            slackUser.StartTime = DateTime.Now;

            await _cosmosManager.ReplaceDocumentAsync(CollectionId, slackUser, slackUser.UserId);

            return "You started working.";
        }

        public bool IsLoggedIn(string user)
        {
            return _secretManager.GetSecret(user) != null;
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

            return user;
        }
    }
}
