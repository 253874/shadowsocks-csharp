using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Sabisu
{
    internal class Saba
    {
        public Saba()
        {

        }
        public Saba(string Server, int Server_port, string Password, string Method, bool Auth)
        {
            server = Server;
            server_port = Server_port;
            password = Password;
            method = Method;
            auth = Auth;
        }
        public string server { get; set; }
        public int server_port { get; set; }
        public string password { get; set; }
        public string method { get; set; }
        public bool auth { get; set; }

        public ISabisu[] Relay => new ISabisu[] {new TCPRelay(this), new UDPRelay(this)};
    }

    internal class Listener
    {
        public class UDPState
        {
            public byte[] buffer = new byte[4096];
            public EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        private Socket _tcpSocket;
        private Socket _udpSocket;
        private readonly IPEndPoint localEndPoint;
        private readonly IEnumerable<ISabisu> _services;

        public Listener(string ip, int port, params ISabisu[] sabisu)
        {
            if (port > IPEndPoint.MaxPort || port < IPEndPoint.MinPort) throw new ArgumentOutOfRangeException(nameof(port), "Port Out Of Range");
            if (ip == "0.0.0.0")
            {
                localEndPoint = new IPEndPoint(IPAddress.Any, port);
            }
            else if (ip == "127.0.0.1" || string.CompareOrdinal(ip, "localhost") == 0)
            {
                localEndPoint = new IPEndPoint(IPAddress.Loopback, port);
            }
            else
            {
                if (IPAddress.TryParse(ip, out IPAddress parse))
                {
                    localEndPoint = new IPEndPoint(parse, port);
                }
                else
                {
                    throw new Exception("Invaild Ipaddress");
                }
            }
            _services = sabisu;
        }

        public override int GetHashCode()
        {
            return localEndPoint.GetHashCode() ^ _services.GetHashCode();
        }

        public void Start(bool withUdp = true)
        {
            if (Utils.CheckIfPortInUse(localEndPoint.Port))
                throw new Exception("Port already in use");
            try
            {
                // Create a TCP/IP socket.
                _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _tcpSocket.Bind(localEndPoint);
                _tcpSocket.Listen(1024);
                _tcpSocket.BeginAccept(AcceptCallback, _tcpSocket);
                if (withUdp)
                {
                    _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _udpSocket.Bind(localEndPoint);
                    var udpState = new UDPState();
                    _udpSocket.BeginReceiveFrom(udpState.buffer, 0, udpState.buffer.Length, 0, ref udpState.remoteEndPoint, RecvFromCallback, udpState);
                }
                Logging.Debug("Shadowsocks started");
            }
            catch (SocketException)
            {
                _tcpSocket?.Close();
                throw;
            }
        }

        public void Stop()
        {
            if (_tcpSocket != null)
            {
                _tcpSocket.Close();
                _tcpSocket = null;
            }
            if (_udpSocket != null)
            {
                _udpSocket.Close();
                _udpSocket = null;
            }
        }

        private void RecvFromCallback(IAsyncResult ar)
        {
            UDPState state = (UDPState)ar.AsyncState;
            try
            {
                int bytesRead = _udpSocket.EndReceiveFrom(ar, ref state.remoteEndPoint);
                foreach (var service in _services)
                {
                    if (service.Handle(state.buffer, bytesRead, _udpSocket, state))
                    {
                        break;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
            }
            finally
            {
                try
                {
                    _udpSocket.BeginReceiveFrom(state.buffer, 0, state.buffer.Length, 0, ref state.remoteEndPoint, RecvFromCallback, state);
                }
                catch (ObjectDisposedException)
                {
                    // do nothing
                }
                catch (Exception)
                {
                }
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            try
            {
                Socket conn = listener.EndAccept(ar);

                byte[] buf = new byte[4096];
                object[] state = {
                    conn,
                    buf
                };

                conn.BeginReceive(buf, 0, buf.Length, 0,ReceiveCallback, state);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                try
                {
                    listener.BeginAccept(AcceptCallback, listener);
                }
                catch (ObjectDisposedException)
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            object[] state = (object[])ar.AsyncState;

            Socket conn = (Socket)state[0];
            byte[] buf = (byte[])state[1];
            try
            {
                int bytesRead = conn.EndReceive(ar);
                foreach (var service in _services)
                {
                    if (service.Handle(buf, bytesRead, conn, null))
                    {
                        return;
                    }
                }
                // no service found for this
                if (conn.ProtocolType == ProtocolType.Tcp)
                {
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                conn.Close();
            }
        }
    }
}
