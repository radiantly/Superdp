using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows.Forms.Design;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MSTSCLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using System.Dynamic;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Superdp
{
    public partial class HeroForm
    {
        public static readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private FormWindowState LastWindowState = FormWindowState.Minimized;
        //private Screen curScreen;
        private readonly FormManager manager;
        private readonly long creationTime;
        private readonly InteropQueen interopQueen;
        private string? dragSerializedTab = null;
        private bool flagTabDragActive = false;
        private bool flagWebViewReady = false;
        private readonly List<string> pendingWebMessages = new();

        private SmoothLoadingBar bar;
        public HeroForm(FormManager manager)
        {
            DoubleBuffered = true;
            BackColor = BackgroundColor;
            creationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this.manager = manager;
            InitializeComponent();
            //curScreen = Screen.FromControl(this);
            webView.Size = ClientSize;
            //MaximizedBounds = curScreen.WorkingArea;

            Icon = Properties.Resources.favicon;

            Resize += MainForm_Resize;

            //Move += (sender, e) => curScreen = Screen.FromControl(this);

            bar = new(Color.FromArgb(101, 101, 101), TimeSpan.FromMilliseconds(3000));
            bar.Size = new Size(ClientSize.Width, 4);

            Controls.Add(bar);
            interopQueen = new InteropQueen(this);

            Move += HeroForm_Move;
        }

        private void HeroForm_Move(object? sender, EventArgs e)
        {
            if (dragSerializedTab == null && flagTabDragActive) return;

            foreach (HeroForm form in manager.openForms)
            {
                if (form == this) continue;

                if (form.IsOverTabBar(form.PointToClient(Cursor.Position)))
                {
                    form.PostWebMessage(dragSerializedTab);
                    Close();
                    break;
                }
            }
        }

        public bool IsOverTabBar(Point clientPoint)
        {
            // TODO: 35
            Rectangle r = new(0, 0, ClientRectangle.Width, 35);
            return r.Contains(clientPoint);
        }

        public HeroForm(FormManager manager, bool dragged) : this(manager)
        {
            Location = new Point(Cursor.Position.X - 50, Cursor.Position.Y - 50);
        }

        private bool webViewInBackground = true;
        public void EnsureWebViewPositioning()
        {
            if (webViewInBackground) webView.SendToBack();
            else webView.BringToFront();
        }

        private void resizeControls()
        {
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;
                if (WindowState == FormWindowState.Maximized)
                {
                    if (FormBorderStyle == FormBorderStyle.None) return;
                    WindowState = FormWindowState.Normal;
                    FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Maximized;
                    Debug.WriteLine(Location);
                    Debug.WriteLine(Size);
                    Debug.WriteLine(MaximumSize);
                    Debug.WriteLine(ClientSize);
                    //rdp.Visible = false;
                    //Size = new Size(Size.Width, 35);
                }
                else if (WindowState == FormWindowState.Normal)
                {
                    if (FormBorderStyle != FormBorderStyle.None) return;
                    FormBorderStyle = FormBorderStyle.Sizable;
                }
            }
        }


        private void MainForm_Resize(object? sender, EventArgs e)
        {
            // Debug.WriteLine("Resizing");
            resizeControls();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            bar.Start(() => bar.Visible = false);
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
            webView.CoreWebView2.Settings.IsReputationCheckingRequired = false;
#if DEBUG
            webView.CoreWebView2.Navigate("http://localhost:5173");
#else
            // https://github.com/MicrosoftEdge/WebView2Feedback/issues/2381
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping("superdp.example", "interface\\dist", CoreWebView2HostResourceAccessKind.Allow);
            webView.CoreWebView2.Navigate("https://superdp.example/index.html");
#endif

            webView.CoreWebView2.AddHostObjectToScript("interopQueen", interopQueen);
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        public void PostWebMessage(string message)
        {
            if (!flagWebViewReady)
            {
                pendingWebMessages.Add(message);
                return;
            }

            Debug.WriteLine($"Sending message: {message}");
            webView.CoreWebView2.PostWebMessageAsJson(message);
        }

        public void PostWebMessage(Action<dynamic> callback)
        {
            dynamic msg = new ExpandoObject();

            callback(msg);

            string msgStr = JsonSerializer.Serialize(msg);
            PostWebMessage(msgStr);
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            string msgString = args.TryGetWebMessageAsString();
            var msg = JsonNode.Parse(msgString)?.AsObject();
            if (msg?["type"] == null) return;
            Debug.WriteLine("Message type: " + msg["type"]);
            switch ((string?)msg["type"])
            {
                case "RDP_CONNECT":
                    string? clientId = (string?)msg["client"]?["id"];
                    if (clientId == null) return;

                    //connectRDPControl(rdp_controls[clientId], (string?)msg["client"]?["host"], (string?)msg["client"]?["username"], (string?)msg["client"]?["password"]);
                    break;
                case "RDP_SHOW": break;
                case "RDP_HIDE": break;
            }
        }
        //private const int WM_NCHITTEST = 0x84;
        //private const int HTCLIENT = 0x1;
        //private const int HTCAPTION = 0x2;
        private const int WM_NCLBUTTONUP = 0xA2;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;
        protected override void WndProc(ref Message m)
        {
            // Debug.WriteLine(m.Msg);
            switch (m.Msg)
            {
                case FormManager.newInstanceMesssage:
                    manager.CreateInstance();
                    break;
                case WM_ENTERSIZEMOVE:
                    // There is a case where the user clicks the tab, so dragSerializedTab is non-null
                    // but the window has actually not started moving. So on next move dragSerializedTab
                    // is mistakenly used. flagTabDragActive fixes this.
                    if (dragSerializedTab != null) flagTabDragActive = true;
                    break;
                case WM_EXITSIZEMOVE:
                    flagTabDragActive = false;
                    dragSerializedTab = null;
                    break;
            }
            base.WndProc(ref m);
        }
    }
}