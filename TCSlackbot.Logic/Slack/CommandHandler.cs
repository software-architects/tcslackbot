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
            if (await IsWorkingAsync(user))
            {
                return "Started at: " + (await GetSlackUserAsync(user))?.StartTime;
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
            if (!IsLoggedIn(user)) return "You have to login before you can use this bot!\nType login or link to get the login link.";
            if (!await IsWorkingAsync(user)) return "You are not working at the moment. Did you forget to type start?";
            if (await IsOnBreakAsync(user)) return "You are already on break. Did you forget to unpause?";

            var slackUser = await GetSlackUserAsync(user);
            var curBreakTime = slackUser.OnBreak;

            return "Break has been set. You can now relax.";
            
            
        }

        public async Task<string> StartWorktimeAsync(SlackEventCallbackRequest request)
        {
            var user = request.Event.User;
            if (!IsLoggedIn(user))
            {
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }

            if (await IsWorkingAsync(user))
            {
                return "You are already working.";
            }

            //
            // Get the user from the database
            //
            var slackUser = await GetSlackUserAsync(user);
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

        // TODO: Maybe only pass a SlackUser object
        public async Task<bool> IsWorkingAsync(string request)
        {
            var user = await GetSlackUserAsync(request);
            return user?.StartTime != null;
        }

        // TODO: Might not even be required: just check IsOnBreak in the SlackUser object
        public async Task<bool> IsOnBreakAsync(string user)
        {
            return (await _cosmosManager.GetDocumentAsync<SlackUser>(CollectionId, user)).OnBreak;
        }

        /// <summary>
        /// Returns the user for the specified userId.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>The object of the slack user</returns>
        private async Task<SlackUser> GetSlackUserAsync(string userId)
        {
            return await _cosmosManager.GetDocumentAsync<SlackUser>(CollectionId, userId);
        }
    }
}
