using System.Diagnostics;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace Superdp
{
    using static Native;
    public partial class HeroForm
    {
        public string? Id { get; private set; }
        public static readonly Color BackgroundColor = Color.FromArgb(46, 52, 64);
        private readonly long CreationTime;
        private readonly InteropQueen interopQueen;
        private string? draggedTabId = null;
        private bool flagTabDragActive = false;

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
            CreationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            InitializeComponent();
            webView.Visible = false;
            webView.Size = ClientSize;
            MaximumSize = Screen.FromControl(this).WorkingArea.Size;

            Icon = Properties.Resources.favicon;
            connectionManagers = new() { ["rdp"] = new RDPManager(this), ["ssh"] = new SSHManager(this) };

            Resize += MainForm_Resize;

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

        private bool flagUpdatingBorderStyle = false;
        private void NotifyFormBorderStyleChange() =>
            PostWebMessage(new
            {
                type = "FORMBORDERSTYLE_CHANGE",
                formBorderStyle = FormBorderStyle
            });
        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (flagUpdatingBorderStyle) return;
            

            if (WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.Sizable)
            {
                flagUpdatingBorderStyle = true;

                // We can't directly set FormBorderStyle to None because it messes up the ClientSize
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;

                NotifyFormBorderStyleChange();
                flagUpdatingBorderStyle = false;
            }
            else if (WindowState == FormWindowState.Normal && FormBorderStyle == FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                NotifyFormBorderStyleChange();
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
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

            // prevent drag and drop of files from opening them in a new window
            // https://github.com/MicrosoftEdge/WebView2Feedback/issues/278#issuecomment-768227066
            webView.CoreWebView2.NewWindowRequested += (sender, args) => args.Handled = true;

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            // Fix maximized window size if resolution changes
            if (FormBorderStyle != FormBorderStyle.None) return;
            flagUpdatingBorderStyle = true;
            WindowState = FormWindowState.Normal;

            MaximumSize = Screen.FromControl(this).WorkingArea.Size;

            WindowState = FormWindowState.Maximized;
            flagUpdatingBorderStyle = false;
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