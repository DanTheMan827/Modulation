using DanTheMan827.ModulateDotNet;
using DanTheMan827.Modulation.Extensions;
using DanTheMan827.Modulation.Tweaks;
using DanTheMan827.TempFolders;
using DtxCS;
using Microsoft.Win32;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPFCustomMessageBox;

namespace DanTheMan827.Modulation.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SongsWindow : Window
    {
        private UnpackedInfo openedInfo => this.ViewModel.Modulate.UnpackedInfo;
        private Modulate modulate => this.ViewModel.Modulate;
        public bool ChangesMade
        { get; set; } = false;
        private bool promptSave => this.openedInfo?.FromUnpacked == false && this.ChangesMade == true;

        public SongsWindow()
        {
            this.InitializeComponent();

            this.ViewModel.Songs.CollectionChanged += this.Songs_CollectionChanged;
        }

        private void Songs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ViewModel.ShowSongs.Value = this.ViewModel.Songs.Count > 0;
        }

        public async Task UpdateSongs()
        {
            this.ViewModel.Songs.Clear();

            var songs = (await this.modulate.ListSongs()).Where(s => !Modulate.baseSongs.Contains(s.SongFolder));

            foreach (var song in songs)
            {
                using var dtxStream = File.OpenRead(Path.Combine(this.openedInfo.SongsPath, song.SongFolder, $"{song.SongFolder}.moggsong"));
                var root = DTX.FromDtaStream(dtxStream);
                song.MoggSong = new MoggSong().LoadDta(root);
            }

            foreach (var song in songs.OrderBy(s => $"{s.CleanArtist}{s.CleanName}"))
            {
                this.ViewModel.Songs.Add(song);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await this.UpdateSongs();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            if (App.IsDesign)
            {
                this.ViewModel.Songs.Add(new Song()
                {
                    ID = "MYSONG",
                    Name = "My Song",
                    Path = "../Songs/mysong/mysong.moggsong",
                    Arena = "World1",
                    Type = "kExtraSong",
                    UnlockType = "play_num",
                    UnlockValue = "0"
                });
                this.ViewModel.ShowSongs.Value = true;
            }
        }

        private async void ButtonPack_Click(object sender, RoutedEventArgs e)
        {
            var song = (Song?)((Button?)sender)?.Tag;
            if (song == null || this.openedInfo == null)
            {
                return;
            }

            var saveDialog = new SaveFileDialog()
            {
                Filter = $"Zip Files (*.zip)|*.zip",
                FileName = $"{song.CleanArtist} - {song.CleanName}.zip".ReplaceInvalidFilenameChars(),
                CheckPathExists = true
            };

            try
            {
                if (saveDialog.ShowDialog() == true)
                {
                    using var file = File.Create(saveDialog.FileName);

                    var progActions = ProgressWindow.GetActions("Archiving", "Archiving, please wait.", this);

                    _ = progActions.Show();

                    await this.modulate.ArchiveSong(file, Readme.GenerateReadme(song), null, song.SongFolder);

                    await progActions.Close();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var song = (Song?)((Button?)sender)?.Tag;
            if (song == null || this.openedInfo == null)
            {
                return;
            }

            string? songPath = Path.Combine(this.openedInfo.SongsPath, song.SongFolder);

            var progActions = ProgressWindow.GetActions("Deleting Song", "Deleting: " + song.SongFolder, this);
            _ = progActions.Show();
            this.ChangesMade = true;
            try
            {
                await this.modulate.RemoveSong(song.SongFolder);
                _ = this.ViewModel.Songs.Remove(song);
            }
            catch (Exception ex)
            {
                await progActions.Close();
                progActions = null;
                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (progActions != null)
                {
                    await progActions.Close();
                }
            }
        }

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            string? consoleName = this.openedInfo.Console == UnpackedType.PS3 ? "ps3" : "ps4";
            string? headerName = $"main_{consoleName}.hdr";
            var saveDialog = new SaveFileDialog()
            {
                Filter = $"{headerName}|{headerName}",
                FileName = headerName,
                CheckPathExists = true
            };

            if (saveDialog.ShowDialog() == true)
            {
                var fi = new FileInfo(saveDialog.FileName);

                if (fi.Name != headerName)
                {
                    _ = MessageBox.Show($"File name does is not {headerName}", "Incorrect File Name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (fi.Directory!.FullName.ToLower() == this.openedInfo.HeaderPath.ToLower())
                {
                    _ = MessageBox.Show("Destination path cannot be the same as the source path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    var progActions = ProgressWindow.GetActions("Packing", "Packing, please wait.", this);
                    _ = progActions.Show();

                    await this.modulate.Pack(fi.Directory.FullName);

                    await progActions.Close();
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Title = this.openedInfo.FromUnpacked ? this.openedInfo.HeaderPath : this.openedInfo.SourcePath;
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool addedSong = false;

                string[] files = ((string[])e.Data.GetData(DataFormats.FileDrop)).Where(file => File.Exists(file)).ToArray();
                bool overwriteAll = false;

                foreach (string? file in files)
                {


                    if (file.EndsWith(".moggsong"))
                    {
                        ProgressWindow.ShowCloseActions? progActions = null;

                        try
                        {
                            string? songPath = file.Contains(Path.DirectorySeparatorChar) ? file[..(file.LastIndexOf(Path.DirectorySeparatorChar) + 1)] : "";
                            string? songName = file.Split(Path.DirectorySeparatorChar).Last();
                            songName = songName[..songName.LastIndexOf(".")];

                            if (overwriteAll == false && Directory.Exists(Path.Combine(this.openedInfo.SongsPath, songName)))
                            {
                                var actions = CustomMessageBoxWindow.GetActions(
                                            $"The song \"{songName}\" already exists, do you want to overwrite?",
                                            "Song Already Exists",
                                            new CustomMessageBoxWindow.Button[]
                                            {
                                                new CustomMessageBoxWindow.Button() { Label = "Yes", Result = 1 },
                                                new CustomMessageBoxWindow.Button() { Label = "Yes to All", Result = 2 },
                                                new CustomMessageBoxWindow.Button() { Label = "No", Result = 3 }
                                            }, this);
                                await actions.Show();

                                int result = (int)actions.ViewModel.Result;

                                if (result == 2)
                                {
                                    overwriteAll = true;
                                }

                                if (result == 3)
                                {
                                    continue;
                                }
                            }

                            progActions = ProgressWindow.GetActions("Adding Song", "Adding song: " + songName, this);
                            _ = (progActions?.Show());
                            this.ChangesMade = true;

                            await this.modulate.AddSong(songPath, songName, true);
                            addedSong = true;
                        }
                        catch (Exception ex)
                        {
                            if (progActions != null)
                            {
                                await progActions.Close();
                            }
                            _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            if (progActions != null)
                            {
                                await progActions.Close();
                            }
                        }

                        continue;
                    }

                    try
                    {

                        using var archive = ArchiveFactory.Open(file);
                        var entries = new Dictionary<string, IArchiveEntry>();

                        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                        {
                            entries.Add(entry.Key, entry);
                        }

                        foreach (string? msFile in entries.Keys.Where(f => f.EndsWith(".moggsong")))
                        {
                            ProgressWindow.ShowCloseActions? progActions = null;
                            try
                            {
                                string? songPath = msFile.Contains("/") ? msFile[..(msFile.LastIndexOf("/") + 1)] : "";
                                string? songName = msFile.Split("/").Last();
                                songName = songName[..songName.LastIndexOf(".")];

                                if (entries.ContainsKey($"{songPath}{songName}.mid") && entries.ContainsKey($"{songPath}{songName}.mogg"))
                                {
                                    if (overwriteAll == false && Directory.Exists(Path.Combine(this.openedInfo.SongsPath, songName)))
                                    {
                                        var actions = CustomMessageBoxWindow.GetActions(
                                            $"The song \"{songName}\" already exists, do you want to overwrite?",
                                            "Song Already Exists",
                                            new CustomMessageBoxWindow.Button[]
                                            {
                                                new CustomMessageBoxWindow.Button() { Label = "Yes", Result = 1 },
                                                new CustomMessageBoxWindow.Button() { Label = "Yes to All", Result = 2 },
                                                new CustomMessageBoxWindow.Button() { Label = "No", Result = 3 }
                                            }, this);
                                        await actions.Show();

                                        int result = (int)actions.ViewModel.Result;

                                        if (result == 2)
                                        {
                                            overwriteAll = true;
                                        }

                                        if (result == 3)
                                        {
                                            continue;
                                        }
                                    }

                                    using var temp = new EasyTempFolder(songName, App.SharedTemp);
                                    string? unpackedSong = Path.Combine(temp, songName);
                                    _ = Directory.CreateDirectory(unpackedSong);

                                    progActions = ProgressWindow.GetActions("Adding Song", "Adding song: " + songName, this);
                                    _ = (progActions?.Show());
                                    this.ChangesMade = true;

                                    entries[$"{songPath}{songName}.mid"].WriteToDirectory(unpackedSong);
                                    entries[$"{songPath}{songName}.mogg"].WriteToDirectory(unpackedSong);
                                    entries[msFile].WriteToDirectory(unpackedSong);

                                    await this.modulate.AddSong(unpackedSong, songName, true);
                                    addedSong = true;

                                }
                            }
                            catch (Exception ex)
                            {
                                if (progActions != null)
                                {
                                    await progActions.Close();
                                }
                                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            finally
                            {
                                if (progActions != null)
                                {
                                    await progActions.Close();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                if (addedSong)
                {
                    await this.UpdateSongs();
                }
            }
        }
        private async Task doSave(bool closeAfter = false)
        {
            try
            {
                var progActions = ProgressWindow.GetActions("Packing", "Packing, please wait.", this);
                _ = progActions.Show();

                await this.modulate.Pack(this.openedInfo.SourcePath);

                await progActions.Close();
                this.ChangesMade = false;

                if (closeAfter)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await this.doSave();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.promptSave == false)
            {
                return;
            }

            var result = MessageBox.Show("There are unsaved changes, would you like to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                e.Cancel = true;
                await this.doSave(true);
            }
        }

        private async void PackAllSongs_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog()
            {
                Filter = $"Zip Files (*.zip)|*.zip",
                FileName = $"Packed Songs.zip".ReplaceInvalidFilenameChars(),
                CheckPathExists = true
            };

            try
            {
                if (saveDialog.ShowDialog() == true)
                {
                    using var file = File.Create(saveDialog.FileName);

                    var progActions = ProgressWindow.GetActions("Archiving", "Archiving, please wait.", this);
                    progActions.ViewModel.Maximum.Value = this.ViewModel.Songs.Count - 1;
                    progActions.ViewModel.IsIndeterminate.Value = false;

                    _ = progActions.Show();

                    try
                    {
                        await this.modulate.ArchiveSong(file, Readme.GenerateReadme(this.ViewModel.Songs.ToArray()), (value, max) =>
                        {
                            progActions.ViewModel.Value.Value = value;
                            progActions.ViewModel.Maximum.Value = max;
                        }, this.ViewModel.Songs.Select(s => s.SongFolder).ToArray());
                    }
                    catch (Exception ex)
                    {
                        await progActions.Close();
                        progActions = null;

                        _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (progActions != null)
                    {
                        await progActions.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenBrowser(object sender, RoutedEventArgs e)
        {
            object tag = null;
            if (typeof(FrameworkContentElement).IsAssignableFrom(sender.GetType()))
            {
                tag = ((FrameworkContentElement)sender).Tag;
            }
            if (typeof(FrameworkElement).IsAssignableFrom(sender.GetType()))
            {
                tag = ((FrameworkElement)sender).Tag;
            }

            if (tag == null)
            {
                return;
            }

            if (tag.GetType() == typeof(string))
            {
                if (Uri.TryCreate(tag as string, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    _ = Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = uriResult.AbsoluteUri
                    });
                }
            }
        }

        private async void ReAddSongs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var progActions = ProgressWindow.GetActions("Processing", "Processing, please wait.", this);
                _ = progActions.Show();

                foreach (var song in this.ViewModel.Songs)
                {
                    await this.modulate.BuildSong(song.SongFolder);
                    await this.modulate.RemoveSong(song.SongFolder, false);
                }

                await this.modulate.AutoAdd();
                await this.UpdateSongs();

                await progActions.Close();
                this.ChangesMade = true;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnlockEverything_Click(object sender, RoutedEventArgs e)
        {
            var tweak = new UnlockEverything(this.openedInfo);
            bool newState = !this.ViewModel.EverythingUnlocked.Value;
            _ = tweak.SetState(newState);
            this.ViewModel.EverythingUnlocked.Value = newState;

            this.ChangesMade = true;
        }

        private void UnlockFPS_Click(object sender, RoutedEventArgs e)
        {
            var tweak = new FpsUnlimiter(this.openedInfo);
            bool newState = !this.ViewModel.FpsUnlimited.Value;
            _ = tweak.SetState(newState);
            this.ViewModel.FpsUnlimited.Value = newState;

            this.ChangesMade = true;
        }
    }
}
