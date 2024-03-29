﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DanTheMan827.ModulateDotNet
{
    public class Modulate
    {
        public static string ModulatePath => Path.Combine(ModulateExe.Shared.ExePath);
        public static readonly string[] baseSongs = new string[] {
            "allthetime", "assault_on", "astrosight", "breakforme", "concept", "crazy_ride", "credits", "crystal",
            "dalatecht", "decode_me", "digitalparalysis", "donot", "dreamer", "energize", "entomophobia", "forcequit",
            "humanlove", "impossible", "iseeyou", "lights", "magpie", "muze", "necrodancer", "perfectbrain",
            "phantoms", "recession", "redgiant", "supraspatial", "synthesized2014", "tut0", "tut1", "tutc",
            "unfinished", "wayfarer", "wetware"
        };
        private static readonly string[] coreSongExtensions = new string[] { "mid", "mogg", "moggsong" };
        public UnpackedInfo UnpackedInfo { get; private set; }

        public Modulate(UnpackedInfo unpackedInfo)
        {
            this.UnpackedInfo = unpackedInfo;
        }

        /// <summary>
        /// Create UnpackedInfo based on manual input.
        /// </summary>
        /// <param name="unpackedPath">The path to the unpacked ark data.</param>
        /// <param name="headerPath">The folder containing main_ps3.hdr or main_ps4.hdr.  If null is specified, headerPath will be set to the unpackedPath.</param>
        /// <param name="errorType">Whether to throw errors or return null.</param>
        /// <returns>An UnpackedInfo object with the data provided after being validated, or null if there was an error and errorType was set to ErrorType.ReturnNull.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
#nullable enable
        public static UnpackedInfo? FromUnpacked(string unpackedPath, string? headerPath = null, ErrorType errorType = ErrorType.ReturnNull)
#nullable restore
        {

            headerPath ??= unpackedPath;

            if (unpackedPath == null)
            {
                return errorType == ErrorType.ThrowError ? throw new ArgumentNullException(nameof(unpackedPath)) : (UnpackedInfo)null;
            }

            if (!Directory.Exists(headerPath))
            {
                return errorType == ErrorType.ThrowError ? throw new DirectoryNotFoundException(headerPath) : (UnpackedInfo)null;
            }

            if (!Directory.Exists(unpackedPath))
            {
                return errorType == ErrorType.ThrowError ? throw new DirectoryNotFoundException(unpackedPath) : (UnpackedInfo)null;
            }

            bool ps3Mode = false;

            if (Directory.Exists(Path.Combine(unpackedPath, "ps3")))
            {
                ps3Mode = true;
            }

            if (!ps3Mode && !Directory.Exists(Path.Combine(unpackedPath, "ps4")))
            {
                return errorType == ErrorType.ThrowError
                    ? throw new DirectoryNotFoundException("Neither a ps3 or ps4 folder can be found in the unpacked path")
                    : (UnpackedInfo)null;
            }

            string headerFilename = ps3Mode ? "main_ps3.hdr" : "main_ps4.hdr";
            string headerFullPath = Path.Combine(headerPath, headerFilename);

            return !File.Exists(headerFullPath)
                ? errorType == ErrorType.ThrowError ? throw new FileNotFoundException(headerFullPath) : (UnpackedInfo)null
                : new UnpackedInfo()
                {
                    FromUnpacked = true,
                    HeaderPath = headerPath,
                    UnpackedPath = unpackedPath,
                    UnpackLog = "Info created manually.",
                    Console = ps3Mode ? UnpackedType.PS3 : UnpackedType.PS4,
                    ExitCode = 0
                };
        }

        public async static Task<UnpackedInfo> Unpack(string packedPath, string unpackedPath)
        {
            if (packedPath == null || unpackedPath == null)
            {
                throw new ArgumentNullException();
            }

            var unpackedInfo = new UnpackedInfo
            {
                HeaderPath = unpackedPath,
                UnpackedPath = unpackedPath, // Path.Combine(unpackedPath, "unpacked");
                SourcePath = packedPath,
                FromUnpacked = false
            };
            _ = Directory.CreateDirectory(unpackedInfo.UnpackedPath);

            packedPath = File.Exists(packedPath) ? new FileInfo(packedPath).Directory.FullName : packedPath;

            if (packedPath == null || !Directory.Exists(packedPath))
            {
                throw new ArgumentException("Packed path not valid");
            }

            bool ps3Mode = File.Exists(Path.Combine(packedPath, "main_ps3.hdr"));
            string consoleName = ps3Mode ? "ps3" : "ps4";
            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                WorkingDirectory = packedPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            unpackedInfo.Console = ps3Mode ? UnpackedType.PS3 : UnpackedType.PS4;

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-unpack");
            launchInfo.ArgumentList.Add(unpackedInfo.UnpackedPath);

            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            unpackedInfo.ExitCode = proc.ExitCode;

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }

            string originalHdr = Path.Combine(unpackedInfo.UnpackedPath, $"main_{consoleName}.hdr");

            if (!File.Exists(originalHdr))
            {
                await Task.Run(() =>
                {
                    File.Copy(Path.Combine(packedPath, $"main_{consoleName}.hdr"), originalHdr);
                });
            }

            unpackedInfo.UnpackLog = output;

            return unpackedInfo;
        }

        public Task<IEnumerable<Song>> ListSongs()
        {
            return Modulate.ListSongs(this.UnpackedInfo);
        }

        public static Task<IEnumerable<Song>> ListSongs(UnpackedInfo unpackedInfo)
        {
            return ListSongs(unpackedInfo.UnpackedPath);
        }

        public async static Task<IEnumerable<Song>> ListSongs(string unpackedPath)
        {
            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-listsongs");
            launchInfo.ArgumentList.Add(unpackedPath);


            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }

            var results = Regex.Matches(output, @"Song (\d+)\t  (.*?) - (.*?) - (.*?)\r\n\t  (.*)\r\n\t  Unlocked by ([^\s]+) ([^\s]+)\r\n\t  Arena: (.*?)\r\n");
            var songs = new List<Song>();
            foreach (Match result in results)
            {
                songs.Add(new Song()
                {
                    ID = result.Groups[2].Value,
                    Name = result.Groups[3].Value,
                    Type = result.Groups[4].Value,
                    Path = result.Groups[5].Value,
                    UnlockValue = result.Groups[6].Value,
                    UnlockType = result.Groups[7].Value,
                    Arena = result.Groups[8].Value,
                });
            }

            songs.Sort((song1, song2) =>
            {
                return song1.Name.CompareTo(song2.Name);
            });

            return songs;
        }

        public Task BuildSong(string songName)
        {
            return Modulate.BuildSong(this.UnpackedInfo, songName);
        }

        public static Task BuildSong(UnpackedInfo unpackedInfo, string songName)
        {
            return BuildSong(unpackedInfo.UnpackedPath, songName);
        }

        public async static Task BuildSong(string unpackedPath, string songName)
        {
            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                WorkingDirectory = unpackedPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-buildsong");
            launchInfo.ArgumentList.Add(unpackedPath);
            launchInfo.ArgumentList.Add(songName);


            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }
        }

        public Task BuildSongs()
        {
            return Modulate.BuildSongs(this.UnpackedInfo);
        }

        public static Task BuildSongs(UnpackedInfo unpackedInfo)
        {
            return BuildSongs(unpackedInfo.UnpackedPath);
        }

        public async static Task BuildSongs(string unpackedPath)
        {
            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                WorkingDirectory = unpackedPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-buildsongs");
            launchInfo.ArgumentList.Add(unpackedPath);


            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }
        }

        public Task AutoAdd()
        {
            return Modulate.AutoAdd(this.UnpackedInfo);
        }

        public static Task AutoAdd(UnpackedInfo unpackedInfo)
        {
            return AutoAdd(unpackedInfo.UnpackedPath);
        }

        public async static Task AutoAdd(string unpackedPath)
        {
            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                WorkingDirectory = unpackedPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-autoadd");
            launchInfo.ArgumentList.Add(unpackedPath);


            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }
        }

        public Task Pack(string packedPath)
        {
            return Modulate.Pack(this.UnpackedInfo, packedPath);
        }

        public static Task Pack(UnpackedInfo unpackedInfo, string packedPath)
        {
            return Pack(unpackedInfo.UnpackedPath, packedPath, unpackedInfo.HeaderPath);
        }

        public static async Task Pack(string unpackedPath, string packedPath, string hdrPath)
        {
            if (packedPath == null || unpackedPath == null)
            {
                throw new ArgumentNullException();
            }

            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                WorkingDirectory = hdrPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-pack_add");
            launchInfo.ArgumentList.Add(unpackedPath);
            launchInfo.ArgumentList.Add(packedPath);

            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }

            return;
        }

        public Task RemoveSong(string song, bool deleteAfter = true)
        {
            return Modulate.RemoveSong(this.UnpackedInfo, song, deleteAfter);
        }

        public static Task RemoveSong(UnpackedInfo unpackedInfo, string song, bool deleteAfter = true)
        {
            return RemoveSong(unpackedInfo.UnpackedPath, song, deleteAfter);
        }

        public static async Task RemoveSong(string unpackedPath, string song, bool deleteAfter = true)
        {
            if (baseSongs.Contains(song.ToLower()))
            {
                throw new Exception("Can't remove songs included with game!");
            }

            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            string songPath = Path.Combine(unpackedPath, ps3Mode ? "ps3" : "ps4", "songs");
            string songDestPath = Path.Combine(songPath, song);

            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-removesong");
            launchInfo.ArgumentList.Add(unpackedPath);
            launchInfo.ArgumentList.Add(song);

            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }

            if (deleteAfter && Directory.Exists(songDestPath))
            {
                await Task.Run(() =>
                {
                    Directory.Delete(songDestPath, true);
                });
            }
        }

        public static bool ValidateSong(string songPath, string songName = null)
        {
            var di = new DirectoryInfo(songPath);
            if (!di.Exists)
            {
                return false;
            }

            songName ??= di.Name;

            foreach (string extension in coreSongExtensions)
            {
                if (!File.Exists(Path.Combine(di.FullName, $"{songName}.{extension}")))
                {
                    return false;
                }
            }

            return true;
        }

        public Task AddSong(string songSourcePath, string songName = null, bool replace = false)
        {
            return AddSong(this.UnpackedInfo, songSourcePath, songName, replace);
        }

        public static Task AddSong(UnpackedInfo unpackedInfo, string songSourcePath, string songName = null, bool replace = false)
        {
            return AddSong(unpackedInfo.UnpackedPath, songSourcePath, songName, replace);
        }

        public static async Task AddSong(string unpackedPath, string songSourcePath, string songName = null, bool replace = false)
        {
            if (!ValidateSong(songSourcePath, songName))
            {
                throw new Exception($"Validation failed for {songSourcePath}");
            }

            bool ps3Mode = Directory.Exists(Path.Combine(unpackedPath, "ps3"));
            string consoleName = ps3Mode ? "ps3" : "ps4";
            string songPath = Path.Combine(unpackedPath, consoleName, "songs");

            songName ??= new DirectoryInfo(songSourcePath).Name;

            string songDestPath = Path.Combine(songPath, songName);
            string songDonorName = "tut0";
            string songDonorPath = Path.Combine(songPath, songDonorName);

            if (Directory.Exists(songDestPath))
            {
                if (!replace)
                {
                    throw new Exception($"{songName} already exists.");
                }

                await RemoveSong(unpackedPath, songName);
            }

            _ = Directory.CreateDirectory(songDestPath);

            foreach (string extension in coreSongExtensions)
            {
                await Task.Run(() =>
                {
                    File.Copy(Path.Combine(songSourcePath, $"{songName}.{extension}"), Path.Combine(songDestPath, $"{songName}.{extension}"));
                });
            }

            foreach (string extension in new string[] { $"mid_{consoleName}", $"png.dta_dta_{consoleName}", $"png_{consoleName}" })
            {
                await Task.Run(() =>
                {
                    File.Copy(Path.Combine(songDonorPath, $"{songDonorName}.{extension}"), Path.Combine(songDestPath, $"{songName}.{extension}"));
                });
            }

            var launchInfo = new ProcessStartInfo(ModulatePath)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (ps3Mode)
            {
                launchInfo.ArgumentList.Add("-ps3");
            }

            launchInfo.ArgumentList.Add("-addsong");
            launchInfo.ArgumentList.Add(unpackedPath);
            launchInfo.ArgumentList.Add(songName);

            var proc = Process.Start(launchInfo);
            string output = await proc.StandardOutput.ReadToEndAsync();
            await Task.Run(proc.WaitForExit);

            if (proc.ExitCode != 0)
            {
                throw new Exception(output);
            }

            return;
        }

        public Task ArchiveSong(Stream outputStream, string readmeText = null, Action<long, long> progress = null, params string[] songNames)
        {
            return ArchiveSong(this.UnpackedInfo, outputStream, readmeText, progress, songNames);
        }

        public static Task ArchiveSong(UnpackedInfo unpackedInfo, Stream outputStream, string readmeText = null, Action<long, long> progress = null, params string[] songNames)
        {
            return ArchiveSong(unpackedInfo.UnpackedPath, outputStream, readmeText, progress, songNames);
        }

        public static async Task ArchiveSong(string unpackedPath, Stream outputStream, string readmeText = null, Action<long, long> progress = null, params string[] songNames)
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

                if (ValidateSong(songSourcePath) == false)
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
