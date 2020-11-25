using System;
using System.Collections.Generic;
using System.Linq;
using Geta.Bring.Common.Model;
using Geta.Bring.Shipping.Model;

namespace Geta.Bring.Shipping
{
    /// <summary>
    /// Settings for <see cref="ShippingClient" />
    /// </summary>
    public class ShippingSettings : IAuthenticationSettings
    {
        /// <summary>
        /// Initializes new instance of <see cref="ShippingSettings"/> with default Bring Shipping Guide API endpoint: https://developer.bring.com/api/products/
        /// </summary>
        /// <param name="clientUri">The URI of client Web site.</param>
        /// <param name="uid">required MyBring API User ID.</param>
        /// <param name="key">required MyBring API Key.</param>
        /// <param name="queryHandlers">Additional <see cref="IQueryHandler"/>s. Allows to register additional query handlers for new API endpoints in future.</param>
        /// <remarks>Register at https://www.mybring.com/ for an api key</remarks>
        public ShippingSettings(
            Uri clientUri,
            string uid,
            string key,
            IEnumerable<IQueryHandler> queryHandlers = null)
            : this(clientUri, new Uri("https://api.bring.com/shippingguide/v2/products/"), uid, key, queryHandlers) { }

        /// <summary>
        /// Initializes new instance of <see cref="ShippingSettings"/>
        /// </summary>
        /// <param name="clientUri">The URI of client Web site.</param>
        /// <param name="endpointUri">The URI of Bring Shipping Guide API endpoint.</param>
        /// <param name="uid">required MyBring API User ID.</param>
        /// <param name="key">required MyBring API Key.</param>
        /// <param name="queryHandlers">Additional <see cref="IQueryHandler"/>s. Allows to register additional query handlers for new API endpoints in future.</param>
        /// <remarks>Register at https://www.mybring.com/ for an api key</remarks>
        public ShippingSettings(
            Uri clientUri,
            Uri endpointUri,
            string uid = null,
            string key = null,
            IEnumerable<IQueryHandler> queryHandlers = null)
        {
            EndpointUri = endpointUri ?? throw new ArgumentNullException(nameof(endpointUri));
            ClientUri = clientUri ?? throw new ArgumentNullException(nameof(clientUri));
            Uid = uid;
            Key = key;

            var defaultHandlers = CreateDefaultQueryHandlers(this);
            QueryHandlers = defaultHandlers.Concat(queryHandlers ?? Enumerable.Empty<IQueryHandler>());
        }

        public Uri ClientUri { get; }
        public Uri EndpointUri { get; }

        public string Uid { get; }
        public string Key { get; }

        public IEnumerable<IQueryHandler> QueryHandlers { get; }

        private static IEnumerable<IQueryHandler> CreateDefaultQueryHandlers(ShippingSettings settings)
        {
            yield return new ShipmentEstimateQueryHandler(settings);
            yield return new PriceEstimateQueryHandler(settings);
            yield return new DeliveryEstimateQueryHandler(settings);
        }
    }
}