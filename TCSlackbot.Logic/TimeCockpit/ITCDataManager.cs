using System.Collections.Generic;

namespace TCSlackbot.Logic.Utils
{
    interface ITCDataManager
    {
        public IEnumerable<T> GetObjects<T>(string accessToken);

        public IEnumerable<T> GetFilteredObjects<T>(string accessToken, string filter);
    }
}
