using System;
using System.Linq;
using System.Text;
using Geta.Bring.EPi.Commerce.Model;
using Geta.Bring.Shipping.Model;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Geta.Bring.Shipping;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Bring.EPi.Commerce.Factories;

namespace Geta.Bring.EPi.Commerce
{
    public class BringShippingGateway : IShippingGateway
    {
        private readonly IShippingClient _shippingClient;
        private readonly IEstimateQueryFactory _estimateQueryFactory;
        private readonly IEstimateSettingsFactory _estimateSettingsFactory;

        public BringShippingGateway()
        {
            _shippingClient = ServiceLocator.Current.GetInstance<IShippingClient>();
            _estimateQueryFactory = ServiceLocator.Current.GetInstance<IEstimateQueryFactory>();
            _estimateSettingsFactory = ServiceLocator.Current.GetInstance<IEstimateSettingsFactory>();
        }

        public ShippingRate GetRate(Guid methodId, Shipment shipment, ref string message)
        {
            if (shipment == null)
            {
                return null;
            }

            var shippingMethod = ShippingManager.GetShippingMethod(methodId);
            if (shippingMethod == null || shippingMethod.ShippingMethod.Count <= 0)
            {
                message = string.Format(ErrorMessages.ShippingMethodCouldNotBeLoaded, methodId);
                return null;
            }
            
            var shippingMethodRow = shippingMethod.ShippingMethod[0];
            if (shipment.LineItems.Count == 0)
            {
                message = ErrorMessages.ShipmentContainsNoLineItems;
                return CreateBaseShippingRate(methodId, shippingMethodRow);
            }

            if (shipment.Parent == null || shipment.Parent.Parent == null)
            {
                message = ErrorMessages.OrderFormOrOrderGroupNotFound;
                return null;
            }

            var orderAddress =
                shipment.Parent
                    .Parent
                    .OrderAddresses
                    .FirstOrDefault(address => address.Name == shipment.ShippingAddressId);
            if (orderAddress == null)
            {
                message = ErrorMessages.ShipmentAddressNotFound;
                return CreateBaseShippingRate(methodId, shippingMethodRow);
            }

            var query = _estimateQueryFactory.BuildEstimateQuery(shipment, methodId);
            var estimate = _shippingClient.FindAsync<ShipmentEstimate>(query).Result;
            if (estimate.Success && estimate.Estimates.Any())
            {
                return CreateShippingRate(methodId, shippingMethod, estimate);
            }

            message = GetErrorMessage(estimate);

            if (estimate.Success)
                return CreateBaseShippingRate(methodId, shippingMethodRow);

            return null;
        }

        private static ShippingRate CreateBaseShippingRate(Guid shippingMethodId, ShippingMethodDto.ShippingMethodRow shippingMethodRow)
        {
            return new ShippingRate(
                shippingMethodId, 
                shippingMethodRow.DisplayName, 
                new Money(shippingMethodRow.BasePrice, 
                new Currency(shippingMethodRow.Currency)));
        }

        private ShippingRate CreateShippingRate(
            Guid methodId,
            ShippingMethodDto shippingMethod,
            EstimateResult<ShipmentEstimate> result)
        {
            var settings = _estimateSettingsFactory.CreateFrom(shippingMethod);
            var estimate = result.Estimates.First();

            var usesAdditionalServices = settings.AdditionalServices.Any();
            var packagePrice = estimate.Price.NetPrice ?? estimate.Price.ListPrice;

            var priceWithAdditionalServices = !settings.PriceExclTax
                ? (decimal) packagePrice.PriceWithAdditionalServices.AmountWithVAT
                : (decimal) packagePrice.PriceWithAdditionalServices.AmountWithoutVAT;

            var priceWithoutAdditionalServices = !settings.PriceExclTax
                ? (decimal) packagePrice.PriceWithoutAdditionalServices.AmountWithVAT
                : (decimal) packagePrice.PriceWithoutAdditionalServices.AmountWithoutVAT;

            var amount = AdjustPrice(shippingMethod, settings, usesAdditionalServices ? priceWithAdditionalServices :
                                                                                        priceWithoutAdditionalServices);

            var moneyAmount = new Money(
                amount,
                new Currency(packagePrice.CurrencyCode));

            return new BringShippingRate(
                methodId,
                estimate.GuiInformation.DisplayName,
                estimate.GuiInformation.MainDisplayCategory,
                estimate.GuiInformation.SubDisplayCategory,
                estimate.GuiInformation.DescriptionText,
                estimate.GuiInformation.HelpText,
                estimate.GuiInformation.Tip,
                estimate.ExpectedDelivery.ExpectedDeliveryDate,
                moneyAmount);
        }

        private static decimal AdjustPrice(ShippingMethodDto shippingMethod, IEstimateSettings settings, decimal price)
        {
            if (settings.PriceAdjustmentPercent > 0)
            {
                var pricePart = price * (settings.PriceAdjustmentPercent / 100.0m);
                price += settings.PriceAdjustmentIsAddition ? pricePart : -pricePart;
            }

            var shippingMethodRow = shippingMethod.ShippingMethod[0];
            var amount = shippingMethodRow.BasePrice + price;

            if (settings.PriceRounding)
            {
                return Math.Round(amount, MidpointRounding.AwayFromZero);
            }

            return amount;
        }

        private static string GetErrorMessage(EstimateResult<ShipmentEstimate> result)
        {
            var builder = new StringBuilder();

            foreach (var error in result.Errors)
            {
                builder.Append(error);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        internal static class ParameterNames
        {
            public const string BringProductId = "BringProductId";
            public const string PostingAtPostOffice = "PostingAtPostOffice";
            public const string PostalCodeFrom = "PostalCodeFrom";
            public const string CountryFrom = "CountryFrom";
            public const string Edi = "EDI";
            public const string AdditionalServices = "AdditionalServices";
            public const string PriceRounding = "PriceRounding";
            public const string PriceAdjustmentOperator = "PriceAdjustmentOperator";
            public const string PriceAdjustmentPercent = "PriceAdjustmentPercent";
            public const string BringCustomerNumber = "BringCustomerNumber";
            public const string PriceExclTax = "PriceExclTax";
        }

        internal static class ErrorMessages
        {
            public const string ShipmentAddressNotFound =
                "The shipment address not found in order group.";

            public const string OrderFormOrOrderGroupNotFound =
                "The order form or order group not found for shipment.";

            public const string ShippingMethodCouldNotBeLoaded =
                "The shipping method could not be loaded by '{0}' id.";

            public const string ShipmentContainsNoLineItems =
                "The shipment doesn't contain any line items.";
        }
    }
}