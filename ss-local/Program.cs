using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shadowsocks;

namespace ss_local
{
    class Program
    {
        private static string server;
        private static int server_port = 0;
        private static string local = "127.0.0.1";
        private static int local_port = 0;
        private static string password;
        private static string method;
        private static bool auth = false;

        static void Main(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-s":
                            server = args[++i];
                            break;
                        case "-p":
                            server_port = int.Parse(args[++i]);
                            break;
                        case "-l":
                            local_port = int.Parse(args[++i]);
                            break;
                        case "-k":
                            password = args[++i];
                            break;
                        case "-m":
                            method = args[++i];
                            break;
                        case "-b":
                            local = args[++i];
                            break;
                        case "-a":
                            auth = true;
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                PrintUseage();
            }
            if (string.IsNullOrEmpty(server) || server_port == 0 || local_port == 0 || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(method))
            {
                PrintUseage();
                return;
            }

            using (var ss = Warlock.Affliction(server, server_port, password, method, auth, local, local_port))
            {
                ss.Start();
                while (true)
                {
                    Console.WriteLine("Input \'exit\' or \'quit\' to quit");
                    var input = Console.ReadLine();
                    if (string.CompareOrdinal(input, "exit") == 0 || string.CompareOrdinal(input, "quit") == 0)
                        break;
                }
            }
            Console.WriteLine("失礼します");
        }

        static void PrintUseage()
        {
            Console.Write($@"

       {Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName)}

       -s <server_host>           host name or ip address of your remote server

       -p <server_port>           port number of your remote server

       -l <local_port>            port number of your local server

       -k <password>              password of your remote server

       -m <encrypt_method>        Encrypt method: {string.Join(",", Warlock.EncryptorList)}.

       [-b <local_address>]       local address to bind

       [-a]                       onetime auth
");
        }
    }
}
