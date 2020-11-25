using System;
using System.Linq;
using FluentAssertions;
using Geta.Bring.Shipping.Model;
using Geta.Bring.Shipping.Model.Errors;
using Newtonsoft.Json;
using Xunit;

namespace Tests.Integration.Shipping
{
    public class MultipleProductsResponseDeserializationTests
    {
        [Fact]
        public void it_can_be_deserialized()
        {
            var expected = new ShippingResponse(
                new []
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
                            new Price(98.00, 24.50, 122.5),
                            new Price(98.00, 24.50, 122.5),
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
                        ),
                    new ProductResponse(
                        "PA_DOREN",
                        "1736",
                        new GuiInformation(
                            41,
                            "Pakke",
                            "Til private",
                            "På Døren",
                            "På Døren",
                            "Pakken kan spores og leveres hjem til deg mellom kl. 08-17 eller 17-21 avhengig av ditt postnummer. Sjåføren ringer 30-60 min. før ankomst ved levering på kveldstid.",
                            "På Døren leveres hjem til mottaker mellom kl. 08-17 eller 17-21 avhengig av mottakers postnummer. Mottaker varsles i god tid om forventet utleveringsdag via SMS eller e-post, i tillegg til nytt varsel når sendingen er lastet på bil for utkjøring samme dag. Mottaker kan gi Posten fullmakt til at pakken settes igjen ved døren eller et angitt sted hvis mottaker ikke er hjemme. Sjåføren ringer mottaker 30-60 minutter før ankomst ved levering på kveldstid. Mottaker kan endre leveringsdag når pakken spores (gjelder ikke lokalpakker). Dersom sendingen ikke kan leveres, blir den sendt til mottakers lokale hentested (postkontor eller Post i Butikk). Sendingen kan spores ved hjelp av sporingsnummeret.",
                            null,
                            "Dør",
                            35
                        ),
                        new PackagePrices(new PackagePrice(
                            "NOK",
                            new Price(114.00, 28.50, 142.50),
                            new Price(114.00, 28.50, 142.50),
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
                        ),
                        new ProductResponse("SERVICEPAKKE", "1202", null, null, null, new []
                        {
                            new ProductError("INVALID_MEASUREMENTS", "Package exceed maximum measurements for product SERVICEPAKKE ")
                        })
                    })
                },
                new[]
                {
                    "Added fee 'brev-varsling' (NOK 13.00) to base price of SERVICEPAKKE since request did not have additional service 'eVarsling' specified."
                },                
                null);

            var actual = JsonConvert.DeserializeObject<ShippingResponse>(MultipleProductsSuccessJsonResponse);

            expected.Should().BeEquivalentTo(actual);
        }

        private const string MultipleProductsSuccessJsonResponse = @"
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
        },
        {
          ""id"": ""PA_DOREN"",
          ""productionCode"": ""1736"",
          ""guiInformation"": {
            ""sortOrder"": ""41"",
            ""mainDisplayCategory"": ""Pakke"",
            ""subDisplayCategory"": ""Til private"",
            ""displayName"": ""På Døren"",
            ""productName"": ""På Døren"",
            ""descriptionText"": ""Pakken kan spores og leveres hjem til deg mellom kl. 08-17 eller 17-21 avhengig av ditt postnummer. Sjåføren ringer 30-60 min. før ankomst ved levering på kveldstid."",
            ""helpText"": ""På Døren leveres hjem til mottaker mellom kl. 08-17 eller 17-21 avhengig av mottakers postnummer. Mottaker varsles i god tid om forventet utleveringsdag via SMS eller e-post, i tillegg til nytt varsel når sendingen er lastet på bil for utkjøring samme dag. Mottaker kan gi Posten fullmakt til at pakken settes igjen ved døren eller et angitt sted hvis mottaker ikke er hjemme. Sjåføren ringer mottaker 30-60 minutter før ankomst ved levering på kveldstid. Mottaker kan endre leveringsdag når pakken spores (gjelder ikke lokalpakker). Dersom sendingen ikke kan leveres, blir den sendt til mottakers lokale hentested (postkontor eller Post i Butikk). Sendingen kan spores ved hjelp av sporingsnummeret."",
            ""shortName"": ""På Døren"",
            ""productURL"": ""http://www.bring.no/sende/pakker/private-i-norge/hjemlevering"",
            ""deliveryType"": ""Dør"",
            ""maxWeightInKgs"": ""35""
          },
          ""price"": {
            ""listPrice"": {
              ""priceWithoutAdditionalServices"": {
                ""amountWithoutVAT"": ""114.00"",
                ""vat"": ""28.50"",
                ""amountWithVAT"": ""142.50""
              },
              ""priceWithAdditionalServices"": {
                ""amountWithoutVAT"": ""114.00"",
                ""vat"": ""28.50"",
                ""amountWithVAT"": ""142.50""
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
              ""day"": ""13"",
              ""timeSlots"": [
                {
                  ""startTime"": {
                    ""hour"": ""8"",
                    ""minute"": ""0""
                  },
                  ""endTime"": {
                    ""hour"": ""17"",
                    ""minute"": ""0""
                  }
                }
              ]
            },
            ""alternativeDeliveryDates"": []
          }
        },
        {
            ""id"": ""SERVICEPAKKE"",
            ""productionCode"": ""1202"",
            ""errors"": [
                {
                    ""code"": ""INVALID_MEASUREMENTS"",
                    ""description"": ""Package exceed maximum measurements for product SERVICEPAKKE ""
                }
            ]
        }
      ]
    }
  ]
}
";
    }
}