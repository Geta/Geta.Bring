using Geta.Bring.Shipping.Infrastructure;
using Newtonsoft.Json;

namespace Geta.Bring.Shipping.Model
{
    /// <summary>
    /// GUI information.
    /// </summary>
    public class GuiInformation
    {
        public GuiInformation(
            int sortOrder, 
            string mainDisplayCategory, 
            string subDisplayCategory, 
            string displayName, 
            string productName, 
            string descriptionText, 
            string helpText, 
            string tip, 
            string deliveryType,
            int maxWeightInKgs)
        {
            MaxWeightInKgs = maxWeightInKgs;
            DeliveryType = deliveryType;
            Tip = tip;
            HelpText = helpText;
            DescriptionText = descriptionText;
            ProductName = productName;
            DisplayName = displayName;
            SubDisplayCategory = subDisplayCategory;
            MainDisplayCategory = mainDisplayCategory;
            SortOrder = sortOrder;
        }

        /// <summary>
        /// Order number of the estimation option.
        /// </summary>
        public int SortOrder { get; }

        /// <summary>
        /// The name of main display category.
        /// </summary>
        public string MainDisplayCategory { get; }

        /// <summary>
        /// The name of sub-display category.
        /// </summary>
        public string SubDisplayCategory { get; }

        /// <summary>
        /// Display name of estimation option.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Product name.
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// Description text.
        /// </summary>
        public string DescriptionText { get; }

        /// <summary>
        /// Help text.
        /// </summary>
        public string HelpText { get; }

        /// <summary>
        /// Tip.
        /// </summary>
        public string Tip { get; }

        /// <summary>
        /// Type of delivery.
        /// </summary>
        public string DeliveryType { get; }

        /// <summary>
        /// Max weight in kilograms.
        /// </summary>
        [JsonConverter(typeof(DefaultingPropertyConverter<int>))]
        public int MaxWeightInKgs { get; }
    }
}