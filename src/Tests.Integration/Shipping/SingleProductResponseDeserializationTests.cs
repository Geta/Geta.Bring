using System;
using System.Linq;
using FluentAssertions;
using Geta.Bring.Shipping.Model;
using Geta.Bring.Shipping.Model.Errors;
using Newtonsoft.Json;
using Xunit;

namespace Tests.Integration.Shipping
{
    public class SingleProductResponseDeserializationTests
    {
        [Fact]
        public void it_can_be_deserialized()
        {
            var expected = new ShippingResponse(
                new[]
                {
                    new ConsignmentResponse( new [] { 
                        new ProductResponse(
                            "SERVICEPAKKE",
                            "1202",
                            new GuiInformation(
                                11,
                                "Pakke",
                                "Til private",
                                "Klimanøytral Servicepakke",
                                "Klimanøytral Servicepakke",
                                "Pakken kan spores og utleveres på ditt lokale hentested.",
                                "Klimanøytral Servicepakke leveres til mottakers lokale hentested (postkontor eller Post i Butikk). Mottaker kan velge å hente sendingen på et annet hentested enn sitt lokale. Mottaker varsles om at sendingen er ankommet via SMS, e-post eller hentemelding i postkassen. Sendingen kan spores ved hjelp av sporingsnummeret.",
                                null,
                                "Hentested",
                                35
                            ),
                            new PackagePrices(new PackagePrice(
                                "NOK",
                                new Price(98.00, 24.50, 122.50),
                                new Price(98.00, 24.50, 122.50),
                                Enumerable.Empty<AgreementPrice>()
                            )),
                            new ExpectedDelivery(
                                "1",
                                null,
                                "13.11.2018",
                                null,
                                new DateTime(2018, 11, 13),
                                null
                            ),
                            Enumerable.Empty<ProductError>()
                        )
                    })
                },
                new[]
                {
                    "Added fee 'brev-varsling' (NOK 13.00) to base price of SERVICEPAKKE since request did not have additional service 'eVarsling' specified."
                },
                null);

            var actual = JsonConvert.DeserializeObject<ShippingResponse>(SingleProductSuccessJsonResponse);

            expected.Should().BeEquivalentTo(actual);
        }

        private const string SingleProductSuccessJsonResponse = @"
{
  ""traceMessages"": [
    ""Added fee 'brev-varsling' (NOK 13.00) to base price of SERVICEPAKKE since request did not have additional service 'eVarsling' specified.""
  ],
  ""consignments"": [
    {
      ""products"": [
        {
          ""id"": ""SERVICEPAKKE"",
          ""productionCode"": ""1202"",
          ""guiInformation"": {
            ""sortOrder"": ""11"",
            ""mainDisplayCategory"": ""Pakke"",
            ""subDisplayCategory"": ""Til private"",
            ""displayName"": ""Klimanøytral Servicepakke"",
            ""productName"": ""Klimanøytral Servicepakke"",
            ""descriptionText"": ""Pakken kan spores og utleveres på ditt lokale hentested."",
            ""helpText"": ""Klimanøytral Servicepakke leveres til mottakers lokale hentested (postkontor eller Post i Butikk). Mottaker kan velge å hente sendingen på et annet hentested enn sitt lokale. Mottaker varsles om at sendingen er ankommet via SMS, e-post eller hentemelding i postkassen. Sendingen kan spores ved hjelp av sporingsnummeret."",
            ""shortName"": ""Klimanøytral Servicepakke"",
            ""productURL"": ""http://www.bring.no/sende/pakker/private-i-norge/hentes-pa-posten"",
            ""deliveryType"": ""Hentested"",
            ""maxWeightInKgs"": ""35""
          },
          ""price"": {
            ""listPrice"": {
              ""priceWithoutAdditionalServices"": {
                ""amountWithoutVAT"": ""98.00"",
                ""vat"": ""24.50"",
                ""amountWithVAT"": ""122.50""
              },
              ""priceWithAdditionalServices"": {
                ""amountWithoutVAT"": ""98.00"",
                ""vat"": ""24.50"",
                ""amountWithVAT"": ""122.50""
              },
              ""currencyCode"": ""NOK""
            }
          },
          ""expectedDelivery"": {
            ""workingDays"": ""1"",
            ""userMessage"": """",
            ""formattedExpectedDeliveryDate"": ""13.11.2018"",
            ""expectedDeliveryDate"": {
              ""year"": ""2018"",
              ""month"": ""11"",
              ""day"": ""13""
            },
            ""alternativeDeliveryDates"": []
          }
        }
      ]
    }
  ]
}
"; 
    }
}