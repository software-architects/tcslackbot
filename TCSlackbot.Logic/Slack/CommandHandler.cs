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
            if (await IsWorkingAsync(request))
            {
                return "Started at: " + (await GetSlackUserAsync(request.Event.User))?.StartTime;
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
            if (IsLoggedIn(request) && await IsWorkingAsync(request) && !await IsOnBreakAsync(request))
            {
                var user = await GetSlackUserAsync(request.Event.User);
                var curBreakTime = user.OnBreak;

                return "Break has been set. You can now relax.";
            }
            if (!IsLoggedIn(request)) return "You have to login before you can use this bot!\nType login or link to get the login link.";
            if (!await IsWorkingAsync(request)) return "You are not working at the moment. Did you forget to type start?";
            if (await IsOnBreakAsync(request)) return "You are already on break. Did you forget to unpause?";
            return "You shouldn't get this message.";
        }

        public async Task<string> StartWorktimeAsync(SlackEventCallbackRequest request)
        {
            if (!IsLoggedIn(request))
            {
                return "You have to login before you can use this bot!\nType login or link to get the login link.";
            }

            if (await IsWorkingAsync(request))
            {
                return "You are already working.";
            }

            //
            // Get the user from the database
            //
            var user = await GetSlackUserAsync(request.Event.User);
            if (user is null)
            {
                // User already logged in but no user in the database -> Should never happen
                return "Something went wrong. Please login again.";
            }

            //
            // Tampered userid detected
            //
            if (user.UserId != request.Event.User)
            {
                return "Something went wrong. Please login again.";
            }

            //
            // LoggedIn && !IsWorking
            //
            user.StartTime = DateTime.Now;

            await _cosmosManager.ReplaceDocumentAsync(CollectionId, user, user.UserId);

            return "You started working.";
        }

        public bool IsLoggedIn(SlackEventCallbackRequest request)
        {
            return _secretManager.GetSecret(request.Event.User) != null;
        }

        // TODO: Maybe only pass a SlackUser object
        public async Task<bool> IsWorkingAsync(SlackEventCallbackRequest request)
        {
            var user = await GetSlackUserAsync(request.Event.User);
            return user?.StartTime != null;
        }

        // TODO: Might not even be required: just check IsOnBreak in the SlackUser object
        public async Task<bool> IsOnBreakAsync(SlackEventCallbackRequest request)
        {
            return (await _cosmosManager.GetDocumentAsync<SlackUser>(CollectionId, request.Event.User)).OnBreak;
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
