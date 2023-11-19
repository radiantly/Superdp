using System.Diagnostics;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;

namespace Superdp
{
    using static Native;
    public partial class HeroForm
    {
        public string? Id { get; private set; }
        public static readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private FormWindowState LastWindowState = FormWindowState.Minimized;
        private readonly long creationTime;
        private readonly InteropQueen interopQueen;
        private string? draggedTabId = null;
        private bool flagTabDragActive = false;

        private readonly SmoothLoadingBar bar;
        private readonly Dictionary<string, IConnectionManager> connectionManagers;

        public required FormManager Manager { get; init; }

        private bool _ready = false;
        public event Action? InterfaceReady;

        private readonly List<string> pendingWebMessages = [];
        private List<Rectangle> NavAreas = [];
        public bool CloseOnTransfer { get; private set; } = false;
        public HeroForm()
        {
            DoubleBuffered = true;
            BackColor = BackgroundColor;
            creationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            InitializeComponent();
            webView.Visible = false;
            webView.Size = ClientSize;
            MaximizedBounds = Screen.FromControl(this).WorkingArea;

            Icon = Properties.Resources.favicon;
            connectionManagers = new() { ["rdp"] = new RDPManager(this), ["ssh"] = new SSHManager(this) };

            Resize += MainForm_Resize;

            bar = new(Color.FromArgb(34, 34, 34), TimeSpan.FromMilliseconds(2000));
            bar.Size = new Size(ClientSize.Width, 35);

            Controls.Add(bar);
            interopQueen = new InteropQueen(this);

            Move += HeroForm_Move;
            InterfaceReady += HeroForm_InterfaceReady;
        }

        private void HeroForm_Move(object? sender, EventArgs e)
        {
            // this is the only open form
            if (Manager.Forms.Count <= 1)
                return;

            // no tab is being dragged
            if (draggedTabId == null || !flagTabDragActive)
                return;

            // this form isn't ready
            if (Id == null)
                return;

            var originalStyle = GetWindowLongPtr(Handle, GWL_EXSTYLE);
            SetWindowLongPtr(Handle, GWL_EXSTYLE, originalStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);

            foreach (HeroForm form in Manager.Forms)
            {
                // skip me
                if (form == this)
                    continue;

                // check if over nav bar
                if (!form.IsOverNavBar(form.PointToClient(Cursor.Position)))
                    continue;
                    
                // We need to check if the form we're moving the tab is
                // being obscured by a different window, in which case
                // we don't move the tab
                if (GetAncestor(WindowFromPoint(Cursor.Position), 2) != form.Handle)
                    continue;

                Debug.WriteLine($"form {form.Id} matches.");
                
                CloseOnTransfer = true;
                form.PostWebMessage(new
                {
                    originatingFormId = Id,
                    transferTabId = draggedTabId,
                    type = "TAB_TRANSFER_REQUEST",
                });
                draggedTabId = null;

                break;
            }

            SetWindowLongPtr(Handle, GWL_EXSTYLE, originalStyle);
        }

        public bool IsOverNavBar(Point clientPoint) =>
            NavAreas.Any(r => r.Contains(clientPoint));

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
            bar.Start();
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

        private void HeroForm_InterfaceReady()
        {
            _ready = true;
            foreach (var message in pendingWebMessages)
                PostWebMessage(message);
            pendingWebMessages.Clear();
            webView.Visible = true;
            bar.Visible = false;
        }

        public void PostWebMessage(object msgObj)
        {
            // Messages can only be sent from the UI thread
            if (InvokeRequired)
            {
                Invoke(() => PostWebMessage(msgObj));
                return;
            }

            string message = msgObj is string ? (string)msgObj : JsonSerializer.Serialize(msgObj);

            if (!_ready)
            {
                pendingWebMessages.Add(message);
                return;
            }

            Debug.WriteLine($"Sending message: {message}");
            webView.CoreWebView2.PostWebMessageAsJson(message);
        }

        protected override void WndProc(ref Message m)
        {
            // Debug.WriteLine(m.Msg);
            switch (m.Msg)
            {
                case FormManager.NewInstanceMesssage:
                    Manager.CreateInstance();
                    break;
                case WM_ENTERSIZEMOVE:
                    // There is a case where the user clicks the tab, so draggedTabId is non-null
                    // but the window has actually not started moving. So on next move draggedTabId
                    // is mistakenly used. flagTabDragActive fixes this.
                    if (draggedTabId != null) flagTabDragActive = true;
                    break;
                case WM_EXITSIZEMOVE:
                    flagTabDragActive = false;
                    draggedTabId = null;
                    break;
            }
            base.WndProc(ref m);
        }
    }
}