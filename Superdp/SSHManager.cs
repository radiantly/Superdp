using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Superdp
{
    internal class SSHManager : IConnectionManager
    {
        readonly private static Dictionary<string, SshController> sshControllers = new();

        private readonly HeroForm form;

        internal SSHManager(HeroForm form)
        {
            this.form = form;
        }

        public void Connect(dynamic options)
        {
            string tabId = options.tabId;
            if (!sshControllers.ContainsKey(tabId))
            {
                sshControllers.Add(tabId, new SshController(form, options.tabId, options.rows, options.cols));
                sshControllers[tabId].OnDisconnect += () => sshControllers.Remove(tabId);
            }
            else
            {
                sshControllers[options.tabId].Resize(options.rows, options.cols);
            }

            sshControllers[options.tabId].Connect(options.client.host, options.client.username, options.client.key);
        }

        public void Input(string tabId, string text)
        {
            if (!sshControllers.TryGetValue(tabId, out var controller))
                return;

            controller.Input(text);
        }

        public void Disconnect(dynamic options)
        {
            if (!sshControllers.TryGetValue((string)options.tabId, out var controller))
                return;
            
            controller.Disconnect();
        }

        public void Update(dynamic options)
        {
            if (!sshControllers.TryGetValue((string)options.tabId, out var controller))
                return;

            controller.Resize(options.rows, options.cols);
        }

        public void Transfer(dynamic options)
        {
            if (!sshControllers.TryGetValue((string)options.tabId, out var controller))
                return;

            controller.SetOwningForm(form);
        }
    }
}
