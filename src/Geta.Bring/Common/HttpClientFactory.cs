using Geta.Bring.Common.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Geta.Bring.Common
{
    public static class HttpClientFactory
    {
        private static IEqualityComparer<IAuthenticationSettings> _equalityComparer = new AuthenticationSettingsComparer();
        private static IDictionary<IAuthenticationSettings, HttpClient> _clients = new Dictionary<IAuthenticationSettings, HttpClient>(_equalityComparer);

        public static HttpClient CreateClient(IAuthenticationSettings settings)
        {
            if (_clients.ContainsKey(settings))
            {
                return _clients[settings];
            }

            var client = new HttpClient();

            if (settings.Uid != null)
            {
                client.DefaultRequestHeaders.Add("X-MyBring-API-Uid", settings.Uid);
            }

            if (settings.Key != null)
            {
                client.DefaultRequestHeaders.Add("X-MyBring-API-Key", settings.Key);
            }

            client.DefaultRequestHeaders.Add("X-Bring-Client-URL", settings.ClientUri.ToString());
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _clients.Add(settings, client);

            return client;
        }
    }
}
