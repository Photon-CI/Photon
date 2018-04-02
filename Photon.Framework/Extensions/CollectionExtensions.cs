using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Extensions
{
    public static class CollectionExtensions
    {
        //public static bool ContainsAny<T>(this ICollection<T> collection, IEnumerable<T> anyItems)
        //{
        //    var anyItemList = anyItems as T[] ?? anyItems.ToArray();
        //    return collection.Any(anyItemList.Contains);
        //}

        public static bool ContainsAny<T>(this ICollection<T> collection, params T[] anyItems)
        {
            return collection.Any(anyItems.Contains);
        }
    }
}
