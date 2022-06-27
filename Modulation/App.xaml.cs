using DanTheMan827.ModulateDotNet;
using DanTheMan827.Modulation.Views;
using DanTheMan827.TempFolders;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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
            UnpackedInfo OpenedInfo = null;
            ModulateExe.TempBasePath = SharedTemp;

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

            var folderUnpackedState = Modulate.FromUnpacked(openedPath);

            ProgressWindow.ShowCloseActions? progressActions = null;
            if (folderUnpackedState == null)
            {
                progressActions = ProgressWindow.GetActions("Unpacking", "Unpacking ark files to temporary folder.");
                _ = (progressActions?.Show());
            }


            var mainWindow = new SongsWindow();

            try
            {
                if (folderUnpackedState != null)
                {
                    OpenedInfo = folderUnpackedState;
                }
                else
                {
                    this.UnpackedTemp = new EasyTempFolder("Unpacked", SharedTemp);
                    await Task.Run(async () =>
                    {
                        var info = await Modulate.Unpack(openedPath, this.UnpackedTemp.Path);
                        OpenedInfo = info;
                    });

                    if (progressActions != null)
                    {
                        await progressActions.Close();
                    }
                }
                mainWindow.ViewModel.Modulate = new Modulate(OpenedInfo);
                mainWindow.ViewModel.SaveVisibility.Value = OpenedInfo!.FromUnpacked ? Visibility.Collapsed : Visibility.Visible;
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
            ModulateExe.Shared.Dispose();
            this.UnpackedTemp?.Dispose();
            SharedTemp.Dispose();
        }
    }
}
