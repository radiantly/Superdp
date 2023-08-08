using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AxMSTSCLib;

namespace Superdp
{
    public partial class HeroForm : Form
    {
        [ClassInterface(ClassInterfaceType.AutoDual)]
        [ComVisible(true)]
        public class InteropQueen
        {
            private readonly HeroForm form;

            public InteropQueen(HeroForm form)
            {
                this.form = form;
            }

            public long GetFormCreationTimestamp() => form.creationTime;

            #region TitleBarActions
            // https://github.com/MicrosoftEdge/WebView2Feedback/issues/200
            private const int WM_NCLBUTTONDOWN = 0xA1;
            private const int WM_NCRBUTTONDOWN = 0xA4;
            private const int HT_CAPTION = 0x2;

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]
            private static extern bool ReleaseCapture();

            public void MouseDownWindowDrag()
            {
                ReleaseCapture();
                PostMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            #endregion

            public void BroadcastMessageToOtherInstances(string message)
            {
                form.manager.BroadcastWebMessage(message, form);
            }

            public string ReadConf() => form.manager.Conf;
            public void WriteConf(string contents) => form.manager.Conf = contents;

            public void RDPConnect(string clientId, string host, string username, string password)
            {
                Debug.WriteLine($"Connect Request for {host}");
                RDPFormManager.Connect(form, clientId, host, username, password);
            }

            public void RDPDisconnect(string clientId)
            {
                RDPFormManager.Disconnect(form, clientId);
            }

            public void RDPSetVisibility(string clientId, bool visibility)
            {
                Debug.WriteLine("Wanna set to " + visibility);
                RDPFormManager.SetVisibility(form, clientId, visibility);
            }

            public void RDPSetSize(string clientId, int width, int height)
            {
                Debug.WriteLine($"Setting size to {width} {height}");
                RDPFormManager.SetSize(form, clientId, new Size(width, height));
            }

            public void BringWebViewToFront()
            {
                form.webViewInBackground = false;
                form.EnsureWebViewPositioning();
            }

            public void SendWebViewToBack()
            {
                form.webViewInBackground = true;
                form.EnsureWebViewPositioning();
            }
        }
    }
}