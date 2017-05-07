using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks
{
    public static class Logging
    {
        public static string LogFile;
        private static StreamWriterWithTimestamp sw;
        public static bool TouchLogFile(string fileName = null)
        {
            try
            {
                sw?.Dispose();
                LogFile = Path.Combine(Path.GetTempPath(), fileName?? $"Warlock_{DateTime.Now:HHmmss}.log");
                var fs = new FileStream(LogFile, FileMode.Append);
                sw = new StreamWriterWithTimestamp(fs) {AutoFlush = true};

                Console.SetOut(sw);

                Console.SetError(sw);

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        public static void Error(object o)
        {
            Console.WriteLine("[E] " + o);
        }

        public static void Info(object o)
        {
            Console.WriteLine(o);
        }

        public static void Debug(object o)
        {

#if DEBUG
            Console.WriteLine(o);
#endif
        }

        public static void Debug(string s)
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        }

        public static void Debug(EndPoint local, EndPoint remote, int len, string header = null, string tailer = null)
        {
#if DEBUG
            if (header == null && tailer == null)
                Debug($"{local} => {remote} (size={len})");
            else if (header == null)
                Debug($"{local} => {remote} (size={len}), {tailer}");
            else if (tailer == null)
                Debug($"{header}: {local} => {remote} (size={len})");
            else
                Debug($"{header}: {local} => {remote} (size={len}), {tailer}");
#endif
        }

        public static void Debug(Socket sock, int len, string header = null, string tailer = null)
        {
#if DEBUG
            Debug(sock.LocalEndPoint, sock.RemoteEndPoint, len, header, tailer);
#endif
        }

        public static void LogUsefulException(Exception e)
        {
            // just log useful exceptions, not all of them
            if (e is SocketException)
            {
                SocketException se = (SocketException)e;
                if (se.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    // closed by browser when sending
                    // normally happens when download is canceled or a tab is closed before page is loaded
                }
                else if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // received rst
                }
                else if (se.SocketErrorCode == SocketError.NotConnected)
                {
                    // close when not connected
                }
                else
                {
                    Console.WriteLine(e);
                }
            }
            else if (e is ObjectDisposedException)
            {
            }
            else
            {
                Console.WriteLine(e);
            }
        }
    }

    // Simply extended System.IO.StreamWriter for adding timestamp workaround
    internal class StreamWriterWithTimestamp : StreamWriter
    {
        public StreamWriterWithTimestamp(Stream stream) : base(stream)
        {
        }

        private string GetTimestamp()
        {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ";
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(GetTimestamp() + value);
        }

        public override void Write(string value)
        {
            base.Write(GetTimestamp() + value);
        }
    }
}
