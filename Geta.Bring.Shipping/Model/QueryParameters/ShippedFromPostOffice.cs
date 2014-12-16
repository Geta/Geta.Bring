using System.Collections.Specialized;

namespace Geta.Bring.Shipping.Model.QueryParameters
{
    /// <summary>
    /// Query parameter to describe if shipped from post office.
    /// </summary>
    public class ShippedFromPostOffice : IQueryParameter
    {
        /// <summary>
        /// Initializes new instance of <see cref="ShippedFromPostOffice"/>.
        /// </summary>
        /// <param name="shippedFromPostOffice">Parameter value if shipped from post office.</param>
        public ShippedFromPostOffice(bool shippedFromPostOffice)
        {
            Items = new NameValueCollection
            {
                {"postingAtPostoffice", shippedFromPostOffice.ToString().ToLowerInvariant()}
            };
        }
        public NameValueCollection Items { get; private set; }
    }
}