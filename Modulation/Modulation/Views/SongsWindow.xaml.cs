using DanTheMan827.ModulateDotNet;
using DanTheMan827.TempFolders;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DtxCS;
using System.Diagnostics;
using DtxCS.DataTypes;
using System.Collections.Generic;
using DanTheMan827.Modulation.Extensions;
using Microsoft.Win32;
using SharpCompress.Archives;

namespace DanTheMan827.Modulation.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SongsWindow : Window
    {
        public bool ChangesMade { get; set; } = false;
        private bool promptSave => ViewModel.OpenedInfo?.FromUnpacked == false && ChangesMade == true;
        
        public SongsWindow()
        {
            InitializeComponent();

            ViewModel.Songs.CollectionChanged += Songs_CollectionChanged;
        }

        private void Songs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ViewModel.ShowSongs.Value = ViewModel.Songs.Count > 0;
        }

        public async Task UpdateSongs(UnpackedInfo? openedInfo = null)
        {
            if (ViewModel.OpenedInfo == null)
            {
                ViewModel.OpenedInfo = openedInfo;
            }

            if (ViewModel.OpenedInfo == null)
            {
                throw new ArgumentNullException(nameof(openedInfo));
            }

            ViewModel.Songs.Clear();

            var songs = (await Modulate.ListSongs(ViewModel.OpenedInfo)).Where(s => !Modulate.baseSongs.Contains(s.SongFolder));

            foreach (Song song in songs)
            {
                using (var dtxStream = File.OpenRead(Path.Combine(ViewModel.OpenedInfo.SongsPath, song.SongFolder, $"{song.SongFolder}.moggsong")))
                {
                    var root = DTX.FromDtaStream(dtxStream);
                    song.MoggSong = new MoggSong().LoadDta(root);
                }
            }

            foreach (var song in songs.OrderBy(s => $"{s.CleanArtist}{s.CleanName}"))
            {
                ViewModel.Songs.Add(song);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await UpdateSongs();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            if (App.IsDesign)
            {
                ViewModel.Songs.Add(new Song()
                {
                    ID = "MYSONG",
                    Name = "My Song",
                    Path = "../Songs/mysong/mysong.moggsong",
                    Arena = "World1",
                    Type = "kExtraSong",
                    UnlockType = "play_num",
                    UnlockValue = "0"
                });
                ViewModel.ShowSongs.Value = true;
            }
        }

        private async void ButtonPack_Click(object sender, RoutedEventArgs e)
        {
            var song = ((Song?)((Button?)sender)?.Tag);
            if (song == null || ViewModel.OpenedInfo == null) return;

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

                    progActions.Show();

                    await Task.Run(async () =>
                    {
                        Modulate.ArchiveSong(ViewModel.OpenedInfo, song.SongFolder, file, AppResources.ZipReadme);
                    });

                    await progActions.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var song = ((Song?)((Button?)sender)?.Tag);
            if (song == null || ViewModel.OpenedInfo == null) return;

            var songPath = Path.Combine(ViewModel.OpenedInfo.SongsPath, song.SongFolder);

            ProgressWindow.ShowCloseActions? progActions = ProgressWindow.GetActions("Deleting Song", "Deleting: " + song.SongFolder, this);
            progActions.Show();
            ChangesMade = true;
            try
            {
                await Modulate.RemoveSong(ViewModel.OpenedInfo, song.SongFolder);
                ViewModel.Songs.Remove(song);
            }
            catch (Exception ex)
            {
                await progActions.Close();
                progActions = null;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var consoleName = ViewModel.OpenedInfo.Console == UnpackedType.PS3 ? "ps3" : "ps4";
            var headerName = $"main_{consoleName}.hdr";
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
                    MessageBox.Show($"File name does is not {headerName}", "Incorrect File Name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (fi.Directory.FullName.ToLower() == ViewModel.OpenedInfo.HeaderPath.ToLower())
                {
                    MessageBox.Show("Destination path cannot be the same as the source path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    var progActions = ProgressWindow.GetActions("Packing", "Packing, please wait.", this);
                    progActions.Show();

                    await Task.Run(async () =>
                    {
                        await Modulate.Pack(ViewModel.OpenedInfo, fi.Directory.FullName);
                    });

                    await progActions.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Title = ViewModel.OpenedInfo.FromUnpacked ? ViewModel.OpenedInfo.HeaderPath : ViewModel.OpenedInfo.SourcePath;
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var addedSong = false;

                // Note that you can have more than one file.
                string[] files = ((string[])e.Data.GetData(DataFormats.FileDrop)).Where(file => File.Exists(file)).ToArray();

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                foreach (var file in files)
                {
                    

                    if (file.EndsWith(".moggsong"))
                    {
                        ProgressWindow.ShowCloseActions? progActions = null;

                        try
                        {
                            var songPath = file.Contains(Path.DirectorySeparatorChar) ? file.Substring(0, file.LastIndexOf(Path.DirectorySeparatorChar) + 1) : "";
                            var songName = file.Split(Path.DirectorySeparatorChar).Last();
                            songName = songName.Substring(0, songName.LastIndexOf("."));

                            if (Directory.Exists(Path.Combine(ViewModel.OpenedInfo.SongsPath, songName)))
                            {
                                if (MessageBox.Show($"The song \"{songName}\" already exists, do you want to overwrite?", "Song Already Exists", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                {
                                    continue;
                                }
                            }

                            progActions = ProgressWindow.GetActions("Adding Song", "Adding song: " + songName, this);
                            progActions?.Show();
                            ChangesMade = true;

                            await Modulate.AddSong(ViewModel.OpenedInfo, songPath, songName, true);
                            addedSong = true;
                        }
                        catch (Exception ex)
                        {
                            if (progActions != null)
                            {
                                await progActions.Close();
                            }
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        foreach (var msFile in entries.Keys.Where(f => f.EndsWith(".moggsong")))
                        {
                            ProgressWindow.ShowCloseActions? progActions = null;
                            try
                            {
                                var songPath = msFile.Contains("/") ? msFile.Substring(0, msFile.LastIndexOf("/") + 1) : "";
                                var songName = msFile.Split("/").Last();
                                songName = songName.Substring(0, songName.LastIndexOf("."));

                                if (entries.ContainsKey($"{songPath}{songName}.mid") && entries.ContainsKey($"{songPath}{songName}.mogg"))
                                {
                                    if (Directory.Exists(Path.Combine(ViewModel.OpenedInfo.SongsPath, songName)))
                                    {
                                        if (MessageBox.Show($"The song \"{songName}\" already exists, do you want to overwrite?", "Song Already Exists", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                        {
                                            continue;
                                        }
                                    }

                                    using var temp = new EasyTempFolder(songName, App.SharedTemp);
                                    var unpackedSong = Path.Combine(temp, songName);
                                    Directory.CreateDirectory(unpackedSong);

                                    progActions = ProgressWindow.GetActions("Adding Song", "Adding song: " + songName, this);
                                    progActions?.Show();
                                    ChangesMade = true;

                                    entries[$"{songPath}{songName}.mid"].WriteToDirectory(unpackedSong);
                                    entries[$"{songPath}{songName}.mogg"].WriteToDirectory(unpackedSong);
                                    entries[msFile].WriteToDirectory(unpackedSong);

                                    await Modulate.AddSong(ViewModel.OpenedInfo, unpackedSong, songName, true);
                                    addedSong = true;
                                    
                                }
                            } 
                            catch (Exception ex)
                            {
                                if (progActions != null)
                                {
                                    await progActions.Close();
                                }
                                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                if (addedSong)
                {
                    await UpdateSongs();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ChangesMade = false;
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (promptSave == false) return;

            var result = MessageBox.Show("There are unsaved changes, would you like to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                e.Cancel = true;
                try
                {
                    var progActions = ProgressWindow.GetActions("Packing", "Packing, please wait.", this);
                    progActions.Show();

                    await Task.Run(async () =>
                    {
                        await Modulate.Pack(ViewModel.OpenedInfo, ViewModel.OpenedInfo.SourcePath);
                    });

                    await progActions.Close();
                    ChangesMade = false;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
