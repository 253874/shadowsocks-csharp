using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guldan.Services
{
    public class ServiceManager
    {
        public static readonly ServiceManager Instance = new ServiceManager();

        private ServiceManager()
        {
            _serviceMap = new Dictionary<Type, object>();
            _serviceMapLock = new object();
        }

        public void AddService<TServiceContract>(TServiceContract implementation)
            where TServiceContract : class
        {
            lock (_serviceMapLock)
            {
                _serviceMap[typeof(TServiceContract)] = implementation;
            }
        }

        public TServiceContract GetService<TServiceContract>()
            where TServiceContract : class
        {
            lock (_serviceMapLock)
            {
                if (_serviceMap.TryGetValue(typeof(TServiceContract), out object ot) && ot is TServiceContract service)
                {
                    return service;
                }
                return null;
            }
        }

        readonly Dictionary<Type, object> _serviceMap;
        readonly object _serviceMapLock;
    }
}
