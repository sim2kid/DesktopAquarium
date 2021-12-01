using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Background.Windows
{
    public static class Wallpaper
    {
        /// <summary>
        /// Returns the IntPtr related to the current active window.
        /// </summary>
        public static IntPtr CurrentWindow => GetActiveWindow();
        /// <summary>
        /// Returns the IntPtr related to the parent of the current active window
        /// </summary>
        public static IntPtr ParentWindow => GetParent(CurrentWindow);
        /// <summary>
        /// The number of screens on the system
        /// </summary>
        public static int ScreenCount => Screen.AllScreens.Length;

        /// <summary>
        /// This will ask windows for the IntPtr that refers to the 'Progman' window.
        /// </summary>
        private static IntPtr Progman => FindWindow("Progman", null);
        /// <summary>
        /// Do not use. This variable is never gaurenteed to have a value
        /// </summary>
        private static IntPtr _workerw = IntPtr.Zero;
        /// <summary>
        /// The IntPtr to the WorkerW window where the background lies
        /// </summary>
        private static IntPtr WorkerW 
        {
            get 
            {
                if (_workerw == IntPtr.Zero)
                {
                    createWorkerW(Progman);
                    EnumWindows(windowEnum, IntPtr.Zero);
                }
                return _workerw;
            }
            set 
            {
                _workerw = IntPtr.Zero;
            }
        }



        /// <summary>
        /// Sets <paramref name="originalParent"/> as the new parent of <paramref name="currentWindow"/> 
        /// and brings the window to the front.
        /// It will also clean up some backend stuff.
        /// </summary>
        /// <param name="currentWindow"></param>
        /// <param name="originalParent"></param>
        public static void BringToFront(IntPtr currentWindow, IntPtr originalParent) 
        {
            SetParent(currentWindow, originalParent);
            SendMessage(Progman, 0x0034, 4, IntPtr.Zero);
            WorkerW = IntPtr.Zero;
            BringWindowToTop(currentWindow);
        }
        /// <summary>
        /// Will set the <paramref name="currentWindow"/> to the background
        /// </summary>
        /// <param name="currentWindow"></param>
        public static void SendToBackground(IntPtr currentWindow, int screen = 0) 
        {
            if (screen < 0 || screen >= ScreenCount)
                screen = 0;

            int offsetX = 0;
            int offsetY = 0;

            foreach (Screen s in Screen.AllScreens) 
            {
                Rectangle r = s.Bounds;
                offsetX = Math.Min(offsetX, r.X);
                offsetY = Math.Min(offsetY, r.Y);
            }

            offsetX = Math.Abs(offsetX);
            offsetY = Math.Abs(offsetY);

            Rectangle rect = Screen.AllScreens[screen].Bounds;
            SetParent(currentWindow, WorkerW);
            int x = rect.X + offsetX;
            int y = rect.Y + offsetY;
            SetWindowPos(currentWindow, IntPtr.Zero, x, y, rect.Width, rect.Height, (uint)(MonitorFlags.SWP_NOOWNERZORDER));
        }


        /// <summary>
        /// This method will send a specific code to 'Progman' that will generate a window between the desktop icons and the background.
        /// The new window will have the name of 'WorkerW'.
        /// This only works on Windows 8+ (Tested with Windows 10 and 11)
        /// To retrive the 'WorkerW' window, use <seealso cref="getWorkerW"/>.
        /// </summary>
        /// <param name="progman">The Window IntPtr of the 'Progman' window. That can be retrieved with <seealso cref="Progman"/></param>
        private static void createWorkerW(IntPtr progman)
        {
            IntPtr result = IntPtr.Zero; // Not used
                                         // Send 0x052C to Progman. This message directs Progman to spawn a 
                                         // WorkerW behind the desktop icons. If it is already there, nothing 
                                         // happens.
            SendMessageTimeout(progman,
                           0x052C,
                           new IntPtr(0),
                           IntPtr.Zero,
                           SendMessageTimeoutFlags.SMTO_NORMAL,
                           1000,
                           out result);
        }

        /// <summary>
        /// Helper function for the <see cref="EnumWindowsProc"/> delegate.
        /// Will check IntPtr, <paramref name="hWnd"/>, if it has the child of "SHELLDLL_DefView".
        /// If that child exists, the next "WorkerW" window will be stored as <see cref="_workerw"/>.
        /// </summary>
        /// <param name="hWnd">The Window IntPtr that is returned from <see cref="EnumWindowsProc"/></param>
        /// <param name="lParam">Unused, but is returned by <see cref="EnumWindowsProc"/></param>
        /// <returns>Always returns true.</returns>
        [AOT.MonoPInvokeCallback(typeof(EnumWindowsProc))]
        private static bool windowEnum(IntPtr hWnd, IntPtr lParam)
        {
            IntPtr p = FindWindowEx(hWnd,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            null);

            if (p != IntPtr.Zero)
            {
                // Gets the WorkerW Window after the current one.
                _workerw = FindWindowEx(IntPtr.Zero,
                                           hWnd,
                                           "WorkerW",
                                           null);
            }

            return true;
        }

        //DLLs Required
        #region DllCode
        /// <summary>
        /// Brings <paramref name="hWnd"/> to the top of the desktop.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        /// <summary>
        /// Sends message, <paramref name="Msg"/> to window <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The window to send a message to</param>
        /// <param name="Msg">What message to send</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        /// <summary>
        /// Finds a window's IntPtr using it's <paramref name="lpClassName"/> and <paramref name="lpWindowName"/>.
        /// If <paramref name="lpClassName"/> is null, that field will be ignored in the search.
        /// </summary>
        /// <param name="lpClassName">The Class Name.</param>
        /// <param name="lpWindowName">The Window Name/ The Caption of the Window.</param>
        /// <returns><see cref="IntPtr"/> of the window with said caption and class.</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Returns the parent of <paramref name="hWnd"/>
        /// </summary>
        /// <param name="hWnd">The child who's parent you want to get.</param>
        /// <returns>The parent <see cref="IntPtr"/> of <paramref name="hWnd"/></returns>
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        /// <summary>
        /// Returns the window <see cref="IntPtr"/> of the currently executing code only if it is the focused window.
        /// </summary>
        /// <returns>This window's <see cref="IntPtr"/></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr windowHandle,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr result);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Extra bits to make Dlls work
        #region Extra Bits
        private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [Flags]
        private enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }

        [Flags]
        private enum MonitorFlags : uint 
        {
            SWP_ASYNCWINDOWPOS = 0x4000,
            SWP_DEFERERASE = 0x2000,
            SWP_DRAWFRAME = 0x0020,
            SWP_FRAMECHANGED = 0x0020,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOACTIVATE = 0x0010,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOMOVE = 0x0002,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOREDRAW = 0x0008,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_SHOWWINDOW = 0x0040
        }

        #endregion
        #endregion
    }
}