using System;
using System.Runtime.InteropServices;

namespace Shadowsocks.Encryption
{
    internal class Sodium
    {
        const string DLLNAME = "libsscrypto";

        public static bool Available { get; }
        static Sodium()
        {
            var dllPath = Utils.TouchEncryptorDll();
            if (IntPtr.Size == 4)
            {
                Available = true;
                LoadLibrary(dllPath);
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int crypto_stream_salsa20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int crypto_stream_chacha20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int crypto_stream_chacha20_ietf_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, uint ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ss_sha1_hmac_ex(byte[] key, uint keylen,
            byte[] input, int ioff, uint ilen,
            byte[] output);
    }
}

