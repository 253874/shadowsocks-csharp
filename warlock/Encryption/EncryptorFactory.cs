using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shadowsocks.Encryption
{
    internal static class EncryptorFactory
    {
        private static readonly Dictionary<string, Type> _registeredEncryptors;

        static EncryptorFactory()
        {
            _registeredEncryptors = new Dictionary<string, Type>();
            foreach (string method in MbedTLSEncryptor.SupportedCiphers())
            {
                _registeredEncryptors.Add(method, typeof(MbedTLSEncryptor));
            }
            foreach (string method in SodiumEncryptor.SupportedCiphers())
            {
                _registeredEncryptors.Add(method, typeof(SodiumEncryptor));
            }
        }

        public static string[] GetEncryptorList()
        {
            return _registeredEncryptors.Select(c => c.Key).ToArray();
        }

        public static IEncryptor GetEncryptor(string method, string password, bool onetimeauth, bool isudp)
        {
            if (string.IsNullOrEmpty(method))
                method = "aes-256-cfb";
            method = method.ToLowerInvariant();
            if(!_registeredEncryptors.ContainsKey(method))throw new Exception("Encryptor Not Found");
            var t = _registeredEncryptors[method];
            IEncryptor result = Activator.CreateInstance(t, method, password, onetimeauth, isudp) as IEncryptor;
            return result;
        }
    }
}
