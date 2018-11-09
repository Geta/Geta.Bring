﻿using System;
using System.Linq;
using System.Text;
using Geta.Bring.EPi.Commerce.Extensions;
using Geta.Bring.EPi.Commerce.Model;
using Geta.Bring.Shipping.Model;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Geta.Bring.Shipping;
using EPiServer.Commerce.Order;
using Geta.Bring.EPi.Commerce.Factories;

namespace Geta.Bring.EPi.Commerce
{
    public class BringShippingGateway : IShippingPlugin
    {
        private readonly IShippingClient _shippingClient;
        private readonly IEstimateQueryFactory _estimateQueryFactory;

        public BringShippingGateway(IShippingClient shippingClient, IEstimateQueryFactory estimateQueryFactory)
        {
            _shippingClient = shippingClient;
            _estimateQueryFactory = estimateQueryFactory;
        }

        public ShippingRate GetRate(IMarket market, Guid methodId, IShipment shipment, ref string message)
        {
            return GetRate(methodId, shipment, ref message);
        }

        public ShippingRate GetRate(Guid methodId, IShipment shipment, ref string message)
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

            if (shipment.ShippingAddress == null)
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

            message = estimate.ErrorMessages
                .Aggregate(new StringBuilder(), (sb, msg) =>
                {
                    sb.Append(msg);
                    sb.AppendLine();
                    return sb;
                }).ToString();

            return CreateBaseShippingRate(methodId, shippingMethodRow);
        }

        private static ShippingRate CreateBaseShippingRate(Guid shippingMethodId,ShippingMethodDto.ShippingMethodRow shippingMethodRow)
        {
            return new ShippingRate(
                shippingMethodId, 
                shippingMethodRow.DisplayName, 
                new Money(shippingMethodRow.BasePrice, 
                new Currency(shippingMethodRow.Currency)));
        }

        private static ShippingRate CreateShippingRate(
            Guid methodId,
            ShippingMethodDto shippingMethod,
            EstimateResult<ShipmentEstimate> result)
        {
            var estimate = result.Estimates.First();
            var priceExclTax = shippingMethod.GetShippingMethodParameterValue(ParameterNames.PriceExclTax) == "True";
            var usesAdditionalServices = !string.IsNullOrEmpty(shippingMethod.GetShippingMethodParameterValue(ParameterNames.AdditionalServices));

            var packagePrice = estimate.Price.NetPrice ?? estimate.Price.ListPrice;

            var priceWithAdditionalServices = !priceExclTax
                ? (decimal) packagePrice.PackagePriceWithAdditionalServices.AmountWithVAT
                : (decimal) packagePrice.PackagePriceWithAdditionalServices.AmountWithoutVAT;

            var priceWithoutAdditionalServices = !priceExclTax
                ? (decimal) packagePrice.PackagePriceWithoutAdditionalServices.AmountWithVAT
                : (decimal) packagePrice.PackagePriceWithoutAdditionalServices.AmountWithoutVAT;

            var amount = AdjustPrice(shippingMethod, usesAdditionalServices ? priceWithAdditionalServices :
                                                                              priceWithoutAdditionalServices);

            var moneyAmount = new Money(
                amount,
                new Currency(packagePrice.CurrencyIdentificationCode));

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

        private static decimal AdjustPrice(ShippingMethodDto shippingMethod, decimal price)
        {
            var shippingMethodRow = shippingMethod.ShippingMethod[0];
            var priceRoundingParameter = shippingMethod.GetShippingMethodParameterValue(ParameterNames.PriceRounding, "false");

            var priceAdjustmentPercentParameter = shippingMethod.GetShippingMethodParameterValue(ParameterNames.PriceAdjustmentPercent, "0");
            int.TryParse(priceAdjustmentPercentParameter, out var priceAdjustmentPercent);

            if (priceAdjustmentPercent > 0)
            {
                var priceAdjustmentOperatorParameter = shippingMethod.GetShippingMethodParameterValue(ParameterNames.PriceAdjustmentOperator, "true");
                bool.TryParse(priceAdjustmentOperatorParameter, out var priceAdjustmentAdd);

                //todo: Test this price adjustment
                var pricePart = price * (priceAdjustmentPercent / 100.0m);
                price += priceAdjustmentAdd ? pricePart : -pricePart;
            }

            var amount = shippingMethodRow.BasePrice + price;

            if (bool.TryParse(priceRoundingParameter, out var priceRounding) && priceRounding)
            {
                return Math.Round(amount, MidpointRounding.AwayFromZero);
            }

            return amount;
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