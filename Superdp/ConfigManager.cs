using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superdp
{
    public  class ConfigManager
    {
        static readonly string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Superdp");
        static readonly string clientConfPath = Path.Combine(dataDir, "config.json");

        private readonly FileStream fileStream;

        private readonly Config config;

        public string ConfigJson { get => config.ToJson(); }

        public ConfigManager()
        {
            Directory.CreateDirectory(dataDir);
            fileStream = new FileStream(clientConfPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, false, 1024, true);
            config = Config.FromJson(streamReader.ReadToEnd()) ?? new Config();
        }

        public void Reconcile(string jsonChanges)
        {
            var configChanges = Config.FromJson(jsonChanges);
            if (configChanges != null)
                config.Patch(configChanges);
            WriteStream(fileStream, config.ToJson());
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
