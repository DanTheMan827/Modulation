using System.IO;

namespace DanTheMan827.Modulation.Extensions
{
    internal static class String_ReplaceInvalidPathChars
    {
        internal static string ReplaceInvalidPathChars(this string input)
        {
            return string.Join("_", input.Split(Path.GetInvalidPathChars()));
        }
    }
}
