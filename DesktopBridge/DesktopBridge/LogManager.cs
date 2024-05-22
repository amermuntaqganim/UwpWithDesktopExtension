using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopBridge
{
    public class LogManager
    {

        private static readonly Lazy<LogManager> lazyInstance =
        new Lazy<LogManager>(() => new LogManager());
        public static LogManager Instance => lazyInstance.Value;

        public void WriteLogs(string logmessage)
        {
            //string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            string directoryPath = Environment.GetFolderPath
                            (Environment.SpecialFolder.LocalApplicationData);
            string fileName = "DesktopBridgeUWP.txt";
            string filePath = Path.Combine(directoryPath, fileName);
            Console.WriteLine(filePath);

            using (StreamWriter sw = new StreamWriter(File.Open(filePath, System.IO.FileMode.Append)))
            {
                sw.WriteLineAsync(DateTime.Now + " UWP : " + logmessage);
            }
        }
    }
}
