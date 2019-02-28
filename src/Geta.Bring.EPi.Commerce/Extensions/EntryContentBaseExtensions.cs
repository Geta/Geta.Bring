using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.ServiceLocation;

namespace Geta.Bring.EPi.Commerce.Extensions
{
    internal static class EntryContentBaseExtensions
    {
        private static Injected<IRelationRepository> _injectedRelationRepository;
        private static Injected<IContentLoader> _injectedContentLoader;

        public static decimal CalculateRelationsWeight<T>(this EntryContentBase entry) where T : EntryRelation
        {
            if (entry == null)
            {
                return 0;
            }

            var relations = _injectedRelationRepository.Service.GetRelationsByTarget<T>(entry.ContentLink);
            var contentLoader = _injectedContentLoader.Service;

            decimal weight = 0;

            foreach (var entryRelation in relations)
            {
                if (contentLoader.TryGet(entryRelation.Target, out EntryContentBase relationEntry))
                {
                    var stockPlacementEntry = relationEntry as IStockPlacement;

                    if (stockPlacementEntry != null)
                    {
                        weight += (decimal)stockPlacementEntry.Weight * entryRelation.Quantity.GetValueOrDefault(1);
                    }
                }
            }

            return weight;
        }
    }
}