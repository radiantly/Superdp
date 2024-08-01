using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

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
            public object Init(string id)
            {
                form.Id = id;
                form.InterfaceReady?.Invoke();

                // If the webview is refreshed, hide all rdp forms
                var rdpManager = (RDPManager)form.connectionManagers["rdp"];
                foreach (var rdpForm in rdpManager.Forms)
                    rdpForm.ShouldBeVisible = false;

                return JsonSerializer.Serialize(new
                {
                    creationTimestamp = form.CreationTime,
                    formBorderStyle = form.FormBorderStyle
                });
            }

            public void MouseDownWindowDrag()
            {
                ReleaseCapture();
                PostMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

            public void MouseDownWindowDragWithTab(string tabId)
            {
                MouseDownWindowDrag();
                form.draggedTabId = tabId;
            }

            public void CreateNewDraggedWindow(string tabId)
            {
                var newForm = form.Manager.CreateInstance();
                newForm.Location = new Point(Cursor.Position.X - 75, Cursor.Position.Y - 50);
                newForm.PostWebMessage(new
                {
                    originatingFormId = form.Id,
                    transferTabId = tabId,
                    type = "TAB_TRANSFER_REQUEST"
                });
                newForm.interopQueen.MouseDownWindowDragWithTab(tabId);
            }

            public string ReadConfig() => form.Manager.ConfigJson;
            public void SaveConfigChanges(string jsonChanges) => form.Manager.SaveConfigChanges(jsonChanges);

            public void Connect(string jsonString)
            {
                Debug.WriteLine("Connection Request");
                dynamic? options = DynJson.Parse(jsonString);
                form.connectionManagers[options?.type].Connect(options);
            }

            public void Disconnect(string jsonString)
            {
                dynamic? options = DynJson.Parse(jsonString);
                form.connectionManagers[options?.type].Disconnect(options);
            }

            public void Update(string jsonString)
            {
                dynamic? options = DynJson.Parse(jsonString);
                form.connectionManagers[options?.type].Update(options);
            }

            public void Transfer(string jsonString)
            {
                dynamic? options = DynJson.Parse(jsonString);
                form.connectionManagers[options?.type].Transfer(options);
            }

            public void SSHInput(string tabId, string text) =>
                ((SSHManager)form.connectionManagers["ssh"]).Input(tabId, text);

            public void UpdateNavAreas(string jsonString)
            {
                dynamic[]? conv = DynJson.Parse(jsonString);
                form.NavAreas = conv?.ToList()?.ConvertAll(obj =>
                    new Rectangle(obj.x, obj.y, obj.width, obj.height)
                ) ?? [];
            }

            public void Minimize()
            {
                form.WindowState = FormWindowState.Minimized;
            }

            public void Maximize()
            {
                form.WindowState = FormWindowState.Maximized;
            }

            public void Restore()
            {
                form.WindowState = FormWindowState.Normal;
            }

            public void Close()
            {
                form.Close();
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

            async public Task<int> CheckOpenPorts(string host)
            {
                var cancellationToken = new CancellationTokenSource(1000).Token;

                return await await Task.WhenAny(Utils.IsPortOpen(host, 22, cancellationToken), Utils.IsPortOpen(host, 3389, cancellationToken));
            }
        }
    }
}