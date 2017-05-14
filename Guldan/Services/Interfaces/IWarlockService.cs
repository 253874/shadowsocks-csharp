using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guldan.Models;
using Shadowsocks;

namespace Guldan.Services.Interfaces
{
    public interface IWarlockService
    {
        ObservableCollection<Server> Servers { get; set; }
        Task StartAll();
        Task StopAll();
        void Save();
        void Load();
    }
}
