using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace OldTaskbarPop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, WinEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);

            //form must be visible for pinvocation
            //settings are mostly random right now for testing
            garbage = new Form();
            garbage.StartPosition = FormStartPosition.Manual;
            garbage.Visible = false;
            garbage.Opacity = 1;
            garbage.Location = new Point(0, 0);
            garbage.Size = new Size(111, 111);
            garbage.ShowInTaskbar = false;
            garbage.FormBorderStyle = FormBorderStyle.None;
            
            Application.Run();
        }

        private static Form garbage;

        #region Detect_FocusChange
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        private static bool active = false;
        public static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            RECT rect = new RECT();
            GetWindowRect(new HandleRef(null, hwnd), ref rect);

            Point curs = new Point();
            GetCursorPos(ref curs);
            if (curs.X + 1 > Screen.PrimaryScreen.Bounds.Left &&
                curs.X - 1 < Screen.PrimaryScreen.Bounds.Right &&
                curs.Y + 1 > Screen.PrimaryScreen.Bounds.Top &&
                curs.Y - 1 < Screen.PrimaryScreen.Bounds.Bottom)
            {
                garbage.ShowInTaskbar = false;
                return;
            }

            //not on primary screen anymore
            if (rect.left + rect.right / 2 + 1> Screen.PrimaryScreen.Bounds.Right ||
                rect.top + rect.bottom / 2 - 1> Screen.PrimaryScreen.Bounds.Bottom ||
                rect.right - rect.left / 2 + 1< Screen.PrimaryScreen.Bounds.Left ||
                rect.bottom - rect.top / 2 - 1< Screen.PrimaryScreen.Bounds.Top)
            {
                ShowTaskbar(hwnd);
            }
        }
        #endregion

        #region Set_Taskbar_Foreground
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool FlashStatus);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static void ShowTaskbar(IntPtr currentHwnd)
        {
            IntPtr taskbar = FindWindow("Shell_TrayWnd", null);
            if (currentHwnd == taskbar)
            {
                return;
            }
            SetForegroundWindow(taskbar);
            SetForegroundWindow(currentHwnd);
            SetFocus(currentHwnd);
        }
        #endregion
    }
}