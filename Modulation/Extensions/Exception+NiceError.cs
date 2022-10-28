using System;
using System.Linq;

namespace DanTheMan827.Modulation.Extensions
{
    internal static partial class Extensions
    {
        public static string NiceError(this Exception ex)
        {
            var name = ex.GetType().Name.Split(".").Last();
            return $"{name}: {ex.Message}";
        }
    }
}
