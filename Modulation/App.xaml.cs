using AmpHelper;
using AmpHelper.Enums;
using AmpHelper.Helpers;
using AmpHelper.Interfaces;
using DanTheMan827.ModulateDotNet;
using DanTheMan827.Modulation.Views;
using DanTheMan827.TempFolders;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DanTheMan827.Modulation
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool IsDesign => App.Current is not App;
        public static EasyTempFolder SharedTemp = new("Modulation");
        public EasyTempFolder? UnpackedTemp = null;
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            string? openedPath = null;

            if (e.Args.Length >= 1)
            {
                var info = new FileInfo(e.Args[0]);

                if (info.Exists && (info.Name == "main_ps3.hdr" || info.Name == "main_ps4.hdr"))
                {
                    openedPath = info.Directory?.FullName;
                }
            }

            if (openedPath == null)
            {
                var ofd = new OpenFileDialog()
                {
                    Filter = "main_ps3.hdr / main_ps4.hdr|main_ps3.hdr;main_ps4.hdr"
                };

                if (ofd.ShowDialog() == true)
                {
                    var info = new FileInfo(ofd.FileName);

                    if (info.Exists && (info.Name == "main_ps3.hdr" || info.Name == "main_ps4.hdr"))
                    {
                        openedPath = info.Directory?.FullName;
                    }
                }
            }

            if (openedPath == null)
            {
                Application.Current.Shutdown();
                return;
            }


            var folderUnpackedState = new UnpackedInfo();
            folderUnpackedState.Console = HelperMethods.ConsoleTypeFromPath(openedPath) == ConsoleType.PS3 ? UnpackedType.PS3 : UnpackedType.PS4;
            folderUnpackedState.FromUnpacked = Directory.Exists(Path.Combine(openedPath, folderUnpackedState.Console == UnpackedType.PS3 ? "ps3" : "ps4"));
            folderUnpackedState.ConsoleLabel = folderUnpackedState.Console.ToString().ToLower();
            folderUnpackedState.UnpackedPath = openedPath;
            folderUnpackedState.SourcePath = openedPath;
            folderUnpackedState.HeaderPath = Path.Combine(openedPath, folderUnpackedState.Console == UnpackedType.PS3 ? "main_ps3.hdr" : "main_ps4.hdr");
            folderUnpackedState.ExitCode = 0;

            var mainWindow = new SongsWindow();

            try
            {
                if (folderUnpackedState.FromUnpacked == false)
                {
                    var progressActions = ProgressWindow.GetActions("Unpacking", "Unpacking ark files to temporary folder.");
                    progressActions.ViewModel.IsIndeterminate.Value = false;
                    progressActions.Show();

                    folderUnpackedState.UnpackedPath = this.UnpackedTemp = new EasyTempFolder("Unpacked", SharedTemp, false);

                    await Task.Run(() =>
                    {
                        Ark.Unpack(folderUnpackedState.HeaderPath, folderUnpackedState.UnpackedPath, false, false, (message, current, max) =>
                        {
                            progressActions.ViewModel.Maximum.Value = max;
                            progressActions.ViewModel.Value.Value = current;
                        });
                    });

                    await progressActions.Close();
                }

                mainWindow.ViewModel.SaveVisibility.Value = folderUnpackedState.FromUnpacked ? Visibility.Collapsed : Visibility.Visible;
                mainWindow.ViewModel.OpenedInfo = folderUnpackedState;

                foreach (var tweak in ITweak.GetTweaks().OrderBy(e => e.Name).Select(e => new TweakWrapper(folderUnpackedState.UnpackedPath, e)))
                {
                    mainWindow.TweaksMenu.Visibility = Visibility.Visible;
                    var tweakMenu = new MenuItem()
                    {
                        Header = tweak.Name,
                        ToolTip = tweak.Description,
                        IsCheckable = true,
                        IsChecked = tweak.Enabled,
                        Tag = tweak
                    };

                    tweakMenu.Click += mainWindow.Tweak_Click;
                    mainWindow.TweaksMenu.Items.Add(tweakMenu);
                }

                Application.Current.MainWindow = mainWindow;
                await mainWindow.UpdateSongs();

                mainWindow.Show();
            }
            catch (TaskCanceledException)
            {
                Application.Current.Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            this.UnpackedTemp?.Dispose();
            SharedTemp.Dispose();
        }
    }
}
