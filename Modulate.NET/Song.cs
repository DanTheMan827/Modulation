using System.IO;
using System.Text.RegularExpressions;

namespace DanTheMan827.ModulateDotNet
{
    public class Song
    {
        private static Regex cleanRegex = new Regex(@"[^a-zA-Z\d\s~`!@#$%^&*()_\-+=[{\]}\\|<,>\./?:]+", RegexOptions.Compiled);
        internal string fullPath = string.Empty;
        private string path = string.Empty;
        private string artist = string.Empty;
        private string arena = string.Empty;
        private string name = string.Empty;

        public string ID { get; set; } = string.Empty;
        public string Name { get => this.MoggSong?.Title ?? this.name; set => this.name = value; }
        public string CleanName => cleanRegex.Replace(this.Name, "");
        public string Artist { get => this.MoggSong?.Artist ?? this.artist; set => this.artist = value; }
        public string CleanArtist => cleanRegex.Replace(this.Artist, "");
        public string Type { get; set; } = string.Empty;
        public string Path
        {
            get => this.path;
            set
            {
                this.path = value;
                this.SongFolder = Directory.GetParent(value).Name;
            }
        }
        public string UnlockType { get; set; } = string.Empty;
        public string UnlockValue { get; set; } = string.Empty;
        public string Arena { get => this.MoggSong?.ArenaPath ?? this.arena; set => this.arena = value; }
        public string SongFolder { get; private set; } = string.Empty;
        public bool HasCharter => !string.IsNullOrWhiteSpace(this.MoggSong?.Charter);
        public bool HasDemoVideo => !string.IsNullOrWhiteSpace(this.MoggSong?.DemoVideo);
        public MoggSong MoggSong { get; set; }
    }
}
