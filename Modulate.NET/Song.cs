using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DanTheMan827.ModulateDotNet
{
    public class Song
    {
        private static Regex cleanRegex = new Regex(@"[^a-zA-Z\d\s~`!@#$%^&*()_\-+=[{\]}\\|<,>\./?:]+", RegexOptions.Compiled);
        private string path = String.Empty;
        private string artist = String.Empty;
        private string type = String.Empty;
        private string unlockType = String.Empty;
        private string unlockValue = String.Empty;
        private string arena = String.Empty;
        private string songFolder = String.Empty;
        private string name = String.Empty;

        public string ID { get; set; } = String.Empty;
        public string Name { get => MoggSong?.Title ?? name; set => name = value; }
        public string CleanName => cleanRegex.Replace(Name, "");
        public string Artist { get => MoggSong?.Artist ?? artist; set => artist = value; }
        public string CleanArtist => cleanRegex.Replace(Artist, "");
        public string Type { get => type; set => type = value; }
        public string Path
        {
            get => path;
            set
            {
                path = value;
                SongFolder = Directory.GetParent(value).Name;
            }
        }
        public string UnlockType { get => unlockType; set => unlockType = value; }
        public string UnlockValue { get => unlockValue; set => unlockValue = value; }
        public string Arena { get => MoggSong?.ArenaPath ?? arena; set => arena = value; }
        public string SongFolder { get => songFolder; private set => songFolder = value; }
        public MoggSong MoggSong { get; set; }
    }
}
