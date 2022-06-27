using System.IO;

namespace DanTheMan827.Modulation.Extensions
{
    internal static class String_ReplaceInvalidFilenameCharsExtension
    {
        internal static string ReplaceInvalidFilenameChars(this string input)
        {
            return string.Join("_", input.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
