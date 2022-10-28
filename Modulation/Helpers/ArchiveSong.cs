using AmpHelper;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace DanTheMan827.Modulation.Helpers
{
    internal partial class HelperMethods
    {
        private static readonly string[] coreSongExtensions = new string[] { "mid", "mogg", "moggsong" };
        public static async Task ArchiveSong(string unpackedPath, Stream outputStream, string? readmeText = null, Action<long, long>? progress = null, params string[] songNames)
        {
            if (unpackedPath == null)
            {
                throw new ArgumentNullException(nameof(unpackedPath));
            }

            if (!Directory.Exists(unpackedPath))
            {
                throw new ArgumentException($"Directory does not exist: {unpackedPath}");
            }

            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            string consoleName = ps3Mode ? "ps3" : "ps4";
            string songPath = Path.Combine(unpackedPath, consoleName, "songs");
            using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);

            if (readmeText != null)
            {
                var demoFile = archive.CreateEntry("readme.html");

                using var readme = demoFile.Open();
                using var streamWriter = new StreamWriter(readme);
                await streamWriter.WriteAsync(readmeText);
            }
            long counter = 0;
            foreach (string song in songNames)
            {
                progress?.Invoke(counter, songNames.Length);
                string songSourcePath = Path.Combine(songPath, song);

                if (Song.ValidateSong(songSourcePath) != null)
                {
                    throw new Exception("Song validation failed.");
                }

                _ = archive.CreateEntry($"{song}/");

                foreach (string extension in coreSongExtensions)
                {
                    _ = await Task.Run(() => archive.CreateEntryFromFile(Path.Combine(songPath, song, $"{song}.{extension}"), $"{song}/{song}.{extension}"));
                }

                counter++;
            }

            progress?.Invoke(counter, songNames.Length);
        }
    }
}
