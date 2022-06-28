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
            public Func<Task> Show;
            public Func<Task> Close;
            public ProgressWindow? WindowReference;

            public ShowCloseActions()
            {
                this.Show = async () => { };
                this.Close = async () => { };
                
            }
        }

        public static ShowCloseActions GetActions(string title, string message, Window? owner = null, WindowStartupLocation ownedStartupLocation = WindowStartupLocation.CenterOwner)
        {
            ProgressWindow? progWnd = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                progWnd = new ProgressWindow(title, message, owner, ownedStartupLocation);
            });

            var actions = new ShowCloseActions
            {
                Show = () => Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _ = (progWnd?.ShowDialog());
                    });
                }),

                Close = async () =>
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        progWnd?.Close();
                    });
                },

                WindowReference = progWnd
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
