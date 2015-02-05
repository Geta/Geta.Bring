﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Geta.Bring.Shipping.Model;
using Newtonsoft.Json;

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
            if (settings == null) throw new ArgumentNullException("settings");
            if (methodName == null) throw new ArgumentNullException("methodName");
            Settings = settings;
            MethodName = methodName;
        }

        public bool CanHandle(Type type)
        {
            return type == typeof(T);
        }

        public string MethodName { get; private set; }

        public ShippingSettings Settings { get; private set; }

        internal abstract T MapProduct(ProductResponse response);

        public async Task<EstimateResult<IEstimate>> FindEstimatesAsync(EstimateQuery query)
        {
            string jsonResponse;
            var requestUri = CreateRequestUri(query);
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
            var response = JsonConvert.DeserializeObject<ShippingResponse>(jsonResponse);
            var estimates = response.Product.Select(MapProduct).Cast<IEstimate>();
            return EstimateResult<IEstimate>.CreateSuccess(estimates);
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Bring-Client-URL", Settings.ClientUri.ToString());
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