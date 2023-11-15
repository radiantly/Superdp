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

            public InteropQueen(HeroForm form)
            {
                this.form = form;
            }
            public object Init()
            {
                form.InterfaceReady?.Invoke();

                // If the webview is refreshed, hide all rdp forms
                var rdpManager = (RDPManager)form.connectionManagers["rdp"];
                foreach (var rdpForm in rdpManager.Forms)
                    rdpForm.ShouldBeVisible = false;

                foreach (var f in form.Manager.openForms)
                    Debug.Write(f.Equals(form));
                Debug.WriteLine("");

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
                MouseDownWindowDrag();
                //form.dragSerializedTab = serializedTab;
            }
            #endregion

            public void CreateNewDraggedWindow(string tabId)
            {
                var newForm = form.Manager.CreateInstance();
                newForm.Location = new Point(Cursor.Position.X - 50, Cursor.Position.Y - 50);
                newForm.PostWebMessage(new
                {
                    transferTabId = tabId,
                    type = "TAB_TRANSFER_REQUEST"
                });
                newForm.interopQueen.MouseDownWindowDragWithTab(tabId);
            }

            public string ReadConf() => form.Manager.Conf;
            public void WriteConf(string contents) => form.Manager.Conf = contents;

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