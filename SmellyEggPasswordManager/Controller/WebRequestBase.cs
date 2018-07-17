using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmellyEggPasswordManager.Controller
{
    public class WebRequestBase
    {
        //public WebRequestBase()
        //{

        //}

        public async Task<string> GetJsonObjectAsync(string jsonUrl)
        {
            HttpClient client = new HttpClient();
            var result = await client.GetStringAsync(jsonUrl);
            return result;

            //Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            //return null;
        }

    }
}
