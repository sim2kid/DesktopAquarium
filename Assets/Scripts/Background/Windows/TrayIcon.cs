using System;
using System.Windows.Forms;

namespace Background.Windows
{
    public class TrayIcon
    {
        private NotifyIcon notifyIcon;
        private System.ComponentModel.IContainer components;

        public TrayIcon() 
        {
            components = new System.ComponentModel.Container();

            notifyIcon = new NotifyIcon(components);
        }
    }
}