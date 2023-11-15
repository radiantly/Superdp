using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Superdp
{
    internal class SSHManager : IConnectionManager
    {
        readonly private static Dictionary<string, SshController> sshControllers = new();

        private static SshController Get(HeroForm form, dynamic options)
        {
            if (!sshControllers.ContainsKey(options.tabId))
            {
                var controller = new SshController(form, options.tabId);
                sshControllers.Add(options.tabId, controller);
            }

            // sshControllers[options.tabId].SetOwningForm(form, options.tabId);
            return sshControllers[options.tabId];
        }

        private readonly HeroForm form;

        internal SSHManager(HeroForm form)
        {
            this.form = form;
        }

        public void Connect(dynamic options)
        {
            if (!sshControllers.ContainsKey(options.tabId))
                sshControllers.Add(options.tabId, new SshController(form, options.tabId));

            sshControllers[options.tabId].Connect(options.client.host, options.client.username);
        }

        public void Input(string tabId, string text)
        {
            if (!sshControllers.TryGetValue(tabId, out var controller))
                return;

            controller.Input(text);
        }

        public void Disconnect(dynamic options)
        {
        }

        public void Update(dynamic options)
        {
            if (!sshControllers.TryGetValue((string)options.tabId, out var controller))
                return;

            try
            {
                controller.Resize(options.rows, options.cols);
            } catch { }
            controller.SetOwningForm(form);
        }
    }
}
