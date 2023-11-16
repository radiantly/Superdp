using System.Diagnostics;
using MSTSCLib;

namespace Superdp
{
    public enum RDPConnectionState : short
    {
        DISCONNECTED = 0,
        CONNECTED = 1,
        CONNECTING = 2
    }
    public class RDPConnectionParams
    {
        public string Host { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }

        public RDPConnectionParams() : this("", "", "") { }
        public RDPConnectionParams(string host, string username, string password)
        {
            Host = host;
            Username = username;
            Password = password;
        }

        public override bool Equals(Object? obj) => obj is RDPConnectionParams other && other == this;
        public override int GetHashCode() => (Host, Username, Password).GetHashCode();
        public static bool operator ==(RDPConnectionParams left, RDPConnectionParams right) => left.Host == right.Host && left.Username == right.Username && left.Password == right.Password;
        public static bool operator !=(RDPConnectionParams left, RDPConnectionParams right) => !(left == right);
    }


    public class RDPForm : Form
    {
        private bool shouldBeVisible = false;
        private bool pendingRDPSizeUpdate = false;
        readonly private AxMSTSCLib.AxMsRdpClient11NotSafeForScripting rdp;

        public event Action? OnDisconnect;

        public required string ClientId { get; init; }
        public required string TabId { get; set; }

        public required RDPConnectionParams ConnectionParams { get; init; }

        public HeroForm OwningForm { get; private set; }

        public void SetOwningForm(HeroForm newForm, string tabId)
        {
            bool reparentForm = OwningForm != newForm;
            bool isNewTab = TabId != tabId;

            if (reparentForm)
            {
                OwningForm.Controls.Remove(this);
                OwningForm.Invalidate();
            }

            if (isNewTab)
            {
                OwningForm.PostWebMessage(new
                {
                    clientId = ClientId,
                    tabId = TabId,
                    type = "RDP_NEWOWNER"
                });
            }

            var srcForm = OwningForm;
            OwningForm = newForm;
            TabId = tabId;

            if (reparentForm)
            {
                OwningForm.Controls.Add(this);
                OwningForm.EnsureWebViewPositioning();

                if (srcForm.CloseOnTransfer)
                    srcForm.Close();
            }
        }

        public RDPForm(HeroForm parent)
        {
            // the lines below were in the default form template
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;

            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Name = "RDPForm";
            Text = "RDPForm";

            OwningForm = parent;
            OwningForm.Controls.Add(this);
            OwningForm.EnsureWebViewPositioning();

            Visible = false;
            BackColor = HeroForm.BackgroundColor;

            // rdp control
            rdp = new();
            rdp.BeginInit();
            Controls.Add(rdp);
            rdp.EndInit();
            rdp.OnConnected += Rdp_OnConnected;
            rdp.OnDisconnected += Rdp_OnDisconnected;
            rdp.OnAuthenticationWarningDisplayed += (sender, e) => Log("Authentication request");
            ClientSizeChanged += (sender, e) =>
            {
                rdp.Size = ClientSize;
                pendingRDPSizeUpdate = true;
                RequestRDPSizeUpdate();
            };
            VisibleChanged += (sender, e) => RequestRDPSizeUpdate();
        }

        public void Connect()
        {
            if (ConnectionState != RDPConnectionState.DISCONNECTED)
            {
                if (ConnectionState == RDPConnectionState.CONNECTED)
                    Log("Connecting to existing session", "connect");
                return;
            }

            Log(string.Format("Trying to connect to {0}{1}{2}", ConnectionParams.Host, ConnectionParams.Username == "" ? "" : $" as {ConnectionParams.Username}", ConnectionParams.Password == "" ? "" : $"/***"));

            rdp.Server = ConnectionParams.Host;
            rdp.AdvancedSettings9.MaxReconnectAttempts = 1;

            var split = ConnectionParams.Username.Split('\\', 2);
            if (split.Length == 2)
            {
                rdp.Domain = split[0];
                rdp.UserName = split[1];
            }
            else
            {
                rdp.UserName = ConnectionParams.Username;
            }

            var Secured = (IMsTscNonScriptable)rdp.GetOcx();
            Secured.ClearTextPassword = ConnectionParams.Password;
            rdp.AdvancedSettings9.ClearTextPassword = ConnectionParams.Password;
            rdp.AdvancedSettings9.EnableCredSspSupport = true;

            try
            {
                rdp.Connect();
            }
            catch (Exception ex)
            {
                Log($"Failure: {ex.Message}", "disconnect");
            }
        }

        public void Disconnect()
        {
            if (ConnectionState == RDPConnectionState.DISCONNECTED) return;
            rdp.OnDisconnected -= Rdp_OnDisconnected;
            rdp.Disconnect();
            HandleDisconnect(-1, "Forced disconnect.");
        }

        private void RequestRDPSizeUpdate()
        {
            // TODO: Debounce this
            // Update: this is sorta debounced in the frontend
            try
            {
                if (Visible && ConnectionState == RDPConnectionState.CONNECTED && pendingRDPSizeUpdate)
                {
                    pendingRDPSizeUpdate = false;
                    rdp.UpdateSessionDisplaySettings((uint)Width, (uint)Height, (uint)Width, (uint)Height, 0, 1, 1);
                }
            }
            catch { }
        }

        public bool ShouldBeVisible
        {
            get => shouldBeVisible;
            set
            {
                shouldBeVisible = value;
                Visible = shouldBeVisible && ConnectionState == RDPConnectionState.CONNECTED;
            }
        }

        public RDPConnectionState ConnectionState
        {
            get => (RDPConnectionState)rdp.Connected;
        }

        private void Rdp_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
            => HandleDisconnect(e.discReason, rdp.GetErrorDescription((uint)e.discReason, (uint)rdp.ExtendedDisconnectReason));

        private void HandleDisconnect(int code, string reason)
        {
            Visible = false;
            OwningForm.Invalidate();
            Log($"Disconnected({code}): {reason}", "disconnect");
            OnDisconnect?.Invoke();
        }

        private void Rdp_OnConnected(object? sender, EventArgs e)
        {
            if (ShouldBeVisible)
            {
                Visible = true;
                BringToFront();
                OwningForm.EnsureWebViewPositioning();
            }
            Log("Connected", "connect");
        }

        public void Log(string content, string? @event = null)
        {
            OwningForm.PostWebMessage(new
            {
                clientId = ClientId,
                tabId = TabId,
                type = "RDP_LOG",
                content,
                visibility = Visible,
                @event
            });
        }
    }
}
