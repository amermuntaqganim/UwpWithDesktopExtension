using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DesktopBridge
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// When app is loaded, kick off the desktop process
        /// and listen to app service connection events
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                App.AppServiceConnected += MainPage_AppServiceConnected;
                App.AppServiceDisconnected += MainPage_AppServiceDisconnected;
                 await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();

            }

            if (App.Connection != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // enable UI to access  the connection
                    btnRegKey.IsEnabled = true;
                });
            }
        }

        private async void MainPage_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {

            LogManager.Instance.WriteLogs("App Service Connected");
            //App.Connection.RequestReceived += AppServiceConnection_RequestReceived;

            if (e is null)
            {
                LogManager.Instance.WriteLogs(" Argument Connection Is NULL");
            }
            e.AppServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // enable UI to access  the connection
                btnRegKey.IsEnabled = true;
            });
        }

        /// <summary>
        /// When the desktop process is disconnected, reconnect if needed
        /// </summary>
        private async void MainPage_AppServiceDisconnected(object sender, EventArgs e)
        {

            LogManager.Instance.WriteLogs($"ON UWP Disconnected ");
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // disable UI to access the connection
                btnRegKey.IsEnabled = false;


            });
        }

        /// <summary>
        /// Send request to query the registry
        /// </summary>
        private async void btnClick_ReadKey(object sender, RoutedEventArgs e)
        {
            ValueSet request = new ValueSet();
            request.Add("KEY", tbKey.Text);
            AppServiceResponse response = await App.Connection.SendMessageAsync(request);

            // display the response key/value pairs
            tbResult.Text = "";
            foreach (string key in response.Message.Keys)
            {
                tbResult.Text += key + " = " + response.Message[key] + "\r\n";
            }

            // Show a Custom Notification in WPF
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Parameters");
        }

        /// <summary>
        /// Handle calculation request from desktop process
        /// (dummy scenario to show that connection is bi-directional)
        /// </summary>
        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            LogManager.Instance.WriteLogs(" On Request Recieved ");
            double d1 = (double)args.Request.Message["D1"];
            double d2 = (double)args.Request.Message["D2"];
            double result = d1 + d2;

            ValueSet response = new ValueSet();
            response.Add("RESULT", result);
            await args.Request.SendResponseAsync(response);

            // log the request in the UI for demo purposes
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbRequests.Text += string.Format("Request: {0} + {1} --> Response = {2}\r\n", d1, d2, result);
            });
        }
    }
}
