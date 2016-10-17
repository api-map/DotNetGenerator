using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;
using Apimap.DotnetGenerator.Core.Model;
using Newtonsoft.Json.Linq;

namespace Apimap.DotnetGenerator.Core
{
    public class ApiMapGateway
    {
        private const string BaseAddress = "https://api-map.com/";
        private const string JsonMimeType = "application/json";

        public async Task<Mapping> GetMapping(int id, NetworkCredential credential)
        {
            var token = await GetToken(credential);
            if (token == null)
            {
                throw new SecurityException();
            }

            using (var client = CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.GetAsync("Mapping/GetFullMapping?Id=" + id + "&includeSchemaContent=true");
                if (response.IsSuccessStatusCode)
                {
                    Mapping m = await response.Content.ReadAsAsync<Mapping>();
                    return m;
                }

                // TODO - throw exception ?

                return null;
            }

        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();

            // TODO - configuration of proxies etc.

            client.BaseAddress = new Uri(BaseAddress);
            client.DefaultRequestHeaders.Accept.Clear();

            return client;
        }

        private async Task<string> GetToken(NetworkCredential credential)
        {
            using (var tokenClient = CreateClient())
            {
                tokenClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));
                var response = await tokenClient.PostAsJsonAsync("Account/Token/", new {credential.UserName, credential.Password });
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jobj = JObject.Parse(content);
                    var token = jobj.GetValue("access_token").Value<string>();
                    return token;
                }

                // TODO - throw security exception?

                return null;
            }
        }
    }
}
