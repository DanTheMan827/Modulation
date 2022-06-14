using System;
using System.IO;

namespace DanTheMan827.Modulation.Extensions
{
    internal static class String_ReplaceInvalidFilenameCharsExtension
    {
        internal static string ReplaceInvalidFilenameChars(this String input)
        {
            return string.Join("_", input.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
