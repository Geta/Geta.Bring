using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Geta.Bring.Common;
using Geta.Bring.Pickup.Model;
using Newtonsoft.Json;

namespace Geta.Bring.Pickup
{
    /// <summary>
    /// Bring Pickup Point API.
    /// </summary>
    public class PickupClient : IPickupClient
    {
        public PickupSettings Settings { get; }

        /// <summary>
        /// Initializes new instance of <see cref="PickupClient"/>.
        /// </summary>
        /// <param name="settings">Settings for <see cref="PickupClient"/>.</param>
        public PickupClient(PickupSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Finds pickup points based on country and zipcode.
        /// </summary>
        /// <param name="query">Query parameters of type <see cref="PickupZipCodeQuery"/>.</param>
        /// <returns>A list of matching <see cref="PickupPoint"/></returns>
        public async Task<PickupResult> FindByZipCode(PickupZipCodeQuery query)
        {
            var relativePath = $"postalCode/{query.ZipCode}.json";

            return await GetResponseAsync(relativePath, query);
        }

        /// <summary>
        /// Finds pickup points based on country and geolocation.
        /// </summary>
        /// <param name="query">Query parameters of type <see cref="PickupLocationQuery"/>.</param>
        /// <returns>A list of matching <see cref="PickupPoint"/></returns>
        public async Task<PickupResult> FindByLocation(PickupLocationQuery query)
        {
            var relativePath = string.Format(CultureInfo.InvariantCulture, "location/{0},{1}.json", query.Latitude, query.Longitude);

            return await GetResponseAsync(relativePath, query);
        }

        /// <summary>
        /// Finds pickup points based on country and Bring id.
        /// </summary>
        /// <param name="countryCode">Country code, ISO 2 letter, allowed values: NO, DK, SE, FI</param>
        /// <param name="id">Identifier in Bring</param>
        /// <returns>A list of matching <see cref="PickupPoint"/></returns>
        public async Task<PickupResult> FindById(string countryCode, string id)
        {
            var query = new PickupQuery(countryCode);
            var relativePath = $"id/{id}.json";

            return await GetResponseAsync(relativePath, query);
        }

        /// <summary>
        /// Lists all pickup points for a country.
        /// </summary>
        /// <param name="countryCode">Country code, ISO 2 letter, allowed values: NO, DK, SE, FI</param>
        /// <returns>A list of all <see cref="PickupPoint"/></returns>
        public async Task<PickupResult> All(string countryCode)
        {
            var query = new PickupQuery(countryCode);
            return await GetResponseAsync("all.json", query);
        }

        private async Task<PickupResult> GetResponseAsync(string relativePath, PickupQuery query)
        {
            string jsonResponse;
            var requestUri = CreateRequestUri(relativePath, query);
            var cacheKey = CreateCacheKey(requestUri);

            var cached = HttpRuntime.Cache.Get(cacheKey) as PickupResult;

            if (cached != null)
                return await Task.FromResult(cached);

            var client = HttpClientFactory.CreateClient(Settings);

            try
            {
                jsonResponse = await client.GetStringAsync(requestUri).ConfigureAwait(false);
            }
            catch (HttpRequestException rEx)
            {
                // TODO: parse errors from here and create strongly typed error messages
                // Could be object with Code, Description and HTML (full message received from Bring)
                // Some errors are validation errors like - invalid postal code, invalid city etc., but some are exceptions.
                // Wrap and return only validation errors, others throw further.
                // Wrap configuration errors and throw them with details, but other errors throw as is.
                return PickupResult.CreateFailure(rEx.Message);
            }

            try
            {
                var response = JsonConvert.DeserializeObject<PickupResponse>(jsonResponse);
                var result = PickupResult.CreateSuccess(response.PickupPoints);

                HttpRuntime.Cache.Insert(cacheKey, result, null, DateTime.UtcNow.AddMinutes(1), Cache.NoSlidingExpiration);

                return result;
            }
            catch (JsonException jEx)
            {
                return PickupResult.CreateFailure(jEx.Message);
            }
        }

        private string CreateCacheKey(Uri uri)
        {
            return string.Concat("PickupResult", "-", uri.ToString());
        }

        private Uri CreateRequestUri(string relativePath, PickupQuery query)
        {
            var uri = new Uri(Settings.EndpointUri, string.Concat(query.CountryCode, "/", relativePath));

            var queryItems = HttpUtility.ParseQueryString(string.Empty); // This creates empty HttpValueCollection which creates query string on ToString
            queryItems.Add(query.Items);

            var ub = new UriBuilder(uri) { Query = queryItems.ToString() };
            return ub.Uri;
        }
    }
}