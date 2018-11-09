﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Geta.Bring.Shipping.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Geta.Bring.Shipping
{
    public interface IQueryHandler
    {
        bool CanHandle(Type type);
        Task<EstimateResult<IEstimate>> FindEstimatesAsync(EstimateQuery query);
    }

    public abstract class QueryHandler<T> : IQueryHandler
        where T : IEstimate
    {
        protected QueryHandler(ShippingSettings settings, string methodName)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }

        public bool CanHandle(Type type)
        {
            return type == typeof(T);
        }

        public string MethodName { get; }

        public ShippingSettings Settings { get; }

        internal abstract T MapProduct(ProductResponse response);

        public async Task<EstimateResult<IEstimate>> FindEstimatesAsync(EstimateQuery query)
        {
            string jsonResponse;
            var requestUri = CreateRequestUri(query);
            var cacheKey = CreateCacheKey(requestUri);

            if (HttpRuntime.Cache.Get(cacheKey) is EstimateResult<IEstimate> cached)
                return await Task.FromResult(cached);

            using (var client = CreateClient())
            {
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
                    // http://developer.bring.com/additionalresources/errorhandling.html?from=shipping
                    return EstimateResult<IEstimate>.CreateFailure(rEx.Message);
                }
            }

            var response = JsonConvert.DeserializeObject<ShippingResponse>(jsonResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            if (!response.Product.Any())
            {
                //TODO use V2 api which returns actual error codes
                if (response.TraceMessages.Message.Any(m => m.StartsWith("Package exceed maximum measurements")))
                {
                    return EstimateResult<IEstimate>.CreateFailure(ShippingErrorCodes.MeasurementsExceeded);
                }
                
                if (response.TraceMessages.Message.Any(m => m.StartsWith("Product CARGO_GROUPAGE can not be sent between the given postal codes / countries")))
                {
                    return EstimateResult<IEstimate>.CreateFailure(ShippingErrorCodes.CannotDeliver);
                }
                
                return EstimateResult<IEstimate>.CreateFailure(ShippingErrorCodes.Unknown);
            }

            var estimates = response.Product.Select(MapProduct).Cast<IEstimate>().ToList();
            var result = EstimateResult<IEstimate>.CreateSuccess(estimates);

            HttpRuntime.Cache.Insert(cacheKey, result, null, DateTime.UtcNow.AddMinutes(2), Cache.NoSlidingExpiration);

            return result;
        }

        private string CreateCacheKey(Uri uri)
        {
            return string.Concat("EstimateResult", "-", typeof(T).Name, "-", uri.ToString());
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();

            if (Settings.Uid != null)
            {
                client.DefaultRequestHeaders.Add("X-MyBring-API-Uid", Settings.Uid);
            }

            if (Settings.Key != null)
            {
                client.DefaultRequestHeaders.Add("X-MyBring-API-Key", Settings.Key);
            }

            client.DefaultRequestHeaders.Add("X-Bring-Client-URL", Settings.ClientUri.ToString());
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private Uri CreateRequestUri(EstimateQuery query)
        {
            var uri = new Uri(Settings.EndpointUri, MethodName);
            var queryItems = HttpUtility.ParseQueryString(string.Empty); // This creates empty HttpValueCollection which creates query string on ToString
            queryItems.Add(query.Items);

            if (Settings.PublicId != null)
            {
                queryItems.Add("pid", Settings.PublicId);
            }

            var ub = new UriBuilder(uri) { Query = queryItems.ToString() };
            return ub.Uri;
        }
    }
}