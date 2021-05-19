using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Text;

namespace DesktopAquarium
{
    public class WindowController : MonoBehaviour
    {
        //DLLs Required
        #region DllCode
        /// <summary>
        /// Brings <paramref name="hWnd"/> to the top of the desktop.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);

        /// <summary>
        /// Sends message, <paramref name="Msg"/> to window <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The window to send a message to</param>
        /// <param name="Msg">What message to send</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

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
        public static extern IntPtr GetParent(IntPtr hWnd);

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
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

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

        #endregion
        #endregion

        private const string UnityWindowClassName = "UnityWndClass";
        private const string UnityEditorWindowClassName = "UnityContainerWndClass";

        /// <summary>
        /// The original window ptr for <see cref="_gameWindow"/> on launch.
        /// Recorded with <see cref="getPointers"/> once and used to return the window to front.
        /// </summary>
        private static IntPtr _foregroundParent;
        /// <summary>
        /// The window for the currently executing code. If not avaliable, it will be set as IntPtr.Zero.
        /// Is set by <seealso cref="getPointers"/>
        /// </summary>
        private static IntPtr _gameWindow;
        /// <summary>
        /// The window that sits between the background and icons on Windows.
        /// Can be created by calling <seealso cref="createWorkerW(IntPtr)"/> and can be used after grabbing it with <seealso cref="getWorkerW"/>.
        /// </summary>
        private static IntPtr _workerw;

        /// <summary>
        /// When calling <see cref="PushGameToBack"/>, if true the window will go fullscreen.
        /// on false, it'll go windowed.
        /// </summary>
        [SerializeField]
        public bool BackgroundFullscreen;
        /// <summary>
        /// When calling <see cref="BringGameToFront"/>, if true the window will go fullscreen.
        /// on false, it'll go windowed.
        /// </summary>
        [SerializeField]
        public bool ForegroundFullscreen;

        private Resolution lastWindowResolution;

        private void OnEnable()
        {
            _foregroundParent = IntPtr.Zero;
            _gameWindow = IntPtr.Zero;
            _workerw = IntPtr.Zero;
        }

        private void Start()
        {
            _gameWindow = GetActiveWindow();
            Invoke("PushGameToBack", 5f);
            Invoke("BringGameToFront", 120f);
        }

        private void Update()
        {
            if (_gameWindow == IntPtr.Zero)
            {
                getPointers();
            }
        }

        #region Windows Helpers
        /// <summary>
        /// This will ask windows for the IntPtr that refers to the 'Progman' window.
        /// </summary>
        /// <returns>Window IntPtr of the 'Progman' window</returns>
        private static IntPtr grabProgman()
        {
            // Fetch the Progman window
            return FindWindow("Progman", null);
        }

        /// <summary>
        /// This method will send a specific code to 'Progman' that will generate a window between the desktop icons and the background.
        /// The new window will have the name of 'WorkerW'.
        /// This only works on Windows 8+ (Tested with Windows 10)
        /// To retrive the 'WorkerW' window, use <seealso cref="getWorkerW"/>.
        /// </summary>
        /// <param name="progman">The Window IntPtr of the 'Progman' window. That can be retrieved with <seealso cref="grabProgman"/></param>
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
        /// Used in conjunction with <see cref="getWorkerW"/>.
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

        /// <summary>
        /// Uses <see cref="EnumWindows(EnumWindowsProc, IntPtr)"/> to find the WorkerW window.
        /// This only works properly after running <seealso cref="createWorkerW(IntPtr)"/>.
        /// </summary>
        /// <returns>A window IntPtr that sits between the desktop icons and background.</returns>
        private static IntPtr getWorkerW()
        {
            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            EnumWindows(windowEnum, IntPtr.Zero);

            return _workerw;
        }

        /// <summary>
        /// Helper class that assigns <see cref="_gameWindow"/> and subsequently assigns <see cref="_foregroundParent"/> if <see cref="_gameWindow"/> is not IntPtr.Zero.
        /// </summary>
        private void getPointers()
        {
            _gameWindow = GetActiveWindow();
            if (_gameWindow != IntPtr.Zero)
                _foregroundParent = GetParent(_gameWindow);
        }
        #endregion

