using System;
using System.Collections.Generic;
using System.Text;
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

        public string GetWorktime(SlackEventCallbackRequest request)
        {
            return "Started at: " + _cosmosManager.GetSlackUser("Slack_users", request.Event.User).StartTime;
        }

        //TODO
        public string ResumeWorktime(SlackEventCallbackRequest request)
        {
            var curBreakTime = _cosmosManager.GetSlackUser("Slack_users", request.Event.User).BreakTime;
            return "Break has ended." + _cosmosManager.GetSlackUser("Slack_users", request.Event.User).BreakTime; // No
        }

        public string PauseWorktime(SlackEventCallbackRequest request)
        {
            if (IsLoggedIn(request) && IsWorking(request) && !IsOnBreak(request))
            {
                var curBreakTime = _cosmosManager.GetSlackUser("Slack_users", request.Event.User).OnBreak;

                return "Break has been set. You can now relax.";
            }
            if (!IsLoggedIn(request)) return "You have to login before you can use this bot!\nType login or link to get the login link.";
            if (!IsWorking(request)) return "You are not working at the moment. Did you forget to type start?";
            if (IsOnBreak(request)) return "You are already on break. Did you forget to unpause?";
            return "You shouldn't get this message.";
        }

        public string StartWorktime(SlackEventCallbackRequest request)
        {
            if (IsLoggedIn(request)&&!IsWorking(request))
            {
                var user = new SlackUser()
                {
                    UserId = request.Event.User,
                    StartTime = DateTime.Now
                };

                _cosmosManager.CreateDocumentAsync(CollectionId, user);
                return "StartTime has been set!";
            }
            return "You have to login before you can use this bot!\nType login or link to get the login link.";
        }

        public bool IsLoggedIn(SlackEventCallbackRequest request)
        {
            return _secretManager.GetSecret(request.Event.User) != null;
        }
        public bool IsWorking(SlackEventCallbackRequest request)
        {
            var user = _cosmosManager.GetSlackUser("Slack_users", request.Event.User);
            if (user != null)
            {
                return user.StartTime != null;
            }
            return false;
        }
        public bool IsOnBreak(SlackEventCallbackRequest request)
        {
            return _cosmosManager.GetSlackUser("Slack_users", request.Event.User).OnBreak;
        }
    }
}
