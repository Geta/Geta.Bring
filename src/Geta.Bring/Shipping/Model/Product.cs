﻿using System.Collections.Generic;
using System.Linq;

namespace Geta.Bring.Shipping.Model
{
    /// <summary>
    /// Products from http://developer.bring.com/additionalresources/productlist.html?from=shipping
    /// </summary>
    public class Product
    {
        private Product(string displayName, string code, bool priceAvailable, bool expectedDeliveryAvailable)
        {
            ExpectedDeliveryAvailable = expectedDeliveryAvailable;
            PriceAvailable = priceAvailable;
            Code = code;
            DisplayName = displayName;
        }

        /// <summary>
        /// Product display name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Product code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Mark if price information available for product.
        /// </summary>
        public bool PriceAvailable { get; private set; }

        /// <summary>
        /// Mark if expected delivery information available for product.
        /// </summary>
        public bool ExpectedDeliveryAvailable { get; private set; }

        public static Product Servicepakke = new Product("Klimanøytral Servicepakke", "SERVICEPAKKE", true, true);
        public static Product PaDoren = new Product("På Døren", "PA_DOREN", true, true);
        public static Product BpakkeDorDor = new Product("Bedriftspakke", "BPAKKE_DOR-DOR", true, true);
        public static Product Express09 = new Product("Bedriftspakke Ekspress-Over natten", "EKSPRESS09", true, true);
        public static Product Minipakke = new Product("Minipakke", "MINIPAKKE", true, true);
        public static Product APost = new Product("A-Prioritert", "A-POST", true, false);
        public static Product BPost = new Product("B-Økonomi", "B-POST", true, true);
        public static Product SmaapakkerAPost = new Product("Småpakker A-Post", "SMAAPAKKER_A-POST", true, true);
        public static Product SmaapakkerBPost = new Product("Småpakker B-Post", "SMAAPAKKER_B-POST", true, true);
        public static Product QuickpackSameday = new Product("Quickpack SameDay", "EXPRESS_NORDIC_SAME_DAY", true, true);
        public static Product QuickpackOverNight0900 = new Product("Quickpack Over Night 0900", "EXPRESS_INTERNATIONAL_0900", true, true);
        public static Product QuickpackOverNight1200 = new Product("Quickpack Over Night 1200", "EXPRESS_INTERNATIONAL_1200", true, true);
        public static Product QuickpackDayCertain = new Product("Quickpack Day Certain", "EXPRESS_INTERNATIONAL", true, true);
        public static Product QuickpackExpressEconomy = new Product("Quickpack Express Economy", "EXPRESS_ECONOMY", true, true);
        public static Product CargoGroupage = new Product("Cargo", "CARGO_GROUPAGE", true, true);
        public static Product CarryonBusiness = new Product("CarryOn Business", "CARRYON_BUSINESS", true, true);
        public static Product CarryonHomeshopping = new Product("CarryOn HomeShopping", "CARRYON_HOMESHOPPING", true, true);
        public static Product HomedeliveryCurbsideDag = new Product("HomeDelivery Curb Side", "HOMEDELIVERY_CURBSIDE_DAG", false, true);
        public static Product CourierVip = new Product("Bud VIP", "COURIER_VIP", true, true);
        public static Product Courier1h = new Product("Bud 1 time", "COURIER_1H", true, true);
        public static Product Courier2h = new Product("Bud 2 timer", "COURIER_2H", true, true);
        public static Product Courier4h = new Product("Bud 4 timer", "COURIER_4H", true, true);
        public static Product Courier6h = new Product("Bud 6 timer", "COURIER_6H", true, true);
        public static Product Ox = new Product("Oil Express", "OX", true, true);

        public static IEnumerable<Product> All
        {
            get
            {
                yield return Servicepakke;
                yield return PaDoren;
                yield return BpakkeDorDor;
                yield return Express09;
                yield return Minipakke;
                yield return APost;
                yield return BPost;
                yield return SmaapakkerAPost;
                yield return SmaapakkerBPost;
                yield return QuickpackSameday;
                yield return QuickpackOverNight0900;
                yield return QuickpackOverNight1200;
                yield return QuickpackDayCertain;
                yield return QuickpackExpressEconomy;
                yield return CargoGroupage;
                yield return CarryonBusiness;
                yield return CarryonHomeshopping;
                yield return HomedeliveryCurbsideDag;
                yield return CourierVip;
                yield return Courier1h;
                yield return Courier2h;
                yield return Courier4h;
                yield return Courier6h;
                yield return Ox;
            }
        }

        public static Product GetByCode(string code)
        {
            return All.First(x => x.Code == code);
        }
    }
}