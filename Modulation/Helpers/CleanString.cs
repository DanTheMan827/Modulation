using System.Text.RegularExpressions;

namespace DanTheMan827.Modulation.Helpers
{
    internal partial class HelperMethods
    {
        private static Regex songCleanRegex = new Regex(@"[^a-zA-Z\d\s~`!@#$%^&*()_\-+=[{\]}\\|<,>\./?:]+", RegexOptions.Compiled);
        private static Regex spaceCleanRegex = new Regex(@"\s+", RegexOptions.Compiled);
        public static string CleanString(string input)
        {
            if (input == null)
            {
                return null;
            }

            var output = songCleanRegex.Replace(input, "");
            output = spaceCleanRegex.Replace(output, " ");

            return output;
        }
    }
}
