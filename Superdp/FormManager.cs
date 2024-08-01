using System.Diagnostics;
using System.Text;

namespace Superdp
{
    using static Native;
    public class FormManager : ApplicationContext
    {
        private readonly ConfigManager configManager;
        public string ConfigJson { get => configManager.ConfigJson; }

        public readonly List<HeroForm> Forms = [];

        public const int NewInstanceMesssage = WM_APP + 1;

        public FormManager()
        {
            try
            {
                configManager = new();
            }
            catch (IOException) // the assumed cause of this IOException is that an instance is already open. Of course, this may not be the case.
            {
                var handle = FindWindowByCaption(IntPtr.Zero, "Superdp");
                if (handle != IntPtr.Zero)
                {
                    PostMessage(handle, NewInstanceMesssage, IntPtr.Zero, IntPtr.Zero);
                    Environment.Exit(0);
                    return;
                }
                Environment.Exit(1);
                return;
            }

            CreateInstance();
        }

        public void SaveConfigChanges(string jsonChanges) => configManager.Reconcile(jsonChanges);
        public void BroadcastWebMessage(string arg, HeroForm skip)
        {
            foreach (HeroForm form in Forms)
            {
                if (form == skip) continue;
                form.PostWebMessage(arg);
            }
        }

        public HeroForm CreateInstance()
        {
            var form = new HeroForm() { Manager = this };

            Forms.Add(form);
            form.FormClosed += (sender, e) => 
            {
                Forms.Remove(form);
                if (Forms.Count == 0)
                    ExitThread();
            };

            form.Show();
            return form;
        }
    }
}
