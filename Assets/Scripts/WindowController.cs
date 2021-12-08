using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Background.Windows;

namespace DesktopAquarium
{
    public class WindowController : MonoBehaviour
    {
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

        public bool IsBackground { get; private set; }

        private Resolution lastWindowResolution;

        private void OnEnable()
        {
            _foregroundParent = IntPtr.Zero;
            _gameWindow = IntPtr.Zero;
        }

        private void Start()
        {
            _gameWindow = Wallpaper.CurrentWindow;

            Invoke("PushGameToBack", 10f);
            Invoke("BringGameToFront", 20f);
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
        /// Helper class that assigns <see cref="_gameWindow"/> and subsequently assigns <see cref="_foregroundParent"/> if <see cref="_gameWindow"/> is not IntPtr.Zero.
        /// </summary>
        private void getPointers()
        {
            _gameWindow = Wallpaper.CurrentWindow;
            if (_gameWindow != IntPtr.Zero)
                _foregroundParent = Wallpaper.ParentWindow;
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
                if (lastWindowResolution.width < 50 || lastWindowResolution.height < 50) 
                {
                    lastWindowResolution.width = Screen.currentResolution.width / 3;
                    lastWindowResolution.height = Screen.currentResolution.height / 3;
                }
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
            return;
            //if (value)
            //    toFullscreen();
            //else
            //    toWindowed();
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
            IsBackground = true;
            setFullscreen(BackgroundFullscreen);
            Wallpaper.SendToBackground(_gameWindow, 1);
        }

        /// <summary>
        /// This will set a screen that's been pushed to the background with <seealso cref="PushGameToBack"/> to the desktop.
        /// Note: This is required to restore interaction with the application.
        /// Wrapper for <seealso cref="bringGameToFront(IntPtr, IntPtr)"/>
        /// </summary>
        public void BringGameToFront()
        {
            IsBackground = false; ;
            Wallpaper.BringToFront(_gameWindow, _foregroundParent);
            setFullscreen(ForegroundFullscreen);
        }
        #endregion
    }
}