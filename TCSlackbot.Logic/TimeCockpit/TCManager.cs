using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TCSlackbot.Logic.Utils
{
    class TCManager : ITCDataManager
    {
        private static readonly HttpClient client = new HttpClient();

        public IEnumerable<T> GetFilteredObjects<T>(string accessToken, string filter)
        {
            var objectName = $"APP_{typeof(T).Name}";

            Console.WriteLine(objectName);


            return default;
        }

        public IEnumerable<T> GetObjects<T>(string accessToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
