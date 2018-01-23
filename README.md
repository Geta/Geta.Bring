# Bring .NET API and EPiServer Commerce module

![](http://tc.geta.no/app/rest/builds/buildType:(id:TeamFrederik_GetaBring_GetaBringDebug)/statusIcon)

## Description

This is a .NET Bring API library and module to integrate with EPiServer Commerce. Currently it supports these Bring APIs:
- Shipping Guide API
- Tracking API
- Booking API

## How to install?

Start by installing NuGet packages in you project.


## Package maintainer

https://github.com/svenrog




### Bring API

    Install-Package Geta.Bring

### EPiServer Commerce module

For EPiServer Commerce you have to install two packages. First package - *Geta.Bring.EPi.Commerce*, should be installed into your EPiServer Commerce Web site:

    Install-Package Geta.Bring.EPi.Commerce

Second package - *Geta.Bring.EPi.Commerce.Manager*, should be installed into your EPiServer Commerce Manager Web site:

    Install-Package Geta.Bring.EPi.Commerce.Manager

## How to use it?

### Shipping Guide API

To start using Shipping Guide API you have to create new *ShippingClient* with provided settings.

    var settings = new ShippingSettings(new Uri("http://test.localtest.me"));
    IShippingClient client = new ShippingClient(settings);

*ShippingSettings* requires at least one parameter - *clientUri*, which is the base URI of your Web site.

To find estimated delivery options you have to call *FindAsync* method with query parameters and type of the estimate.
    
    var query = new EstimateQuery(
        new ShipmentLeg("0484", "5600"),
        PackageSize.InGrams(2500));
    var result = await client.FindAsync<ShipmentEstimate>(query);

There are three types of estimates:
- *DeliveryEstimate* - returns estimated delivery options,
- *PriceEstimate* - returns estimated price options,
- *ShipmentEstimate* - returns estimated delivery, price and GUI information options.

Query requires at least two parameters - *ShipmentLeg* and *PackageSize* parameters. Additionaly it can have more parameters according to your needs. Full list of parameters:
- *ShipmentLeg* - query parameter to describe shipment source and destination by providing postal codes and/or country codes,
- *PackageSize* - query parameter to describe package size in grams, by dimensions or by volume,
- *AdditionalServices* - query parameter to describe required additional services. List of available services: http://developer.bring.com/additionalresources/productlist.html?from=shipping#additionalServices ,
- *Edi* - query parameter to describe if EDI is used,
- *Products* - query parameter to describe required products. List of available products: http://developer.bring.com/additionalresources/productlist.html?from=shipping ,
- *ShippedFromPostOffice* - query parameter to describe if shipped from post office,
- *ShippingDateAndTime* - query parameter to describe package shipping date and/or time to Bring,
- *PriceAdjustment* - query parameter to describe price adjustments. Prices can be increased/decreased by percentage or amount or fixed price used.

### Pickup API

To start using Pickup API you have to create new *PickupClient* with provided settings.

    var settings = new PickupSettings();
    IPickupClient client = new PickupClient(settings);

To find pickup points there are 4 different methods to use.

    client.FindByZipCode(PickupZipCodeQuery query);

Query requires that *CountryCode* and *ZipCode* are set.
- *CountryCode* - Country. Possible values: NO, DK, SE, FI
- *PostalCode* - Postal code.

List of available additional parameters: https://developer.bring.com/api/pickup-point/#pickup-points-for-postal-code

    client.FindByLocation(PickupLocationQuery query);

Query requires that *CountryCode*, *Latitude* and *Longitude* are set.
- *CountryCode* - Country. Possible values: NO, DK, SE, FI
- *Latitude* - Latitude. Geographic coordinate specifying the north-south position.
- *Longitude* - Longitude. Geographic coordinate specifying the east-west position.

List of available additional parameters: https://developer.bring.com/api/pickup-point/#pickup-points-for-location

    client.FindById(string countryCode, string id);

- *countryCode* - Country. Possible values: NO, DK, SE, FI
- *id* - Id of pickup point.

List of available additional parameters: https://developer.bring.com/api/pickup-point/#a-specific-pickup-point

    client.All(string countryCode);

- *countryCode* - Country. Possible values: NO, DK, SE, FI

Full documentation: https://developer.bring.com/api/pickup-point/#all-pickup-points-in-a-country

### Tracking API

To start using Tracking API you have to create new *TrackingClient* with provided settings.

    ITrackingClient client = new TrackingClient();

Default constructor do not require any parameters, but there is available constructor which accepts *TrackingSettings*. *TrackingSettings* by default do not have any paramters, but you can change endpoint URI if it changes. By default it uses http://sporing.bring.no/sporing.json as service endpoint.

To get tracking information you have to call *TrackAsync* method with tracking number.

    var result = await client.TrackAsync("123456789");

The method returns list of package consignment statuses.

### Booking API

To start using Booking API you have to create new *BookingClient* with provided settings.

    var settings = new BookingSettings(
        "Uid",
        "ApiKey",
        new Uri("http://clientUri"),
        test: false
        );
    IBookingClient client = new BookingClient(settings);

*BookingSettings* has four parameters:
- *uid* - Bring API User ID,
- *key* - Bring API Key,
- *clientUri* - base URI of your Web site,
- *isTest* - mark if test mode in use (default: false),
- *endpointUri* - Bring API endpoint (default: https://api.bring.com/booking/api/booking).

To book consignment you have to call *BookAsync* method with consignment details. The overload of the method can handle multiple consignments.

    var sender = new Party(
        "Sender Name",
        "Address 1",
        "Address 2",
        "0123",
        "Oslo",
        "NOR",
        "reference number",
        "additional info",
        new Contact("John", "john@example.com", "98745612"));

    var recipient = new Party(
        "Recipient Name",
        "Address 1",
        "Address 2",
        "0123",
        "Oslo",
        "NOR",
        "reference number",
        "additional info",
        new Contact("Tom", "tom@example.com", "23654789"));

    var product = new Product(
        "A-POST", // Bring product code
        "customer number"); // Customer number for Bring product

    var packages = new[]
    {
        new Package(
            "correlation ID", 
            1.0, // weight in kilograms
            "Products", // goods description
            new Dimensions(10, 10, 10)) // dimensions H W L in cm
    };

    var consignment = new Consignment(
        "correlation ID",
        DateTime.UtcNow.AddDays(1), // date when package delivered to Bring
        new Parties(sender, recipient),
        product,
        packages);

    var result = await sut.BookAsync(consignment);

The method returns confirmation information.

### EPiServer Commerce module

EPiServer Commerce module consists of two libraries: *Geta.Bring.EPi.Commerce* - library which should be installed into your Web site; *Geta.Bring.EPi.Commerce.Manager* - library which should be installed into Commerce Manager Web site. *Geta.Bring.EPi.Commerce* contains shipping gateway - *BringShippingGateway* and types for shipping details - *BringShippingRate* and *BringShippingRateGroup*. *Geta.Bring.EPi.Commerce.Manager* contains required views to configure EPiServer Commerce Bring module.

#### Getting rates

To list all awailable shipping options create gateway instances for each available shipping method and use *GetRate* method as in EPiServer Commerce [documentation](http://world.episerver.com/documentation/Items/Developers-Guide/EPiServer-Commerce/8/Shipping/Shipping/). Example:

    CommerceCart.Shipment shipment = ...; // getting shipment from cart
    var shippingMethods = ShippingManager.GetShippingMethods(languageId);
    var shippingRates = new List<CommerceCart.ShippingRate>();
    foreach (var shippingMethod in 
        shippingMethods.ShippingMethod.OrderBy(x => x.Ordering))
    {
        var type = Type.GetType(shippingMethod.ShippingOptionRow.ClassName);
        var shippingGateway = 
            (CommerceCart.IShippingGateway)Activator.CreateInstance(type);
        var message = string.Empty;
        var rate = shippingGateway.GetRate(
            shippingMethod.ShippingMethodId, shipment, ref message);
        shippingRates.Add(rate);
    }

This example shows how to get all awailable shipping rates. You also can get only Bring shipping rates with specific details and also group them.

    var bringShippingRates = shippingRates.OfType<BringShippingRate>();
    var rateGroups = bringShippingRates
        .GroupBy(x => x.MainDisplayCategory)
        .Select(x => new BringShippingRateGroup(x.Key, x));

You can create a partial view to render bring shipping rates. For example:

    @model IEnumerable<Geta.Bring.EPi.Commerce.Model.BringShippingRateGroup>

    @foreach (var shippingGroup in Model)
    {
        <div>
            <h2>@shippingGroup.Name</h2>
            @foreach (var shippingMethod in shippingGroup.ShippingRates)
            {
                <div>
                    <input type="radio" value="@shippingMethod.Id" group="ShippingMethod" />
                    @shippingMethod.Name
                    @shippingMethod.Description
                    @shippingMethod.Money.Amount.ToString("0.00") @shippingMethod.Money.Currency.CurrencyCode
                </div>
            }
        </div>
    }

There is available sample view on [GitHub](https://github.com/Geta/Geta.Bring/tree/master/docs/example). It contains HTML/CSS sample and sample MVC view - *Rates.cshtml*.

To configure shipping methods you have to go to *Commerce Manager* - *Administration* and under *Order System* - *Shipping* - *Shipping Methods* - *language* start creating new shipping method by clicking *New*. Provide shipping method details in *Overview* tab and use *Bring Shipping Gateway* as *Provider*. Click *OK* and open again newly created method. In *Parameters* tab provide Bring Product, customer number, ship from postal code, price rounding and other parameters.

![Configuration](https://github.com/Geta/Geta.Bring/blob/master/docs/config.png?raw=true)

## More info about Bring API

### Shipping Guide API

http://developer.bring.com/api/shipping-guide/

*Demo*

http://fraktguide.bring.no/fraktguide/demoVelgFraktalternativ.do?from=0484&to=0470&weightInGrams=800&length=10&width=20&height=5&product=servicepakke&product=pa_doren&product=bpakke_dor-dor&product=smaapakker_a-post&product=ekspress09&product=smaapakker_b-post&product=courier_1h&product=minipakke&callbackUrl=http://fraktguide.bring.no/fraktguide/popupCallback.jsp&date=2014-03-17

### Tracking API

http://developer.bring.com/api/tracking/

### Booking API

http://developer.bring.com/api/booking
