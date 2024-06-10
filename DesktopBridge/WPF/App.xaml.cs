using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

namespace WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        static Mutex mutex = new Mutex(true, "WPF_SINGLE_INSTANCE");
        
        
        App()
        {

            string[] args = Environment.GetCommandLineArgs();


            if (args.Length > 1)
            {
                // Assuming the first argument is the executable path
                // and the second argument onwards are the actual parameters
                string parameter = args[1];
                Debug.WriteLine("Parameter received: " + args.Length +" "+ args[args.Length-1]);


                if (args[args.Length - 1].Contains("parameter"))
                {
                    User32API.SendMessage((IntPtr)User32API.HWND_BROADCAST, User32API.WM_SHOWNOTI, IntPtr.Zero, IntPtr.Zero);

                    App.Current.Shutdown();
                }
            }

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                // Send Win32 message to shown currently existing instance window
                Console.WriteLine("Send Win32 message Synchronously or Aysnchronously");

                //If Main Window ShowInTaskbar == false, use SendMessage and if ShowInTaskbar == true then use Postmessage
                User32API.SendMessage((IntPtr)User32API.HWND_BROADCAST, User32API.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);

                //Console.WriteLine("Send Win32 message Asynchronously");
                //User32API.PostMessage((IntPtr)User32API.HWND_BROADCAST, User32API.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);

                App.Current.Shutdown();

            }
            else
            {

                // First Time Entry, Created a Tray Icon.


            }

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            MainWindow = new MainWindow();

            MainWindow.Show();
        }

    }

}
