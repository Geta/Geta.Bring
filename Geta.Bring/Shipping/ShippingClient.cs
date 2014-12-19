﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Geta.Bring.Shipping.Model;

namespace Geta.Bring.Shipping
{
    /// <summary>
    /// Bring Shipping Guide API client.
    /// </summary>
    public class ShippingClient : IShippingClient
    {
        public ShippingSettings Settings { get; private set; }

        /// <summary>
        /// Initializes new instance of <see cref="ShippingClient"/>.
        /// </summary>
        /// <param name="settings">Settings for <see cref="ShippingClient"/>.</param>
        public ShippingClient(ShippingSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            Settings = settings;
        }

        /// <summary>
        /// Finds Bring Shipping estimates based on estimate query.
        /// </summary>
        /// <typeparam name="T">Type of estimates <see cref="IEstimate"/>.</typeparam>
        /// <param name="query">Query parameters of type <see cref="EstimateQuery"/>.</param>
        /// <returns>Estimate find result of type <see cref="EstimateResult{T}"/>.</returns>
        public async Task<EstimateResult<T>> FindAsync<T>(EstimateQuery query)
            where T : IEstimate
        {
            foreach (var handler in Settings.QueryHandlers)
            {
                if (handler.CanHandle(typeof(T)))
                {
                    var estimate = await handler.FindEstimatesAsync(query).ConfigureAwait(false);
                    return new EstimateResult<T>(estimate.Estimates.Cast<T>());
                }
            }

            throw new Exception(string.Format("No matching query handler found for estimate type {0}", typeof(T).Name));
        }
    }
}