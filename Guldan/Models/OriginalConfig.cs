using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guldan.Models
{
    [Serializable]
    public class OriginalServer
    {
        public string server { get; set; }
        public int server_port { get; set; }
        public string password { get; set; }
        public string method { get; set; }
        public string remarks { get; set; }
        public bool auth { get; set; }

        [SimpleJson.Ignore]
        public string FriendlyName
        {
            get
            {
                if (string.IsNullOrEmpty(server))
                {
                    return "New server";
                }
                if (string.IsNullOrEmpty(remarks))
                {
                    return server + ":" + server_port;
                }
                return remarks + " (" + server + ":" + server_port + ")";
            }
        }
        public string GetSSUrl()
        {
            return "ss://" + Convert.ToBase64String(Encoding.UTF8.GetBytes(method + ":" + password + "@" + server + ":" + server_port));
        }
    }

    [Serializable]
    public class OriginalConfig
    {
        public List<OriginalServer> configs;

        public int index;
        public bool global;
        public bool enabled;
        public bool shareOverLan;
        public bool isDefault;
        public int localPort;
        public string pacUrl;
        public bool useOnlinePac;

        public const string CONFIG_FILE = "gui-config.json";

        public OriginalServer GetCurrentServer()
        {
            if (index >= 0 && index < configs.Count)
                return configs[index];
            else
                return GetDefaultServer();
        }

        public static void CheckServer(OriginalServer server)
        {
            CheckPort(server.server_port);
            CheckPassword(server.password);
            CheckServer(server.server);
        }

        public static OriginalConfig Load(string path = null)
        {
            try
            {
                string configContent = File.ReadAllText(path ?? CONFIG_FILE);
                var config = G.DeSerializeJsonObject<OriginalConfig>(configContent);
                config.isDefault = false;
                if (config.localPort == 0)
                    config.localPort = 1080;
                if (config.index == -1)
                    config.index = 0;
                return config;
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException))
                {
                    //TODO
                }
                return new OriginalConfig
                {
                    index = 0,
                    isDefault = true,
                    localPort = 1080,
                    configs = new List<OriginalServer>
                    {
                        GetDefaultServer()
                    }
                };
            }
        }

        public static void Save(OriginalConfig config)
        {
            if (config.index >= config.configs.Count)
                config.index = config.configs.Count - 1;
            if (config.index < -1)
                config.index = -1;
            if (config.index == -1)
                config.index = 0;
            config.isDefault = false;
            try
            {
                File.WriteAllText(CONFIG_FILE, G.SerializeToJsonString(config));
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public static OriginalServer GetDefaultServer()
        {
            return new OriginalServer();
        }

        private static void Assert(bool condition)
        {
            if (!condition)
                throw new Exception("assertion failure");
        }

        public static void CheckPort(int port)
        {
            if (port <= 0 || port > 65535)
                throw new ArgumentException("Port out of range");
        }

        public static void CheckLocalPort(int port)
        {
            CheckPort(port);
            if (port == 8123)
                throw new ArgumentException("Port can't be 8123");
        }

        private static void CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password can not be blank");
        }

        private static void CheckServer(string server)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentException("Server IP can not be blank");
        }
    }
}
