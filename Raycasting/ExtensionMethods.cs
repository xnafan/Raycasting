using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public static class ExtensionMethods
    {
        private static Random _rnd = new Random();
        public static T ModuloIndexedItem<T>(this List<T> items, int index)
        {
            return items.ToList()[index % items.Count];
        }

        public static T GetRandomElement<T>(this IList<T> items)
        {
            return items[_rnd.Next(items.Count)];
        }
    }
}
