using DanTheMan827.ModulateDotNet;
using DtxCS.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanTheMan827.Modulation.Extensions
{
    public static class MoggSongExtensions
    {
        public static MoggSong LoadDta(this MoggSong moggSong, DataArray root)
        {
            var song = new MoggSong();

            var dict = new Dictionary<string, string>();

            foreach (var entry in root.Children.Where(x => x is DataArray).Cast<DataArray?>().Where(x => x.Children.Count == 2 && x.Children[0] is DataSymbol && (x.Children[1] is DataAtom || x.Children[1] is DataSymbol)))
            {
                var key = (entry.Children[0] as DataSymbol).ToString();
                var atom = entry.Children[1];
                string value = atom.ToString();

                if (atom is DataAtom && (atom as DataAtom).Type == DataType.STRING)
                {
                    value = (atom as DataAtom).String;
                }

                dict.Add(key, value);
            }

            if (dict.ContainsKey("mogg_path"))
                moggSong.MoggPath = dict["mogg_path"];

            if (dict.ContainsKey("midi_path"))
                moggSong.MidiPath = dict["midi_path"];

            if (dict.ContainsKey("arena_path"))
                moggSong.ArenaPath = dict["arena_path"];

            if (dict.ContainsKey("tunnel_scale"))
            {
                decimal decValue;

                if (decimal.TryParse(dict["tunnel_scale"], out decValue))
                {
                    moggSong.TunnelScale = decValue;
                }
            }

            if (dict.ContainsKey("title"))
                moggSong.Title = dict["title"];

            if (dict.ContainsKey("title_short"))
                moggSong.ShortTitle = dict["title_short"];

            if (dict.ContainsKey("artist"))
                moggSong.Artist = dict["artist"];

            if (dict.ContainsKey("artist_short"))
                moggSong.ShortArtist = dict["artist_short"];

            if (dict.ContainsKey("desc"))
                moggSong.Description = dict["desc"];

            if (dict.ContainsKey("unlock_requirement"))
                moggSong.UnlockRequirement = dict["unlock_requirement"];

            if (dict.ContainsKey("bpm"))
            {
                int intVal;

                if (int.TryParse(dict["bpm"], out intVal))
                {
                    moggSong.Bpm = intVal;
                }
            }

            if (dict.ContainsKey("preview_start_ms"))
            {
                int intVal;

                if (int.TryParse(dict["preview_start_ms"], out intVal))
                {
                    moggSong.PreviewStartMs = intVal;
                }
            }

            if (dict.ContainsKey("preview_length_ms"))
            {
                int intVal;

                if (int.TryParse(dict["preview_length_ms"], out intVal))
                {
                    moggSong.PreviewLengthMs = intVal;
                }
            }

            return moggSong;
        }
    }
}
