using DanTheMan827.Modulation.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace DanTheMan827.Modulation.Views
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public class ShowCloseActions
        {
            public Func<Task> Show { get; set; }
            public Func<Task> Close { get; set; }
            internal ProgressWindow Window { get; set; }
            internal ProgressWindowViewModel OldModel { get; set; }
            public ProgressWindowViewModel ViewModel => OldModel ?? Window.ViewModel;

            public ShowCloseActions()
            {
                this.Show = async () => { };
                this.Close = async () => { };
            }
        }

        public static ShowCloseActions GetActions(string title, string message, Window? owner = null, WindowStartupLocation ownedStartupLocation = WindowStartupLocation.CenterOwner)
        {


            var actions = new ShowCloseActions();

            var newWnd = () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    actions.Window = new ProgressWindow(title, message, owner, ownedStartupLocation);

                    if (actions.OldModel != null)
                    {
                        actions.Window.ViewModel.Title.Value = actions.OldModel.Title.Value;
                        actions.Window.ViewModel.Message.Value = actions.OldModel.Message.Value;
                        actions.Window.ViewModel.IsIndeterminate.Value = actions.OldModel.IsIndeterminate.Value;
                        actions.Window.ViewModel.Minimum.Value = actions.OldModel.Minimum.Value;
                        actions.Window.ViewModel.Maximum.Value = actions.OldModel.Maximum.Value;
                        actions.Window.ViewModel.Value.Value = actions.OldModel.Value.Value;

                        actions.OldModel = null;
                    }
                });
            };

            newWnd();

            actions.Show = () => Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (actions.Window == null)
                    {
                        newWnd();
                    }

                    _ = (actions.Window?.ShowDialog());
                });
            });

            actions.Close = async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    actions.OldModel = actions.Window?.ViewModel;
                    actions.Window?.Close();
                    actions.Window = null;
                });
            };

            return actions;
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public ProgressWindow()
        {
            this.InitializeComponent();
        }

        public ProgressWindow(string title, string message, Window? owner = null, WindowStartupLocation ownedStartupLocation = WindowStartupLocation.CenterOwner)
        {
            this.InitializeComponent();
            this.ViewModel.Title.Value = title;
            this.ViewModel.Message.Value = message;

            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = ownedStartupLocation;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            _ = SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }


    }
}
