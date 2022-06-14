using System;
using System.IO;

namespace DanTheMan827.Modulation.Extensions
{
    internal static class String_ReplaceInvalidPathChars
    {
        internal static string ReplaceInvalidPathChars(this String input)
        {
            return string.Join("_", input.Split(Path.GetInvalidPathChars()));
        }
    }
}
