using SlackAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TCSlackbot.Logic{
    public class BotClient : IBotClient {
        public void Test(AuthTokensSlack _authTokensSlack) {
            ManualResetEventSlim clientReady = new ManualResetEventSlim(false);
            SlackSocketClient client = new SlackSocketClient(_authTokensSlack.UserAuthToken);
            client.Connect((connected) => {
                // This is called once the client has emitted the RTM start command
                clientReady.Set();
            }, () => {
                // This is called once the RTM client has connected to the end point
            });
            client.OnMessageReceived += (message) =>
            {
                if(message.text == "quit") {
                    clientReady.Dispose();
                }
                // Handle each message as you receive them
            };

            client.GetChannelList((clr) => { Console.WriteLine("got channels"); });
            foreach (Channel ch in client.Channels) {
                Console.WriteLine(ch.name);
            }
            var c = client.Channels.Find(x => x.name.Contains("general"));
            client.PostMessage((mr) => Console.WriteLine("sent message to general!"), c.id, "Hello general world");
            clientReady.Wait();
        }
        
    }
}
