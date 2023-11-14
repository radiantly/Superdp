using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;

namespace Superdp
{
    using static Native;
    public partial class HeroForm
    {
        public static readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private FormWindowState LastWindowState = FormWindowState.Minimized;
        private readonly FormManager manager;
        private readonly long creationTime;
        private readonly InteropQueen interopQueen;
        private string? dragSerializedTab = null;
        private bool flagTabDragActive = false;
        private readonly List<string> pendingWebMessages = new();

        private readonly SmoothLoadingBar bar;
        private readonly string id;
        private readonly Dictionary<string, IConnectionManager> connectionManagers;
        public HeroForm(FormManager manager)
        {
            id = Guid.NewGuid().ToString();
            DoubleBuffered = true;
            BackColor = BackgroundColor;
            creationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this.manager = manager;
            InitializeComponent();
            webView.Size = ClientSize;
            MaximizedBounds = Screen.FromControl(this).WorkingArea;

            Icon = Properties.Resources.favicon;
            connectionManagers = new() { ["rdp"] = new RDPManager(this), ["ssh"] = new SSHManager(this) };

            Resize += MainForm_Resize;

            bar = new(Color.FromArgb(101, 101, 101), TimeSpan.FromMilliseconds(3000));
            bar.Size = new Size(ClientSize.Width, 4);

            Controls.Add(bar);
            interopQueen = new InteropQueen(this);

            Move += HeroForm_Move;
        }

        private void HeroForm_Move(object? sender, EventArgs e)
        {
            if (dragSerializedTab == null || !flagTabDragActive || manager.openForms.Count <= 1) return;

            foreach (HeroForm form in manager.openForms)
            {
                if (form == this) continue;

                if (form.IsOverTabBar(form.PointToClient(Cursor.Position)))
                {
                    var originalStyle = GetWindowLongPtr(Handle, GWL_EXSTYLE);
                    SetWindowLongPtr(Handle, GWL_EXSTYLE, originalStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);

                    // We need to check if the form we're moving the tab is
                    // being obscured by a different window, in which case
                    // we don't move the tab
                    if (GetAncestor(WindowFromPoint(Cursor.Position), 2) != form.Handle)
                    {
                        SetWindowLongPtr(Handle, GWL_EXSTYLE, originalStyle);
                        continue;
                    }

                    // Move tab and close current form
                    form.PostWebMessage(dragSerializedTab);
                    dragSerializedTab = null;
                    // RDPManager.TransferOwnership(this, form);
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

        private bool webViewInBackground = true;
        public void EnsureWebViewPositioning()
        {
            if (webViewInBackground) webView.SendToBack();
            else webView.BringToFront();
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;
                if (WindowState == FormWindowState.Maximized && FormBorderStyle != FormBorderStyle.None)
                {
                    // We can't directly set FormBorderStyle to None because it messes up the ClientSize
                    WindowState = FormWindowState.Normal;
                    FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Maximized;
                }
                else if (WindowState == FormWindowState.Normal && FormBorderStyle != FormBorderStyle.Sizable)
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                }
            }
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
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping("superdp.example", "interface\\dist", Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
            webView.CoreWebView2.Navigate("https://superdp.example/index.html");
#endif

            webView.CoreWebView2.AddHostObjectToScript("interopQueen", interopQueen);
            webView.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
        }

        private void CoreWebView2_PermissionRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2PermissionRequestedEventArgs e)
        {
            var def = e.GetDeferral();
            e.State = CoreWebView2PermissionState.Allow;
            e.Handled = true;
            def.Complete();
        }

        private bool _interfaceReady = false;
        public bool InterfaceReady
        {
            get => _interfaceReady;
            private set
            {
                _interfaceReady = value;
                PostWebMessage();
            }
        }

        public void PostWebMessage(params string[] messages)
        {
            pendingWebMessages.AddRange(messages);
            if (!InterfaceReady) return;

            foreach (string message in pendingWebMessages)
            {
                Debug.WriteLine($"Sending message: {message}");
                webView.CoreWebView2.PostWebMessageAsJson(message);
            }

            pendingWebMessages.Clear();
        }

        public void PostWebMessage(Action<dynamic> callback)
        {
            dynamic msg = new ExpandoObject();

            callback(msg);

            string msgStr = JsonSerializer.Serialize(msg);
            PostWebMessage(msgStr);
        }

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
                case WM_NCHITTEST:
                    Debug.WriteLine("Hit test called");
                    break;
            }
            base.WndProc(ref m);
        }
    }
}