using System.Collections.Generic;
using System.Linq;

namespace DanTheMan827.Modulation.Extensions
{
    public static class IEnumerable_WithIndex
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
