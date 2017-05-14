using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Guldan.Models;
using Guldan.Services.Interfaces;
using Shadowsocks;

namespace Guldan.Services
{
    public class WarlockService:IWarlockService
    {
        private static readonly string CONFIG_FILE = $"{G.MyName}.json";
        public ObservableCollection<Server> Servers { get; set; }
        public Task StartAll()
        {
            return Task.Run(() =>
            {
                Servers.Where(c=>c.enabled).AsParallel().ForAll(svc =>
                {
                    svc.Run(true);
                });
            });
        }

        public Task StopAll()
        {
            return Task.Run(() =>
            {
                Servers.Where(c => c.enabled).AsParallel().ForAll(svc =>
                {
                    svc.Run(false);
                });
            });
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(CONFIG_FILE, G.SerializeToJsonString(Servers.ToArray()), Encoding.UTF8);
            }
            catch (SecurityException)
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CONFIG_FILE), G.SerializeToJsonString(this), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                throw;
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    Servers = new ObservableCollection<Server>(G.DeSerializeJsonObject<Server[]>(File.ReadAllText(CONFIG_FILE)));
                    return;
                }
                var cf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CONFIG_FILE);
                if (File.Exists(cf))
                {
                    Servers = new ObservableCollection<Server>(G.DeSerializeJsonObject<Server[]>(File.ReadAllText(cf)));
                }
            }
            catch
            {
                //TODO hehe
            }
        }
    }
}
