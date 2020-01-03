using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    interface ITCDataManager
    {
        public Task<IEnumerable<T>> GetObjectsAsync<T>(string accessToken);

        public Task<IEnumerable<T>> GetFilteredObjectsAsync<T>(string accessToken, string filter);
    }
}
