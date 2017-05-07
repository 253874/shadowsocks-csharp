using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Sabisu
{
    internal class PACServer:ISabisu
    {
        public string Content { get; set; } = @"function FindProxyForURL(url, host) {
var p = ""SOCKS5 127.0.0.1:1080"";
if (shExpMatch(host, ""*twitter.com*"")) return p;
if (shExpMatch(host, ""*t.co"")) return p;
if (shExpMatch(host, ""*bit.ly"")) return p;
if (shExpMatch(host, ""*j.mp"")) return p;
if (shExpMatch(host, ""*facebook.com*"")) return p;
if (shExpMatch(host, ""*facebook.net*"")) return p;
if (shExpMatch(host, ""*fbcdn.net"")) return p;
if (shExpMatch(host, ""*twimg.com"")) return p;
if (shExpMatch(host, ""*twitpic.com"")) return p;
if (shExpMatch(host, ""*yfrog.com"")) return p;
if (shExpMatch(host, ""*img.ly"")) return p;
if (shExpMatch(host, ""*cl.ly"")) return p;
if (shExpMatch(host, ""*campl.us"")) return p;
if (shExpMatch(host, ""*wp.com"")) return p;
if (shExpMatch(host, ""*wordpress.com"")) return p;
if (shExpMatch(host, ""*posterous.com"")) return p;
if (shExpMatch(host, ""*delicious.com"")) return p;
if (shExpMatch(host, ""*.static.flickr.com"")) return p;
if (shExpMatch(host, ""*google.com"")) return p;
if (shExpMatch(host, ""*google.co.uk"")) return p;
if (shExpMatch(host, ""*google.com.tw"")) return p;
if (shExpMatch(host, ""*googleusercontent.com"")) return p;
if (shExpMatch(host, ""*groups.google.com"")) return p;
if (shExpMatch(host, ""*doc.google.com"")) return p;
if (shExpMatch(host, ""*spreadsheets.google.com"")) return p;
if (shExpMatch(host, ""*vimeo.com"")) return p;
if (shExpMatch(host, ""*youtube.com"")) return p;
if (shExpMatch(host, ""*ytimg.com"")) return p;
if (shExpMatch(host, ""*googlevideo.com"")) return p;
if (shExpMatch(host, ""*tumblr.com"")) return p;
if (shExpMatch(host, ""*blogspot.com"")) return p;
if (shExpMatch(host, ""*blogger.com"")) return p;
if (shExpMatch(host, ""*foursquare.com"")) return p;
if (shExpMatch(host, ""*feedproxy.google.com"")) return p;
if (shExpMatch(host, ""*feeds.feedburner.com"")) return p;
if (shExpMatch(host, ""*upload.wikimedia.org"")) return p;
if (shExpMatch(host, ""*dropbox.com*"")) return p;
if (shExpMatch(host, ""*appspot.com*"")) return p;
if (shExpMatch(host, ""*imdb.com"")) return p;
if (shExpMatch(host, ""*code.google.com"")) return p;
if (shExpMatch(host, ""*android.com"")) return p;
if (shExpMatch(host, ""*thehitlistapp.com"")) return p;
return ""DIRECT"";
}";

        public bool Handle(byte[] firstPacket, int length, Socket socket, object state)
        {
            if (socket.ProtocolType != ProtocolType.Tcp)
            {
                return false;
            }
            try
            {
                string request = Encoding.UTF8.GetString(firstPacket, 0, length);
                string[] lines = request.Split('\r', '\n');
                bool hostMatch = false, pathMatch = false, useSocks = false;
                foreach (string line in lines)
                {
                    string[] kv = line.Split(new char[] { ':' }, 2);
                    if (kv.Length == 2)
                    {
                        if (kv[0] == "Host")
                        {
                            if (kv[1].Trim() == ((IPEndPoint)socket.LocalEndPoint).ToString())
                            {
                                hostMatch = true;
                            }
                        }
                        else if (kv[0] == "User-Agent")
                        {
                            // we need to drop connections when changing servers
                            /* if (kv[1].IndexOf("Chrome") >= 0)
                            {
                                useSocks = true;
                            } */
                        }
                    }
                    else if (kv.Length == 1)
                    {
                        if (line.IndexOf("pac", StringComparison.Ordinal) >= 0)
                        {
                            pathMatch = true;
                        }
                    }
                }
                if (hostMatch && pathMatch)
                {
                    SendResponse(firstPacket, length, socket, useSocks);
                    return true;
                }
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public void SendResponse(byte[] firstPacket, int length, Socket socket, bool useSocks)
        {
            try
            {
                string text = $@"HTTP/1.1 200 OK
Server: Shadowsocks
Content-Type: application/x-ns-proxy-autoconfig
Content-Length: {Encoding.UTF8.GetBytes(Content).Length}
Connection: Close

{Content}";
                byte[] response = Encoding.UTF8.GetBytes(text);
                socket.BeginSend(response, 0, response.Length, 0, SendCallback, socket);
                Utils.ReleaseMemory(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                socket.Close();
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket conn = (Socket)ar.AsyncState;
            try
            {
                conn.Shutdown(SocketShutdown.Send);
            }
            catch
            { }
        }
    }
}
