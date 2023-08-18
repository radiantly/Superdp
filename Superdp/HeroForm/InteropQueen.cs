using System.Diagnostics;
using System.Runtime.InteropServices;

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

            public long GetFormCreationTimestamp()
            {
                form.InterfaceReady = true;
                return form.creationTime;
            }

            #region TitleBarActions
            // https://github.com/MicrosoftEdge/WebView2Feedback/issues/200
            private const int WM_NCLBUTTONDOWN = 0xA1;
            private const int WM_NCRBUTTONDOWN = 0xA4;
            private const int HT_CAPTION = 0x2;

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]
            private static extern bool ReleaseCapture();

            public void MouseDownWindowDrag()
            {
                ReleaseCapture();
                PostMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

            public void MouseDownWindowDragWithTab(string serializedTab)
            {
                ReleaseCapture();
                PostMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                form.dragSerializedTab = serializedTab;
            }
            #endregion

            public void CreateNewDraggedWindow(string serializedTab)
            {
                var newForm = form.manager.CreateInstance();
                newForm.Location = new Point(Cursor.Position.X - 50, Cursor.Position.Y - 50);
                newForm.PostWebMessage(serializedTab);
                newForm.interopQueen.MouseDownWindowDragWithTab(serializedTab);
            }

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

            public void RDPSetCharacteristics(string clientId, int x, int y, int width, int height, bool visibility)
            {
                RDPFormManager.SetCharacteristics(form, clientId, new Point(x, y), new Size(width, height), visibility);
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