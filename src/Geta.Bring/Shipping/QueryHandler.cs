using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Geta.Bring.Common;
using Geta.Bring.Shipping.Extensions;
using Geta.Bring.Shipping.Model;
using Geta.Bring.Shipping.Model.Errors;
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
        private static IContractResolver _contractResolver = new CamelCasePropertyNamesContractResolver();

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
            HttpResponseMessage responseMessage = null;
            string jsonResponse = null;

            var requestUri = CreateRequestUri(query);
            var cacheKey = CreateCacheKey(requestUri);

            if (HttpRuntime.Cache.Get(cacheKey) is EstimateResult<IEstimate> cached)
                return await Task.FromResult(cached);

            var client = HttpClientFactory.CreateClient(Settings);

            try
            {
                responseMessage = await client.GetAsync(requestUri).ConfigureAwait(false);
                jsonResponse = await responseMessage.Content.ReadAsStringAsync();

                responseMessage.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    var responseError = new ResponseError(responseMessage?.StatusCode ?? HttpStatusCode.InternalServerError);
                    return EstimateResult<IEstimate>.CreateFailure(responseError);
                }
            }

            var response = ReadResponse(jsonResponse);
            var errors = response.GetAllErrors().ToArray();
            if (errors.Any())
            {
                return EstimateResult<IEstimate>.CreateFailure(errors);
            }

            var products = response.GetAllProducts();
            var estimates = products.Select(MapProduct).Cast<IEstimate>().ToList();
            var result = EstimateResult<IEstimate>.CreateSuccess(estimates);

            HttpRuntime.Cache.Insert(cacheKey, result, null, DateTime.UtcNow.AddMinutes(2), Cache.NoSlidingExpiration);

            return result;
        }
        
        private ShippingResponse ReadResponse(string jsonResponse)
        {
            try
            {
                return JsonConvert.DeserializeObject<ShippingResponse>(jsonResponse, new JsonSerializerSettings
                {
                    ContractResolver = _contractResolver
                });
            }
            catch (JsonException)
            {
                return CreateErrorResponse(jsonResponse);
            }
        }

        private ShippingResponse CreateErrorResponse(string message)
        {
            var consignments = Enumerable.Empty<ConsignmentResponse>();
            var traceMessages =  Enumerable.Empty<string>();
            var fieldErrors = new [] { new FieldError("INTERNAL", message, "Response") };

            return new ShippingResponse(consignments, traceMessages, fieldErrors);
        }

        private string CreateCacheKey(Uri uri)
        {
            return string.Concat("EstimateResult", "-", typeof(T).Name, "-", Settings.Uid, "-", uri.ToString());
        }

        private Uri CreateRequestUri(EstimateQuery query)
        {
            var uri = new Uri(Settings.EndpointUri, MethodName);
            var queryItems = HttpUtility.ParseQueryString(string.Empty); // This creates empty HttpValueCollection which creates query string on ToString
            queryItems.Add(query.Items);

            var ub = new UriBuilder(uri) { Query = queryItems.ToString() };
            return ub.Uri;
        }
    }
}