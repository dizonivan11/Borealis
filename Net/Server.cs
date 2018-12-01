using System.Net;
using System.Net.Sockets;

public delegate void AcceptHandler(Borealis.Net.Network newClient);

namespace Borealis.Net {
    public class Server {
        TcpListener listener;

        public event AcceptHandler ClientAccepted;
        protected virtual void OnClientAccepted(Network newNetwork) { ClientAccepted?.Invoke(newNetwork); }

        public Server(IPEndPoint iPEndPoint) {
            listener = new TcpListener(iPEndPoint);
        }

        public void Start() {
            listener.Start();
            AcceptNextClient();
        }

        private async void AcceptNextClient() {
            TcpClient newClient = await listener.AcceptTcpClientAsync();
            OnClientAccepted(new Network(newClient));
            AcceptNextClient();
        }
    }
}
