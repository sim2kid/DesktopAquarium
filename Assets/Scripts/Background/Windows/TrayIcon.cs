using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Events;

namespace Background.Windows
{
    public class TrayIcon : MonoBehaviour
    {
        public string ICOPath;
        public UnityEvent onClick;

        private NotifyIcon notifyIcon;


        private void OnEnable()
        {
            notifyIcon = new NotifyIcon();

            if (!string.IsNullOrWhiteSpace(ICOPath))
            {
                string p = ToBackslash(System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, ICOPath));
                notifyIcon.Icon = new System.Drawing.Icon(p);
            }

            notifyIcon.Text = $"{UnityEngine.Application.productName}";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += new System.EventHandler(this.DoubleClick);
        }

        private void OnDisable()
        {
            notifyIcon.Dispose();
            notifyIcon = null;
        }

        private void DoubleClick(object Sender, System.EventArgs e) 
        {
            Debug.Log("Click!");
            onClick.Invoke();
        }

        private string ToBackslash(string s) 
        {
            return s.Replace('/', '\\');
        }
    }
}