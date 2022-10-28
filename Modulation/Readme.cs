using AmpHelper.Types;
using DanTheMan827.Modulation.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DanTheMan827.Modulation
{
    internal static class Readme
    {
        private static string Indent(int levels = 0, string indent = "    ", int baseLevels = 3)
        {
            return string.Join(indent, Enumerable.Repeat("", levels + baseLevels + 1).ToArray());
        }
        public static string GenerateReadme(params MoggSong[] songs)
        {
            var songListHtml = new List<string>();

            foreach (var song in songs.OrderBy(song => song.CleanName()).OrderBy(song => song.CleanArtist()))
            {
                songListHtml.AddRange(new string[] {
                    $"\n{Indent(0)}<div class=\"song\">\n",
                    $"{Indent(1)}<div class=\"title\">\n{Indent(2)}{HttpUtility.HtmlEncode($"{song.CleanArtist()} - {song.CleanName()}")}",
                    !string.IsNullOrWhiteSpace(song.DemoVideo) ? $" - \n{Indent(2)}<a href=\"{HttpUtility.HtmlEncode(song.DemoVideo.Trim())}\" target=\"_blank\">\n{Indent(3)}Demo Video\n{Indent(2)}</a>\n" : "\n",
                    $"{Indent(1)}</div>\n",

                    $"{Indent(1)}<div class=\"id\">\n{Indent(2)}<strong>ID: </strong>\n{Indent(2)}{HttpUtility.HtmlEncode($"{song.ID}")}\n{Indent(1)}</div>\n",
                    (song.Bpm != null) ? $"{Indent(1)}<div class=\"bpm\">\n{Indent(2)}<strong>BPM: </strong>\n{Indent(2)}{HttpUtility.HtmlEncode($"{song.Bpm}")}\n{Indent(1)}</div>\n" : "",
                    !string.IsNullOrWhiteSpace(song.Charter) ? $"{Indent(1)}<div class=\"charter\">\n{Indent(2)}<strong>Charter: </strong>\n{Indent(2)}{HttpUtility.HtmlEncode(song.Charter.Trim())}\n{Indent(1)}</div>\n" : "",
                    $"{Indent(1)}<div class=\"content\">",
                    !string.IsNullOrWhiteSpace(song.CleanDescription()) ? $"\n{Indent(2)}<p>{HttpUtility.HtmlEncode(song.CleanDescription().Trim())}</p>\n{Indent(1)}</div>\n" : "</div>\n",
                    $"{Indent(0)}</div>"
                });
            }

            return AppResources.ZipReadme.Replace("<!--#SongList#-->", string.Join("", songListHtml));
        }
    }
}
