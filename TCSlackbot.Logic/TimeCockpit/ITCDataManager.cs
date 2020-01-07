using System.Collections.Generic;
using System.Threading.Tasks;
using TCSlackbot.Logic.TimeCockpit;

namespace TCSlackbot.Logic.Utils
{
    public interface ITCDataManager
    {
        public Task<IEnumerable<T>> GetObjectsAsync<T>(string accessToken);

        public Task<IEnumerable<T>> GetFilteredObjectsAsync<T>(string accessToken, TCQueryData queryData);
    }
}
