using System.Collections.Generic;
using System.Linq;

namespace DanTheMan827.Modulation.Extensions
{
    internal static partial class Extensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
