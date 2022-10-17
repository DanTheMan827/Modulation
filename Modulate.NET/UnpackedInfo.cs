using System.IO;

namespace DanTheMan827.ModulateDotNet
{
    public class UnpackedInfo
    {
        /// <summary>
        /// The source path.
        /// </summary>
        public string SourcePath { get; internal set; }

        /// <summary>
        /// If the info came from an already unpacked folder.
        /// </summary>
        public bool FromUnpacked { get; internal set; }

        /// <summary>
        /// The path to the unpacked files.
        /// </summary>
        public string UnpackedPath { get; internal set; }

        /// <summary>
        /// The path to the header file.
        /// </summary>
        public string HeaderPath { get; internal set; }

        /// <summary>
        /// The log from Modulate.exe
        /// </summary>
        public string UnpackLog { get; internal set; }

        private UnpackedType _Console { get; set; }

        /// <summary>
        /// What console the unpacked data belongs to.
        /// </summary>
        public UnpackedType Console
        {
            get => this._Console;
            set
            {
                this._Console = value;
                this.ConsoleLabel = value == UnpackedType.PS3 ? "ps3" : "ps4";
            }
        }

        /// <summary>
        /// A label of the console type (ps3 or ps4)
        /// </summary>
        public string ConsoleLabel { get; internal set; }

        /// <summary>
        /// The exit code from Modulate.exe
        /// </summary>
        public int ExitCode { get; internal set; }
        public string SongsPath => Path.Combine(this.UnpackedPath, this.Console == UnpackedType.PS3 ? "ps3" : "ps4", "songs");

        public static explicit operator Modulate(UnpackedInfo info)
        {
            return new Modulate(info);
        }
    }
}
