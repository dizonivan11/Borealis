using System.Net.Sockets;

namespace Borealis.Net {
    public class BasicNetwork : Network {
        public BasicNetwork(TcpClient socket) : base(socket) {

        }
    }
}
