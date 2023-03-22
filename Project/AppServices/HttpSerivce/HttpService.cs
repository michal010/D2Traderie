using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class HttpService
    {
        private HttpClient client;

        public HttpService()
        {
            Initialize();
        }

        void Initialize()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Chrome/79");
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await client.SendAsync(BuildGetRequestMessage(url));
        }

        private HttpRequestMessage BuildGetRequestMessage(string url)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            
            httpRequestMessage.RequestUri = new Uri(url);
            httpRequestMessage.Method = HttpMethod.Get;
            httpRequestMessage.Version = HttpVersion.Version20;

            return httpRequestMessage;
        }

    }
}
