using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Superdp
{
    class RDPManager : IConnectionManager
    {
        readonly private static Dictionary<string, RDPForm> rdpForms = new();

        private readonly HeroForm form;

        public IEnumerable<RDPForm> Forms { get => rdpForms.Values.Where(entry => entry.OwningForm == form); }

        internal RDPManager(HeroForm form)
        {
            this.form = form;
            this.form.FormClosing += HeroForm_FormClosing;
        }

        private void HeroForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            rdpForms
                .Where(entry => entry.Value.OwningForm == form)
                .Select(entry => entry.Key)
                .ToList()
                .ForEach(client_id => rdpForms.Remove(client_id));
        }

        public void Connect(dynamic options)
        {
            RDPConnectionParams want = new(options.client.host, options.client.username, options.client.password);

            // check if a form for the given client id already exists.
            if (rdpForms.ContainsKey(options.clientId))
            {
                RDPForm existing = rdpForms[options.clientId];
                if (existing.ConnectionParams == want)
                {
                    Update(options);
                    existing.Connect();
                    return;
                }

                Disconnect(options);
            }

            Debug.WriteLine("Instantiating...");
            var rdpForm = new RDPForm(form)
            {
                ClientId = options.clientId,
                TabId = options.tabId,
                Location = new Point(options.x, options.y),
                Size = new Size(options.width, options.height),
                ShouldBeVisible = options.visible,
                ConnectionParams = want
            };
            Debug.WriteLine("Instantiation complete...");
            rdpForms.Add(options.clientId, rdpForm);
            rdpForm.OnDisconnect += () =>
            {
                rdpForms.Remove(options.clientId);
                rdpForm.OwningForm.Controls.Remove(rdpForm);
                rdpForm.OwningForm.EnsureWebViewPositioning();
            };
            rdpForm.Connect();
        }

        public void Disconnect(dynamic options)
        {
            rdpForms.TryGetValue((string)options.clientId, out var rdpForm);
            rdpForm?.Disconnect();
        }

        public void Update(dynamic options)
        {
            if (!rdpForms.TryGetValue((string)options.clientId, out var rdpForm))
                return;
            
            rdpForm.SetOwningForm(form, options.tabId);
            rdpForm.Location = new Point(options.x, options.y);
            rdpForm.Size = new Size(options.width, options.height);
            rdpForm.ShouldBeVisible = options.visible;
        }
    }
}
