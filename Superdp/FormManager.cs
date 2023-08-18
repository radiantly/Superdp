using System.Runtime.InteropServices;
using System.Text;

namespace Superdp
{
    public class FormManager
    {
        static readonly string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Superdp");
        readonly string clientConfPath = Path.Combine(dataDir, "config.json");
        readonly FileStream confStream;
        string confStr;

        public readonly List<HeroForm> openForms = new();

        private const int WM_APP = 0x8000;
        public const int newInstanceMesssage = WM_APP + 1;

        public readonly MultiFormContext context;

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

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
                    SendMessage(handle, newInstanceMesssage, IntPtr.Zero, IntPtr.Zero);
                    Environment.Exit(0);
                }
                Environment.Exit(1);
            }


            confStr = ReadStream(confStream);
            context = new();
            CreateInstance();
        }

        public void BroadcastWebMessage(string arg, HeroForm skip)
        {
            foreach (HeroForm form in openForms)
            {
                if (form == skip) continue;
                form.PostWebMessage(arg);
            }
        }

        public HeroForm CreateInstance()
        {
            var form = new HeroForm(this);

            openForms.Add(form);
            form.FormClosed += (sender, e) => openForms.Remove(form);

            context.AddForm(form);
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
