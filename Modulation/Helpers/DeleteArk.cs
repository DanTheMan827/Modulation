using System.IO;

namespace DanTheMan827.Modulation.Helpers
{
    internal partial class HelperMethods
    {
        public static void DeleteArk(string headerFile)
        {
            var info = new FileInfo(headerFile);
            var arkPrefix = Path.GetFileNameWithoutExtension(headerFile);

            if (info.Exists)
            {
                info.Delete();
            }

            foreach (var file in info.Directory.GetFiles($"{arkPrefix}_*.ark", SearchOption.TopDirectoryOnly))
            {
                if (file.Name.ToLower().StartsWith($"{arkPrefix.ToLower()}_") && file.Name.ToLower().EndsWith(".ark"))
                {
                    file.Delete();
                }
            }
        }
    }
}