        #region Resolution Helpers

        /// <summary>
        /// Sets the window's resolution to the monitor resolution and changes <see cref="Screen.fullScreen"/> to true.
        /// Also stores <see cref="lastWindowResolution"/> to be used in <seealso cref="toWindowed"/>.
        /// </summary>
        private void toFullscreen()
        {
            if (!Screen.fullScreen)
            {
                // Store window resolution;
                lastWindowResolution = new Resolution { height = Screen.height, refreshRate = Screen.currentResolution.refreshRate, width = Screen.width };
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
        }

        /// <summary>
        /// Sets the window's resolution to <see cref="lastWindowResolution"/> and sets <see cref="Screen.fullScreen"/> to false.
        /// <see cref="lastWindowResolution"/> is set after <seealso cref="toFullscreen"/> is run successfully;
        /// </summary>
        private void toWindowed()
        {
            if (Screen.fullScreen)
            {
                Screen.SetResolution(lastWindowResolution.width, lastWindowResolution.height, false);
            }
        }

        /// <summary>
        /// Will call <seealso cref="toFullscreen"/> when <paramref name="value"/> is true and
        /// will call <seealso cref="toWindowed"/> when <paramref name="value"/> is false.
        /// </summary>
        /// <param name="value">True for Fullscreen, False for Windowed</param>
        private void setFullscreen(bool value)
        {
            if (value)
                toFullscreen();
            else
                toWindowed();
        }
        #endregion

        #region Wrapers
        /// <summary>
        /// This will set the game windows ontop of the wall paper.
        /// Please use <seealso cref="BringGameToFront"/> to undo this function.
        /// Note: Direct interaction will the application will be blocked by windows.
        /// Wrapper for <seealso cref="pushGameToBack(IntPtr)"/>.
        /// </summary>
        public void PushGameToBack()
        {
            setFullscreen(BackgroundFullscreen);
            pushGameToBack(_gameWindow);
        }

        /// <summary>
        /// This will set a screen that's been pushed to the background with <seealso cref="PushGameToBack"/> to the desktop.
        /// Note: This is required to restore interaction with the application.
        /// Wrapper for <seealso cref="bringGameToFront(IntPtr, IntPtr)"/>
        /// </summary>
        public void BringGameToFront()
        {
            bringGameToFront(_gameWindow, _foregroundParent);
            setFullscreen(ForegroundFullscreen);
        }
        #endregion

        #region worker functions
        /// <summary>
        /// Sets window <paramref name="gameWindow"/>'s parent as window <paramref name="foregroundParent"/>.
        /// Resets the original background image.
        /// Sets <paramref name="gameWindow"/> as the topmost window.
        /// The wrapper for this class is <seealso cref="BringGameToFront"/>
        /// </summary>
        /// <param name="gameWindow">The child window that is being moved</param>
        /// <param name="foregroundParent">The parent that's typically the normal Desktop.</param>
        private static void bringGameToFront(IntPtr gameWindow, IntPtr foregroundParent)
        {
            // Pushes Game window to the front
            SetParent(gameWindow, foregroundParent);
            SendMessage(grabProgman(), 0x0034, 4, IntPtr.Zero);
            BringWindowToTop(gameWindow);
        }

        /// <summary>
        /// Creates a window behind the desktop icons using <see cref="createWorkerW(IntPtr)"/>.
        /// Grabs said window with <see cref="getWorkerW"/> and sets <paramref name="gameWindow"/> as it's child.
        /// Thus moving the <paramref name="gameWindow"/> behind the icons and in front of the desktop.
        /// The wrapper for this class is <seealso cref="BringGameToFront"/>
        /// </summary>
        /// <param name="gameWindow"></param>
        private static void pushGameToBack(IntPtr gameWindow)
        {

            // Gets progman and tells it to make WorkerW
            createWorkerW(grabProgman());
            // Grab the newly created WorkerW window (Well the one after it)
            IntPtr workerw = getWorkerW();

            // Sets Game Window as a child of WorkerW
            SetParent(gameWindow, workerw);
        }
        #endregion
    }
}