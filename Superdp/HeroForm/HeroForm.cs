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
        private Screen curScreen;
        private readonly FormManager manager;
        private readonly long creationTime;

        private SmoothLoadingBar bar;
        public HeroForm(FormManager manager)
        {
            DoubleBuffered = true;
            BackColor = BackgroundColor;
            creationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this.manager = manager;
            InitializeComponent();
            curScreen = Screen.FromControl(this);
            webView.Size = ClientSize;
            MaximizedBounds = curScreen.WorkingArea;

            Icon = Properties.Resources.favicon;

            Resize += MainForm_Resize;
            ResizeEnd += MainForm_ResizeEnd;

            Move += (sender, e) => curScreen = Screen.FromControl(this);

            bar = new(Color.FromArgb(101, 101, 101), TimeSpan.FromMilliseconds(3000));
            bar.Size = new Size(ClientSize.Width, 4);

            Controls.Add(bar);
        }

        private bool webViewInBackground = true;
        public void EnsureWebViewPositioning()
        {
            if (webViewInBackground) webView.SendToBack();
            else webView.BringToFront();
        }



        private void MainForm_ResizeEnd(object? sender, EventArgs e)
        {

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

            var myQueen = new InteropQueen(this);
            webView.CoreWebView2.AddHostObjectToScript("interopQueen", myQueen);
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        public void PostWebMessage(string message)
        {
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
        protected override void WndProc(ref Message m)
        {
            // Debug.WriteLine(m.Msg);
            switch (m.Msg)
            {
                case FormManager.newInstanceMesssage:
                    manager.CreateInstance();
                    //case WM_NCHITTEST:
                    //    Point pos = new(m.LParam.ToInt32());
                    //    if (pos.Y < 35)
                    //    {
                    //        m.Result = HTCAPTION;
                    //        return;
                    //    }
                    //    Debug.WriteLine(pos);
                    break;
            }
            base.WndProc(ref m);
        }
    }
}