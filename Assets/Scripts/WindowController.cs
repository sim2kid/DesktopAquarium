using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Text;

public class WindowController : MonoBehaviour
{
    //DLLs Required
    #region DllCode
    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);

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

    private enum GetWindowType : uint
    {
        /// <summary>
        /// The retrieved handle identifies the window of the same type that is highest in the Z order.
        /// <para/>
        /// If the specified window is a topmost window, the handle identifies a topmost window.
        /// If the specified window is a top-level window, the handle identifies a top-level window.
        /// If the specified window is a child window, the handle identifies a sibling window.
        /// </summary>
        GW_HWNDFIRST = 0,
        /// <summary>
        /// The retrieved handle identifies the window of the same type that is lowest in the Z order.
        /// <para />
        /// If the specified window is a topmost window, the handle identifies a topmost window.
        /// If the specified window is a top-level window, the handle identifies a top-level window.
        /// If the specified window is a child window, the handle identifies a sibling window.
        /// </summary>
        GW_HWNDLAST = 1,
        /// <summary>
        /// The retrieved handle identifies the window below the specified window in the Z order.
        /// <para />
        /// If the specified window is a topmost window, the handle identifies a topmost window.
        /// If the specified window is a top-level window, the handle identifies a top-level window.
        /// If the specified window is a child window, the handle identifies a sibling window.
        /// </summary>
        GW_HWNDNEXT = 2,
        /// <summary>
        /// The retrieved handle identifies the window above the specified window in the Z order.
        /// <para />
        /// If the specified window is a topmost window, the handle identifies a topmost window.
        /// If the specified window is a top-level window, the handle identifies a top-level window.
        /// If the specified window is a child window, the handle identifies a sibling window.
        /// </summary>
        GW_HWNDPREV = 3,
        /// <summary>
        /// The retrieved handle identifies the specified window's owner window, if any.
        /// </summary>
        GW_OWNER = 4,
        /// <summary>
        /// The retrieved handle identifies the child window at the top of the Z order,
        /// if the specified window is a parent window; otherwise, the retrieved handle is NULL.
        /// The function examines only child windows of the specified window. It does not examine descendant windows.
        /// </summary>
        GW_CHILD = 5,
        /// <summary>
        /// The retrieved handle identifies the enabled popup window owned by the specified window (the
        /// search uses the first such window found using GW_HWNDNEXT); otherwise, if there are no enabled
        /// popup windows, the retrieved handle is that of the specified window.
        /// </summary>
        GW_ENABLEDPOPUP = 6
    }

    #endregion
    #endregion

    private const string UnityWindowClassName = "UnityWndClass";
    private const string UnityEditorWindowClassName = "UnityContainerWndClass";

    private static IntPtr _foregroundParent;
    private static IntPtr _gameWindow;
    private static IntPtr _workerw;

    [SerializeField]
    public bool BackgroundFullscreen;
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
    private static IntPtr grabProgman() 
    {
        // Fetch the Progman window
        return FindWindow("Progman", null);
    }

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

    private static IntPtr getWorkerW() 
    {
        // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
        // as a child. 
        // If we found that window, we take its next sibling and assign it to workerw.
        EnumWindows(windowEnum, IntPtr.Zero);

        return _workerw;
    }

    private void getPointers() 
    {
        _gameWindow = GetActiveWindow();
        if(_gameWindow != IntPtr.Zero)
            _foregroundParent = GetParent(_gameWindow);
    }
    #endregion

    #region Resolution Helpers
    private void toFullscreen() 
    {
        if (!Screen.fullScreen) 
        {
            // Store window resolution;
            lastWindowResolution = new Resolution { height = Screen.height, refreshRate = Screen.currentResolution.refreshRate, width = Screen.width};
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
    }

    private void toWindowed() 
    {
        if (Screen.fullScreen)
        {
            Screen.SetResolution(lastWindowResolution.width, lastWindowResolution.height, false);
        }
    }

    private void setFullscreen(bool value) 
    {
        if (value)
            toFullscreen();
        else
            toWindowed();
    }
    #endregion

    #region Wrapers
    public void PushGameToBack() 
    {
        setFullscreen(BackgroundFullscreen);
        pushGameToBack(_gameWindow);
    }

    public void BringGameToFront() 
    {
        bringGameToFront(_gameWindow, _foregroundParent);
        setFullscreen(ForegroundFullscreen);
    }
    #endregion

    #region worker functions
    private static void bringGameToFront(IntPtr gameWindow, IntPtr foregroundParent) 
    {
        // Pushes Game window to the front
        SetParent(gameWindow, foregroundParent);
        SendMessage(grabProgman(), 0x0034, 4, IntPtr.Zero);
        BringWindowToTop(gameWindow);
    }

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
