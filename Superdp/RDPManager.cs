using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Superdp
{
    class RDPManager : IConnectionManager
    {
        readonly private static Dictionary<string, RDPForm> tabForms = new();

        private readonly HeroForm form;

        public IEnumerable<RDPForm> Forms { get => tabForms.Values.Where(entry => entry.OwningForm == form); }

        internal RDPManager(HeroForm form)
        {
            this.form = form;
            this.form.FormClosing += HeroForm_FormClosing;
        }

        private void HeroForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            tabForms
                .Where(entry => entry.Value.OwningForm == form)
                .Select(entry => entry.Key)
                .ToList()
                .ForEach(tab_id => tabForms.Remove(tab_id));
        }

        public void Connect(dynamic options)
        {
            RDPConnectionParams want = new(options.client.host, options.client.username, options.client.password);

            // check if an rdp form for the current tab already exists
            if (tabForms.ContainsKey(options.tabId))
            {
                RDPForm existing = tabForms[options.tabId];
                if (existing.ConnectionParams == want)
                {
                    Update(options);
                    existing.SetOwningForm(form, options.tabId);
                    existing.Connect();
                    return;
                }
                
                existing.Disconnect();
            }

            var rdpForm = new RDPForm(form)
            {
                ClientId = options.clientId,
                TabId = options.tabId,
                Location = new Point(options.x, options.y),
                Size = new Size(options.width, options.height),
                ShouldBeVisible = options.visible,
                ConnectionParams = want
            };
            tabForms.Add(options.tabId, rdpForm);
            rdpForm.OnDisconnect += () =>
            {
                if (tabForms.ContainsKey(options.tabId) && tabForms[options.tabId] == rdpForm)
                    tabForms.Remove(options.tabId);
                rdpForm.OwningForm.Controls.Remove(rdpForm);
                rdpForm.OwningForm.EnsureWebViewPositioning();
            };
            rdpForm.Connect();
        }

        public void Disconnect(dynamic options)
        {
            tabForms.TryGetValue((string)options.tabId, out var rdpForm);
            rdpForm?.Disconnect();
        }

        public void Update(dynamic options)
        {
            if (!tabForms.TryGetValue((string)options.tabId, out var rdpForm))
                return;
            
            rdpForm.Location = new Point(options.x, options.y);
            rdpForm.Size = new Size(options.width, options.height);
            rdpForm.ShouldBeVisible = options.visible;
        }

        public void Transfer(dynamic options)
        {
            if (!tabForms.TryGetValue((string)options.tabId, out var rdpForm))
                return;

            rdpForm.SetOwningForm(form, options.tabId);
        }
    }
}
