using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guldan.Services.Interfaces;

namespace Guldan.Services
{
    public static class ServiceInjector
    {
        public static void InjectServices()
        {
            var svcMgr = ServiceManager.Instance;
            svcMgr.AddService<IDispatcherService>(new DispatcherService());
            svcMgr.AddService<IMessageService>(new MessageService(svcMgr.GetService<IDispatcherService>()));
        }
    }
}
