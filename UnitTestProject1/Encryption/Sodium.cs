﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Shadowsocks;

namespace UnitTestProject1.Encryption
{
    public class Sodium
    {
        const string DLLNAME = "libsscrypto";

        static Sodium()
        {
            string dllPath = $"{Environment.CurrentDirectory}\\libsscrypto.dll";
            LoadLibrary(dllPath);
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int crypto_stream_salsa20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int crypto_stream_chacha20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int crypto_stream_chacha20_ietf_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, uint ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void ss_sha1_hmac_ex(byte[] key, uint keylen,
            byte[] input, int ioff, uint ilen,
            byte[] output);
    }
}

