using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Text;

namespace Superdp
{
    using static Native;
    public class FormManager : ApplicationContext
    {
        static readonly string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Superdp");
        readonly string clientConfPath = Path.Combine(dataDir, "config.json");
        readonly FileStream confStream;
        string confStr;

        public readonly List<HeroForm> Forms = [];

        public const int NewInstanceMesssage = WM_APP + 1;

        public FormManager()
        {
            Directory.CreateDirectory(dataDir);
            try
            {
                confStream = new FileStream(clientConfPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
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


            confStr = ReadStream(confStream);
            CreateInstance();
        }

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

        public string Conf
        {
            get => confStr;
            set { confStr = value; WriteStream(confStream, value); }
        }

        private static string ReadStream(FileStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
            return streamReader.ReadToEnd();
        }

        private static void WriteStream(FileStream stream, string contents)
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
            streamWriter.Write(contents);
        }
    }
}
