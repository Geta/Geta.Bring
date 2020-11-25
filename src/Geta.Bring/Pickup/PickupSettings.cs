using Geta.Bring.Common.Model;
using System;

namespace Geta.Bring.Pickup
{
    /// <summary>
    /// Settings for <see cref="PickupClient" /> 
    /// </summary>
    public class PickupSettings : IAuthenticationSettings
    {
        // <summary>
        /// Initializes new instance of <see cref="PickupSettings"/> with default Bring Shipping Guide API endpoint: https://api.bring.com/pickuppoint/api/pickuppoint/
        /// </summary>
        /// <param name="clientUri">The URI of client Web site.</param>
        /// <param name="uid">required MyBring API User ID.</param>
        /// <param name="key">required MyBring API Key.</param>
        /// <remarks>Register at https://www.mybring.com/ for an api key</remarks>
        public PickupSettings(
            Uri clientUri,
            string uid = null,
            string key = null)
            : this(clientUri, new Uri("https://api.bring.com/pickuppoint/api/pickuppoint/"), uid, key) { }

        /// <summary>
        /// Initializes new instance of <see cref="PickupSettings"/>
        /// </summary>
        /// <param name="clientUri">The URI of client Web site.</param>
        /// <param name="endpointUri">The URI of Bring Shipping Guide API endpoint.</param>
        /// <param name="uid">required MyBring API User ID.</param>
        /// <param name="key">required MyBring API Key.</param>
        /// <remarks>Register at https://www.mybring.com/ for an api key</remarks>
        public PickupSettings(
            Uri clientUri,
            Uri endpointUri,
            string uid = null,
            string key = null)
        {
            EndpointUri = endpointUri ?? throw new ArgumentNullException(nameof(endpointUri));
            ClientUri = clientUri ?? throw new ArgumentNullException(nameof(clientUri));
            Uid = uid;
            Key = key;
        }

        public Uri ClientUri { get; }
        public Uri EndpointUri { get; }

        public string Uid { get; }
        public string Key { get; }
    }
}