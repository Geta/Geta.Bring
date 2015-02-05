﻿using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Web.Mvc;
using Geta.Bring.Shipping;
using Geta.Bring.Shipping.Model;

namespace Geta.Bring.Sample
{
    public class BringRatesSampleController : BlockController<BringRatesSampleBlock>
    {
        private const string ViewPath = "~/Views/Shared/Blocks/BringRatesSample.cshtml";

        public override ActionResult Index(BringRatesSampleBlock currentContent)
        {
            return PartialView(ViewPath, new BringRatesSampleBlockView());
        }

        [HttpPost]
        public ActionResult Post(BringRatesSampleBlockView formData)
        {
            Validate(formData);
            if (!ModelState.IsValid)
            {
                return PartialView(ViewPath, formData);
            }

            var model = Search(formData);
            return PartialView(ViewPath, model);
        }

        private void Validate(BringRatesSampleBlockView formData)
        {
            ValidatePackageSize(formData);
        }

        private void ValidatePackageSize(BringRatesSampleBlockView formData)
        {
            var hasWeight = formData.Weight.HasValue;
            var hasDimensions = formData.Width.HasValue && formData.Height.HasValue && formData.Length.HasValue;
            var hasVolume = formData.Volume.HasValue;
            var hasPackageSize = hasWeight || hasDimensions || hasVolume;

            if (!hasPackageSize)
            {
                ModelState.AddModelError("", "Ingen pakkestørrelse gitt. Vennligst oppgi vekt, volum eller dimensjoner.");
            }
        }

        private BringRatesSampleBlockView Search(BringRatesSampleBlockView formData)
        {
            var settings = new ShippingSettings(GetBaseUri());
            var client = new ShippingClient(settings);

            var shipmentLeg = ParameterMapper.GetShipmentLeg(formData);
            var packageSize = ParameterMapper.GetPackageSize(formData);
            var additionalParameters = ParameterMapper.GetAdditionalParameters(formData);

            var query = new EstimateQuery(shipmentLeg, packageSize, additionalParameters);
            var result = client.FindAsync<ShipmentEstimate>(query).Result;

            var estimateGroups = result.Estimates
                .GroupBy(x => x.GuiInformation.MainDisplayCategory)
                .Select(x => new BringRatesSampleBlockView.EstimateGroup(x.Key, x));

            var model = new BringRatesSampleBlockView(estimateGroups);
            return model;
        }

        private Uri GetBaseUri()
        {
            if (Request.Url == null)
            {
                throw new Exception("Request.Url is null.");
            }

            return new Uri(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")));
        }
    }
}