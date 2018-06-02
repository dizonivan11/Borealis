using System.Net;
using System.Net.Sockets;

public delegate void AcceptHandler(TcpClient newClient);

namespace Borealis.Net {
    public class Server {
        TcpListener listener;

        public event AcceptHandler ClientAccepted;
        protected virtual void OnClientAccepted(TcpClient newClient) { ClientAccepted?.Invoke(newClient); }

        public Server(IPEndPoint iPEndPoint) {
            listener = new TcpListener(iPEndPoint);
        }

        public void Start() {
            listener.Start();
            AcceptNextClient();
        }

        private async void AcceptNextClient() {
            TcpClient newClient = await listener.AcceptTcpClientAsync();
            OnClientAccepted(newClient);
            AcceptNextClient();
        }
    }
}
