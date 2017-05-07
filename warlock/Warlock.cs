using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Shadowsocks.Encryption;
using Shadowsocks.Sabisu;

namespace Shadowsocks
{
    public abstract class Warlock : IDisposable
    {
        public static string[] EncryptorList => EncryptorFactory.GetEncryptorList();

        public static Warlock Affliction(string server, int server_port, string password, string method, bool auth = false, string local_ip = "localhost", int local_port = 1080)
        {
            return new AfflictionWarlock(server, server_port, local_ip, local_port, password, method, auth);
        }

        public static void Demonology()
        {
            throw new NotImplementedException();
        }

        public static void Destruction()
        {
            throw new NotImplementedException();
        }

        internal Listener listener;
        public void Start()
        {
            listener?.Start();
            //var tmr = new System.Threading.Timer(state => Utils.ReleaseMemory(),null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
            ChangeBackgroundProcessing(false, true);
        }

        public void Dispose()
        {
            listener?.Stop();
            Utils.ReleaseMemory();
            ChangeBackgroundProcessing(false, false);
        }

        private static void ChangeBackgroundProcessing(bool process, bool start)
        {
            var ok = process
                ? SetPriorityClass(GetCurrentWin32ProcessHandle(),
                    start ? ProcessBackgroundMode.Start : ProcessBackgroundMode.End)
                : SetThreadPriority(GetCurrentWin32ThreadHandle(),
                    start ? ThreadBackgroundgMode.Start : ThreadBackgroundgMode.End);
            if (!ok) throw new System.ComponentModel.Win32Exception("ChangeBackgroundProcessing Failure");
        }

        private enum ThreadBackgroundgMode { Start = 0x10000, End = 0x20000 }

        private enum ProcessBackgroundMode { Start = 0x100000, End = 0x200000 }

        [DllImport("Kernel32", EntryPoint = "GetCurrentProcess", ExactSpelling = true)]
        private static extern SafeWaitHandle GetCurrentWin32ProcessHandle();

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetPriorityClass(SafeWaitHandle hprocess, ProcessBackgroundMode mode);

        [DllImport("Kernel32", EntryPoint = "GetCurrentThread", ExactSpelling = true)]
        private static extern SafeWaitHandle GetCurrentWin32ThreadHandle();

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetThreadPriority(SafeWaitHandle hthread, ThreadBackgroundgMode mode);

        [DllImport("Kernel32", SetLastError = true, EntryPoint = "CancelSynchronousIo")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CancelSynchronousIO(SafeWaitHandle hThread);
    }

    public class AfflictionWarlock : Warlock
    {
        public AfflictionWarlock(string server, int server_port, string local_ip, int local_port, string password, string method, bool auth)
        {
            var saba = new Saba(server, server_port, password, method, auth);
            listener = new Listener(local_ip,local_port,saba.Relay);
            Start();
        }
    }
}
