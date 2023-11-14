using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Superdp
{
    using static Native;
    public partial class HeroForm : Form
    {
        [ClassInterface(ClassInterfaceType.AutoDual)]
        [ComVisible(true)]
        public class InteropQueen
        {
            private readonly HeroForm form;
            public readonly long t;

            public InteropQueen(HeroForm form)
            {
                this.form = form;
                t = form.creationTime;
            }

            public long GetFormCreationTimestamp()
            {
                form.InterfaceReady = true;
                return form.creationTime;
            }

            #region TitleBarActions
            // https://github.com/MicrosoftEdge/WebView2Feedback/issues/200

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

            public string ReadConf() => form.manager.Conf;
            public void WriteConf(string contents) => form.manager.Conf = contents;

            public void Connect(string jsonString)
            {
                Debug.WriteLine("Connection Request");
                dynamic options = new DynJson(jsonString);
                form.connectionManagers[options.type].Connect(options);
            }

            public void Disconnect(string jsonString)
            {
                dynamic options = new DynJson(jsonString);
                form.connectionManagers[options.type].Disconnect(options);
            }

            public void Update(string jsonString)
            {
                dynamic options = new DynJson(jsonString);
                form.connectionManagers[options.type].Update(options);
            }

            public void SSHInput(string tabId, string text) =>
                ((SSHManager)form.connectionManagers["ssh"]).Input(tabId, text);

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