using System.IO;

namespace DanTheMan827.Modulation.Extensions
{
    internal static partial class Extensions
    {
        internal static string ReplaceInvalidPathChars(this string input)
        {
            return string.Join("_", input.Split(Path.GetInvalidPathChars()));
        }
    }
}
