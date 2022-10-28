using AmpHelper;
using AmpHelper.Enums;
using AmpHelper.Types;
using DanTheMan827.ModulateDotNet;
using DanTheMan827.Modulation.Extensions;
using DanTheMan827.Modulation.Helpers;
using DanTheMan827.TempFolders;
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
        private UnpackedInfo openedInfo => ViewModel.OpenedInfo;
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

            var songs = (await Song.GetSongsAsync(openedInfo.UnpackedPath)).Where(e => !e.Special && !e.BaseSong).ToArray();

            foreach (var song in songs.OrderBy(s => $"{s.CleanArtist()}{s.CleanName()}"))
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
                this.ViewModel.Songs.Add(new MoggSong()
                {
                    Title = "My Song",
                    MoggPath = "../Songs/mysong/mysong.moggsong",
                    ArenaPath = "World1",
                    UnlockRequirement = UnlockRequirement.PlayCount
                });

                this.ViewModel.ShowSongs.Value = true;
            }
        }

        private async void ButtonPack_Click(object sender, RoutedEventArgs e)
        {
            var song = (MoggSong?)((Button?)sender)?.Tag;
            if (song == null || this.openedInfo == null)
            {
                return;
            }

            var saveDialog = new SaveFileDialog()
            {
                Filter = $"Zip Files (*.zip)|*.zip",
                FileName = $"{song.CleanArtist()} - {song.CleanName()}.zip".ReplaceInvalidFilenameChars(),
                CheckPathExists = true
            };

            try
            {
                if (saveDialog.ShowDialog() == true)
                {
                    using var file = File.Create(saveDialog.FileName);

                    var progActions = ProgressWindow.GetActions("Archiving", "Archiving, please wait.", this);

                    _ = progActions.Show();

                    await HelperMethods.ArchiveSong(openedInfo.UnpackedPath, file, Readme.GenerateReadme(song), null, song.ID);

                    await progActions.Close();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var song = (MoggSong?)((Button?)sender)?.Tag;
            if (song == null || this.openedInfo == null)
            {
                return;
            }

            string? songPath = Path.Combine(this.openedInfo.SongsPath, song.ID);

            var progActions = ProgressWindow.GetActions("Deleting Song", "Deleting: " + song.ID, this);
            _ = progActions.Show();
            this.ChangesMade = true;
            try
            {
                await Song.RemoveSongAsync(openedInfo.UnpackedPath, song.ID, true);

                _ = this.ViewModel.Songs.Remove(song);
            }
            catch (Exception ex)
            {
                await progActions.Close();
                progActions = null;
                _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    progActions.ViewModel.IsIndeterminate.Value = false;
                    _ = progActions.Show();
                    await Ark.PackAsync(openedInfo.UnpackedPath, fi.FullName, (message, current, max) =>
                    {
                        progActions.ViewModel.Maximum.Value = max;
                        progActions.ViewModel.Value.Value = current;
                    });

                    await progActions.Close();
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string[] files = ((string[])e.Data.GetData(DataFormats.FileDrop)).Where(file => File.Exists(file)).ToArray();

                _ = ImportFiles(files);
            }
        }

        private void ImportSongs_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Supported files (*.moggsong, *.zip, *.7z, *.rar)|*.moggsong;*.zip;*.7z;*.rar",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                _ = ImportFiles(ofd.FileNames);
            }
        }

        private async Task ImportFiles(string[] files)
        {
            bool addedSong = false;
            bool overwriteAll = false;

            var progActions = ProgressWindow.GetActions("Adding Song", "", this);
            _ = progActions.Show();
            foreach (string? file in files.OrderBy(e => e))
            {
                if (file.EndsWith(".moggsong"))
                {
                    try
                    {
                        string? songPath = file.Contains(Path.DirectorySeparatorChar) ? file[..(file.LastIndexOf(Path.DirectorySeparatorChar) + 1)] : "";
                        string? songName = file.Split(Path.DirectorySeparatorChar).Last();
                        songName = songName[..songName.LastIndexOf(".")];

                        progActions.ViewModel.Message.Value = "Adding song: " + songName;


                        if (overwriteAll == false && Directory.Exists(Path.Combine(this.openedInfo.SongsPath, songName)))
                        {
                            await progActions.Close();
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
                            _ = progActions.Show();

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

                        this.ChangesMade = addedSong = true;

                        await Song.ImportSongAsync(openedInfo.UnpackedPath, file, true);
                    }
                    catch (Exception ex)
                    {
                        await progActions.Close();
                        _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    foreach (string? msFile in entries.Keys.Where(f => f.EndsWith(".moggsong")).OrderBy(f => f))
                    {
                        try
                        {
                            string? songPath = msFile.Contains("/") ? msFile[..(msFile.LastIndexOf("/") + 1)] : "";
                            string? songName = msFile.Split("/").Last();
                            songName = songName[..songName.LastIndexOf(".")];

                            progActions.ViewModel.Message.Value = "Adding song: " + songName;

                            if (entries.ContainsKey($"{songPath}{songName}.mid") && entries.ContainsKey($"{songPath}{songName}.mogg"))
                            {
                                if (overwriteAll == false && Directory.Exists(Path.Combine(this.openedInfo.SongsPath, songName)))
                                {
                                    await progActions.Close();
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

                                    _ = progActions.Show();

                                    if (result == 2)
                                    {
                                        overwriteAll = true;
                                    }

                                    if (result == 3)
                                    {
                                        continue;
                                    }
                                }

                                await Task.Run(() =>
                                {
                                    using var temp = new EasyTempFolder(songName, App.SharedTemp);
                                    string unpackedSong = Path.Combine(temp, songName);

                                    _ = Directory.CreateDirectory(unpackedSong);

                                    this.ChangesMade = addedSong = true;

                                    entries[$"{songPath}{songName}.mid"].WriteToDirectory(unpackedSong);
                                    entries[$"{songPath}{songName}.mogg"].WriteToDirectory(unpackedSong);
                                    entries[msFile].WriteToDirectory(unpackedSong);

                                    Song.ImportSongAsync(openedInfo.UnpackedPath, unpackedSong, true).Wait();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            await progActions.Close();
                            _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            await progActions?.Close();

            if (addedSong)
            {
                await this.UpdateSongs();
            }
        }
        private async Task doSave(bool closeAfter = false)
        {
            try
            {
                var progActions = ProgressWindow.GetActions("Packing", "Packing, please wait.", this);
                progActions.ViewModel.IsIndeterminate.Value = false;
                _ = progActions.Show();

                await Ark.PackAsync(openedInfo.UnpackedPath, openedInfo.HeaderPath, (message, current, max) =>
                {
                    progActions.ViewModel.Maximum.Value = max;
                    progActions.ViewModel.Value.Value = current;
                });

                await progActions.Close();
                this.ChangesMade = false;

                if (closeAfter)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        await HelperMethods.ArchiveSong(openedInfo.UnpackedPath, file, Readme.GenerateReadme(this.ViewModel.Songs.ToArray()), (value, max) =>
                        {
                            progActions.ViewModel.Value.Value = value;
                            progActions.ViewModel.Maximum.Value = max;
                        }, ViewModel.Songs.Select(e => e.ID).ToArray());
                    }
                    catch (Exception ex)
                    {
                        await progActions.Close();
                        progActions = null;

                        _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (progActions != null)
                    {
                        await progActions.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                progActions.ViewModel.IsIndeterminate.Value = false;
                _ = progActions.Show();

                var songNames = (await Song.GetSongsAsync(openedInfo.UnpackedPath)).Where(e => !e.Special && !e.BaseSong).Select(e => e.ID).ToArray();
                var consoleType = openedInfo.Console == UnpackedType.PS3 ? ConsoleType.PS3 : ConsoleType.PS4;

                progActions.ViewModel.Maximum.Value = songNames.Length * 2 + 2;

                await Song.AddSongAsync(openedInfo.UnpackedPath, songNames, consoleType, (message, current, max) =>
                {
                    progActions.ViewModel.Maximum.Value = max;
                    progActions.ViewModel.Value.Value = current;
                });

                await this.UpdateSongs();

                await progActions.Close();
                this.ChangesMade = true;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.NiceError(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void Tweak_Click(object sender, RoutedEventArgs e)
        {
            TweakWrapper? tag = null;

            if (typeof(FrameworkContentElement).IsAssignableFrom(sender.GetType()))
            {
                tag = ((FrameworkContentElement)sender).Tag as TweakWrapper;
            }

            if (typeof(FrameworkElement).IsAssignableFrom(sender.GetType()))
            {
                tag = ((FrameworkElement)sender).Tag as TweakWrapper;
            }

            if (tag != null)
            {
                var status = tag.ToggleTweak();

                MessageBox.Show(this, status, "Tweak Updated");
            }
        }
    }
}
