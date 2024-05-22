using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF
{
    internal class User32API
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern bool SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);



        [DllImport("user32", EntryPoint = "GetWindowLongA")]
        public static extern long GetWindowLong(long hwnd, long nIndex);

        [DllImport("user32", EntryPoint = "SetWindowLongA")]
        public static extern long SetWindowLong(long hwnd, long nIndex, long dwNewLong);

        [DllImport("user32")]
        public static extern long SetWindowPos(long hwnd, long hWndInsertAfter, long x, long y, long cx, long cy,
                                               long wFlags);

        [DllImport("User32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);
    }
}
