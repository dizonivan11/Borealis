using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;

public delegate void AsyncEventHandler();

namespace Borealis.Net
{
    public abstract class Network
    {
        public const int BUFFER_SIZE = 10240;
        public const int TIMEOUT = 30000;
        public const int PINGDELAY = 5000;
        public static Encoding ENCODER;
        
        public static Encodable ENUMERATOR = new Encodable("&", "_%7%_");
        public static Encodable ENUMERATOR_END = new Encodable("?", "_%/%_");
        public static Encodable EOS = new Encodable(";", "_%:%_");
        public static Encodable SPLITTER = new Encodable("|", "_%\\%_");

        static Network() {
            ENCODER = Encoding.UTF8;
        }

        public TcpClient Socket { get; set; }
        public byte[] Buffer { get; set; }
        public string Data { get; set; }

        // Event Controller
        public bool RunScriptAsync { get; set; }
        public Queue<Script> EventQueue { get; set; }
        
        // PING CONTROLLER (for response detection)
        private Stopwatch timer;
        private double timeWaited;

        // PING DELAY CONTROLLER
        private bool stillResponding;
        private Timer delayer; // referenced to avoid being collected by the GC

        public event AsyncEventHandler Disconnected;
        protected virtual void OnDisconnected() { Disconnected?.Invoke(); }
        public event AsyncEventHandler Connected;
        protected virtual void OnConnected() { Connected?.Invoke(); }

        public Network(TcpClient socket) {
            Socket = socket;
            Buffer = new byte[BUFFER_SIZE];
            Data = string.Empty;

            RunScriptAsync = true;
            EventQueue = new Queue<Script>();

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
                    Console.WriteLine("{0} still responding", Socket.Client.RemoteEndPoint.ToString());
                } catch {
                    Console.WriteLine("Unknown address not responding");
                }
                WritePing();
            }
        }

        public async void Connect(IPEndPoint ipEndPoint) {
            if (!Socket.Connected) await Socket.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
            OnConnected();
            Socket.GetStream().BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(Read), null);
            WritePing();
        }

        Thread ping;
        public void Respond() {
            ping = new Thread(delegate () {
                while (Socket != null) {
                    timeWaited += timer.Elapsed.TotalMilliseconds;
                    timer.Restart();
                    if (timeWaited > TIMEOUT) {
                        Console.WriteLine("Connection timeout for {0}", Socket.Client.RemoteEndPoint.ToString());
                        Disconnect();
                        return;
                    }
                }
            });
            Socket.GetStream().BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(Read), null);
            timer.Start();
            ping.Start();
        }

        private bool hasDisconnected = false;
        public void Disconnect() {
            if (hasDisconnected) return;
            OnDisconnected();
            hasDisconnected = true;
            try {
                Console.WriteLine("{0} disconnected", Socket?.Client.RemoteEndPoint.ToString());
            } catch {
                Console.WriteLine("Unknown address disconnected");
            }
            Socket?.Close();
            timer?.Stop();
            ping?.Abort();
        }
        
        void Read(IAsyncResult ar) {
            try {
                NetworkStream stream = Socket.GetStream();
                int byteSize = stream.EndRead(ar);
                timeWaited = 0;
                Console.WriteLine("{0} raw data length: {1}", Socket.Client.RemoteEndPoint.ToString(), byteSize);

                if (byteSize > 0) {
                    stillResponding = true;
                    Data += ENCODER.GetString(Buffer);

                    if (Data.IndexOf('\0') > -1) Data = Data.Replace("\0", string.Empty);
                    Console.WriteLine("{0} raw data: {1}", Socket.Client.RemoteEndPoint.ToString(), Data);

                    if (Data.IndexOf(EOS.Decoded) > -1) {
                        string[] requests = Data.Split(new string[] { EOS.Decoded }, StringSplitOptions.None);
                        for (int i = 0; i < requests.Length - 1; i++) {
                            Console.WriteLine("{0} sent data: {1}", Socket.Client.RemoteEndPoint.ToString(), requests[i]);
                            requests[i] = requests[i].Decode(EOS);
                            requests[i] = requests[i].Decode(SPLITTER);
                            ScriptEventArgs args = new ScriptEventArgs(requests[i]);
                            if (RunScriptAsync) Script.Run(this, args);
                            else {
                                Script syncScript = new Script(this, args);
                                EventQueue.Enqueue(syncScript);
                            }
                        }
                        Data = requests[requests.Length - 1];
                    }
                    Console.WriteLine("{0} remaining data: {1}", Socket.Client.RemoteEndPoint.ToString(), Data);
                } else {
                    Disconnect();
                    return;
                };
                Buffer = new byte[BUFFER_SIZE];
                stream.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(Read), null);
            } catch (Exception ex) {
                Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        public async void WritePing() {
            try {
                NetworkStream stream = Socket.GetStream();
                byte[] buffer = ENCODER.GetBytes(new char[] { '\0' });
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public async void Write(string header, string content) {
            try {
                NetworkStream stream = Socket.GetStream();
                content = content.Encode(EOS);
                content = content.Encode(SPLITTER);
                string data = header + SPLITTER + content + EOS;
                byte[] buffer = ENCODER.GetBytes(data);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
                Console.WriteLine(Socket.Client.RemoteEndPoint.ToString() + " wrote " + data);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
