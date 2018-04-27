using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;

public delegate void DisconnectedHandler();

namespace Borealis.Net
{
    public abstract class Network
    {
        public const int BUFFER_SIZE = 10240;
        public const int TIMEOUT = 30000;
        public const int PINGDELAY = 5000;
        public static Encoding ENCODER;

        public static string ENUMERATOR = "&";
        public static string ENUMERATOR_END = "?";
        public static string EOS = ";";
        public static string SPLITTER = "|";

        public static string ENCODED_ENUMERATOR = "_%7%_";
        public static string ENCODED_ENUMERATOR_END = "_%/%_";
        public static string ENCODED_EOS = "_%:%_";
        public static string ENCODED_SPLITTER = "_%\\%_";

        static Network() {
            ENCODER = Encoding.UTF8;
        }

        public TcpClient socket;
        public byte[] buffer;
        public string data;

        // PING CONTROLLER (for response detection)
        private Stopwatch timer;
        private double timeWaited;

        // PING DELAY CONTROLLER
        private bool stillResponding;
        private Timer delayer; // referenced to avoid being collected by the GC

        public event DisconnectedHandler Disconnected;
        protected virtual void OnDisconnected() { Disconnected?.Invoke(); }

        public Network(TcpClient socket) {
            this.socket = socket;
            buffer = new byte[BUFFER_SIZE];
            data = string.Empty;

            timer = new Stopwatch();
            timeWaited = 0;

            stillResponding = false;
            delayer = new Timer(PINGDELAY) {
                AutoReset = true
            };
            delayer.Start();
            delayer.Elapsed += Ping;
        }

        private void Ping(object sender, System.Timers.ElapsedEventArgs e) {
            if (stillResponding) {
                stillResponding = false;
                try {
                    Console.WriteLine("{0} still responding", socket.Client.RemoteEndPoint.ToString());
                } catch {
                    Console.WriteLine("Unknown address not responding");
                }
                WritePing();
            }
        }

        public async void Connect(IPEndPoint ipEndPoint) {
            if (!socket.Connected) await socket.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
            socket.GetStream().BeginRead(buffer, 0, buffer.Length, new AsyncCallback(Read), null);
            WritePing();
        }

        Thread ping;
        public void Respond() {
            ping = new Thread(delegate () {
                while (socket != null) {
                    timeWaited += timer.Elapsed.TotalMilliseconds;
                    timer.Restart();
                    if (timeWaited > TIMEOUT) {
                        Console.WriteLine("Connection timeout for {0}", socket.Client.RemoteEndPoint.ToString());
                        Disconnect();
                        return;
                    }
                }
            });
            socket.GetStream().BeginRead(buffer, 0, buffer.Length, new AsyncCallback(Read), null);
            timer.Start();
            ping.Start();
        }

        private bool hasDisconnected = false;
        public void Disconnect() {
            if (hasDisconnected) return;
            OnDisconnected();
            hasDisconnected = true;
            try {
                Console.WriteLine("{0} disconnected", socket?.Client.RemoteEndPoint.ToString());
            } catch {
                Console.WriteLine("Unknown address disconnected");
            }
            socket?.Close();
            timer?.Stop();
            ping?.Abort();
        }

        void Read(IAsyncResult ar) {
            try {
                NetworkStream stream = socket.GetStream();
                int byteSize = stream.EndRead(ar);
                timeWaited = 0;
                Console.WriteLine("{0} raw data length: {1}", socket.Client.RemoteEndPoint.ToString(), byteSize);

                if (byteSize > 0) {
                    data += ENCODER.GetString(buffer);

                    if (data.IndexOf('\0') > -1) {
                        stillResponding = true;
                        data = data.Replace("\0", string.Empty);
                    }
                    Console.WriteLine("{0} raw data: {1}", socket.Client.RemoteEndPoint.ToString(), data);

                    if (data.IndexOf(EOS) > -1) {
                        string[] requests = data.Split(new string[] { EOS }, StringSplitOptions.None);
                        for (int i = 0; i < requests.Length - 1; i++) {
                            Console.WriteLine("{0} sent data: {1}", socket.Client.RemoteEndPoint.ToString(), requests[i]);
                            requests[i] = requests[i].Replace(ENCODED_EOS, EOS);
                            requests[i] = requests[i].Replace(ENCODED_SPLITTER, SPLITTER);
                            ScriptEventArgs args = new ScriptEventArgs(requests[i]);
                            Script.Run(this, args);
                        }
                        data = requests[requests.Length - 1];
                    }
                    Console.WriteLine("{0} remaining data: {1}", socket.Client.RemoteEndPoint.ToString(), data);
                } else {
                    Disconnect();
                    return;
                };
                buffer = new byte[BUFFER_SIZE];
                stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(Read), null);
            } catch (Exception ex) {
                Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        public async void WritePing() {
            try {
                NetworkStream stream = socket.GetStream();
                byte[] buffer = ENCODER.GetBytes(new char[] { '\0' });
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public async void Write(string header, string content) {
            try {
                NetworkStream stream = socket.GetStream();
                content = content.Replace(EOS, ENCODED_EOS);
                content = content.Replace(SPLITTER, ENCODED_SPLITTER);
                string data = header + SPLITTER + content + EOS;
                byte[] buffer = ENCODER.GetBytes(data);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
                Console.WriteLine(socket.Client.RemoteEndPoint.ToString() + " wrote " + data);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
