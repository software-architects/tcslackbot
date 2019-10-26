using SlackAPI;
using System;
using System.Threading;

namespace TCSlackbot {
    public class WeatherForecast {
        /*
           SlackClient.StartAuth((AuthStartResponse) => {
               SlackClient.AuthSignin(
                   (authSigninResponse) => {
                       // token = ""; Authentification Token
                       "token";
                   },
                   AuthStartResponse.users[0].user_id,
                   AuthStartResponse.users[1].team_id,
                   //Password
                   "password"
                   );   
           }, "Mail@Mail.com");
           */
        ManualResetEventSlim clientReady = new ManualResetEventSlim(false);
        SlackSocketClient client = new SlackSocketClient()
    }
}
