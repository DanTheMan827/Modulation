using DanTheMan827.Modulation.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace WPFCustomMessageBox
{
    /// <summary>
    /// Interaction logic for ModalDialog.xaml
    /// </summary>
    public partial class CustomMessageBoxWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public class ShowCloseActions
        {
            public Func<Task> Show;
            public Func<Task> Close;
            public CustomMessageBoxWindowViewModel ViewModel;

            public ShowCloseActions()
            {
                this.Show = async () => { };
                this.Close = async () => { };

            }
        }

        public static ShowCloseActions GetActions(string message, string caption, Button[] buttons, Window? owner = null, WindowStartupLocation ownedStartupLocation = WindowStartupLocation.CenterOwner)
        {
            CustomMessageBoxWindow? progWnd = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                progWnd = new CustomMessageBoxWindow(message, caption, buttons);
                if (owner != null)
                {
                    progWnd.Owner = owner;
                    progWnd.WindowStartupLocation = ownedStartupLocation;
                }
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

                ViewModel = progWnd!.ViewModel
            };

            return actions;
        }
        public class Button
        {
            public string? Label { get; set; }
            public object? Result { get; set; }
        }



        internal CustomMessageBoxWindow(string message, string caption, params Button[] buttons)
        {
            this.InitializeComponent();

            this.ViewModel.Message.Value = message;
            this.ViewModel.Caption.Value = caption;
            foreach (var button in buttons)
            {
                this.ViewModel.Buttons.Add(button);
            }

            this.ViewModel.Result = MessageBoxResult.None;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Result = ((Control)sender).Tag;
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            _ = SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}