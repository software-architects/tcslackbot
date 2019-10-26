using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TCSlackbot.Logic {
    public interface IBotClient {
        public void Test(AuthTokensSlack _authTokensSlack);
    }
}
