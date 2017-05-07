using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Sabisu
{
    internal interface ISabisu
    {
        bool Handle(byte[] firstPacket, int length, Socket socket, object state);
    }
}
