using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSTSCLib;

namespace Superdp
{
    public enum RDPConnectionState : short
    {
        DISCONNECTED = 0,
        CONNECTED = 1,
        CONNECTING = 2
    }
    internal class RDPConnectionParams
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
        private HeroForm owningForm;
        private bool shouldBeVisible = false;
        private bool flagConnectAfterDisconnect = false;
        readonly private string clientId;
        readonly private AxMSTSCLib.AxMsRdpClient11NotSafeForScripting rdp;
        private RDPConnectionParams want = new(), have = new();
        public RDPForm(HeroForm parent, string clientId)
        {
            // the lines below were in the default form template
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;

            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Name = "RDPForm";
            Text = "RDPForm";
            owningForm = parent;
            this.clientId = clientId;

            owningForm.Controls.Add(this);
            owningForm.EnsureWebViewPositioning();

            Visible = false;
            BackColor = HeroForm.BackgroundColor;

            Load += RDPForm_Load;
            Resize += RDPForm_Resize;

            // rdp control
            rdp = new();
            rdp.BeginInit();
            Controls.Add(rdp);
            rdp.EndInit();
            rdp.OnConnected += Rdp_OnConnected;
            rdp.OnDisconnected += Rdp_OnDisconnected;
            rdp.OnAuthenticationWarningDisplayed += (sender, e) => Log("Authentication request");
        }

        private void RDPForm_Load(object? sender, EventArgs e)
        {
            rdp.Size = ClientSize;
            rdp.Location = new Point(0, 0);
        }

        public void Connect(string host, string username, string password)
        {
            want = new RDPConnectionParams(host, username, password);
            RequestConnect();
        }

        private void RequestConnect()
        {
            if (ConnectionState != RDPConnectionState.DISCONNECTED)
            {
                if (have == want)
                {
                    Log("Session already exists");
                    return;
                }

                if (flagConnectAfterDisconnect) return;

                Log("Disconnecting existing session");
                flagConnectAfterDisconnect = true;
                rdp.Disconnect();

                return;
            }


            Log(string.Format("Tring to connect to {0}{1}{2}", want.Host, want.Username == "" ? "" : $" as {want.Username}", want.Password == "" ? "" : $"/***"));

            have = want;

            rdp.Server = want.Host;
            rdp.AdvancedSettings9.MaxReconnectAttempts = 1;
            rdp.UserName = want.Username;
            var Secured = (IMsTscNonScriptable)rdp.GetOcx();
            Secured.ClearTextPassword = want.Password;
            rdp.AdvancedSettings9.ClearTextPassword = want.Password;
            rdp.AdvancedSettings9.EnableCredSspSupport = true;

            try
            {
                rdp.Connect();
            }
            catch
            {
                Log("Invalid connection parameters");
            }

        }

        private void UpdateRDPDisplaySize()
        {
            // TODO: Debounce this
            // Update: this is sorta debounced in the frontend
            try
            {
                rdp.UpdateSessionDisplaySettings((uint)Width, (uint)Height, (uint)Width, (uint)Height, 0, 1, 1);
            }
            catch { }
        }

        private void RDPForm_Resize(object? sender, EventArgs e) => UpdateRDPDisplaySize();

        public void SetPositioning(Point location, Size size)
        {
            Location = location;
            Size = size;
            rdp.Size = ClientSize;
        }

        public HeroForm OwningForm
        {
            get => owningForm;
            set
            {
                if (owningForm == value) return;
                owningForm.Controls.Remove(this);
                owningForm.Invalidate();

                owningForm.PostWebMessage(message =>
                    {
                        message.clientId = clientId;
                        message.type = "RDP_NEWOWNER";
                    });

                owningForm = value;
                owningForm.Controls.Add(this);
                owningForm.EnsureWebViewPositioning();
            }
        }

        public bool ShouldBeVisible
        {
            get { return shouldBeVisible; }
            set
            {
                if (shouldBeVisible == value) return;
                shouldBeVisible = value;
                if (shouldBeVisible == false) Visible = false;
                else if (ConnectionState == RDPConnectionState.CONNECTED) Visible = true;
                Debug.WriteLine("RDPForm visibility: " + Visible);
            }
        }

        public RDPConnectionState ConnectionState
        {
            get => (RDPConnectionState)rdp.Connected;
        }

        private void Rdp_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {
            Visible = false;
            owningForm.Invalidate();
            Log($"Disconnected({e.discReason}): {rdp.GetErrorDescription((uint)e.discReason, (uint)rdp.ExtendedDisconnectReason)}", "disconnect");
            if (flagConnectAfterDisconnect)
            {
                flagConnectAfterDisconnect = false;
                RequestConnect();
            }
        }

        private void Rdp_OnConnected(object? sender, EventArgs e)
        {
            if (ShouldBeVisible)
            {
                Visible = true;
                BringToFront();
                owningForm.EnsureWebViewPositioning();
                UpdateRDPDisplaySize();
            }
            Log("Connected", "connect");
        }

        public void Log(string content, string? @event = null)
        {
            owningForm.PostWebMessage(msg =>
            {
                msg.clientId = clientId;
                msg.type = "RDP_LOG";
                msg.content = content;
                msg.visibility = Visible;
                if (@event != null) msg.@event = @event;
            });
        }
    }
}
