using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;


namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon? _notifyIcon = null;
        private AppServiceConnection connection = null;
        private double d1, d2;

        public MainWindow()
        {
            InitializeComponent();
            CreateTrayIcon();
            InitializeAppServiceConnection();
        }

        private void CreateTrayIcon()
        {
            var iconPath = System.AppDomain.CurrentDomain.BaseDirectory + "myicon.ico";
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon(iconPath); // Make sure to add an icon to your project
            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, Show_Click);
            contextMenu.Items.Add("Exit", null, Exit_Click);
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void Show_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            _notifyIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            //_notifyIcon.Dispose();
            base.OnClosed(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the closing event
            this.Hide();     // Hide the main window
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            if (msg == User32API.WM_SHOWME)
            {
                Console.WriteLine("Existing Window");
                if (this.WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                if (this.Visibility == Visibility.Hidden || this.Visibility == Visibility.Collapsed)
                {
                    Show();
                }
            }

            if (msg == User32API.WM_SHOWNOTI)
            {

                NotificationWindow notiwindows = new NotificationWindow();
                notiwindows.Show();
            }

            return IntPtr.Zero;
        }

        private async void InitializeAppServiceConnection()
        {
            LogManager.Instance.WriteLogs(" Initialize App Service connection");

            connection = new AppServiceConnection();
            connection.AppServiceName = "SampleInteropService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                LogManager.Instance.WriteLogs(" App Service connection Failed");
                // something went wrong ...
                System.Windows.MessageBox.Show(status.ToString());
                this.IsEnabled = false;
            }
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            LogManager.Instance.WriteLogs($"AppService Disconnected From WPF {args.ToString()}");
            // connection to the UWP lost, so we shut down the desktop process
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                //Application.Current.Shutdown();
                InitializeAppServiceConnection();
            }));

            LogManager.Instance.WriteLogs(" On Connection closed service");
        }

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // retrive the reg key name from the ValueSet in the request
            string key = args.Request.Message["KEY"] as string;
            int index = key.IndexOf('\\');
            if (index > 0)
            {
                // read the key values from the respective hive in the registry
                string hiveName = key.Substring(0, key.IndexOf('\\'));
                string keyName = key.Substring(key.IndexOf('\\') + 1);
                RegistryHive hive = RegistryHive.ClassesRoot;

                switch (hiveName)
                {
                    case "HKLM":
                        hive = RegistryHive.LocalMachine;
                        break;
                    case "HKCU":
                        hive = RegistryHive.CurrentUser;
                        break;
                    case "HKCR":
                        hive = RegistryHive.ClassesRoot;
                        break;
                    case "HKU":
                        hive = RegistryHive.Users;
                        break;
                    case "HKCC":
                        hive = RegistryHive.CurrentConfig;
                        break;
                }

                using (RegistryKey regKey = RegistryKey.OpenRemoteBaseKey(hive, "").OpenSubKey(keyName))
                {
                    // compose the response as ValueSet
                    ValueSet response = new ValueSet();
                    if (regKey != null)
                    {
                        foreach (string valueName in regKey.GetValueNames())
                        {
                            response.Add(valueName, regKey.GetValue(valueName).ToString());
                        }
                    }
                    else
                    {
                        response.Add("ERROR", "KEY NOT FOUND");
                    }
                    // send the response back to the UWP
                    await args.Request.SendResponseAsync(response);
                }
            }
            else
            {
                ValueSet response = new ValueSet();
                response.Add("ERROR", "INVALID REQUEST");
                await args.Request.SendResponseAsync(response);
            }
        }

        /// <summary>
        /// Sends a request to the UWP app
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // ask the UWP to calculate d1 + d2
            ValueSet request = new ValueSet();
            request.Add("D1", d1);
            request.Add("D2", d2);
            AppServiceResponse response = await connection.SendMessageAsync(request);
            double result = (double)response.Message["RESULT"];
            tbResult.Text = result.ToString();
        }

        /// <summary>
        /// Determines whether the "equals" button should be enabled
        /// based on input in the text boxes
        /// </summary>
        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(tb1.Text, out d1) && double.TryParse(tb2.Text, out d2))
            {
                btnCalc.IsEnabled = true;
            }
            else
            {
                btnCalc.IsEnabled = false;
            }
        }
    }
}