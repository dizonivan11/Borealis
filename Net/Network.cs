using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;

public delegate void AsyncEventHandler(Borealis.Net.Network sender);
public delegate void ErrorEventHandler(Borealis.Net.Network sender, Exception ex);

namespace Borealis.Net {
    public class Network {
        public readonly int BUFFER_SIZE = 512;
        public readonly int TIMEOUT = 30000; // 30 second timeout
        public readonly int PINGRATE = 5000; // Every 5 seconds
        public readonly Encoding ENCODER = Encoding.UTF8;

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
        protected virtual void OnDisconnected() { Disconnected?.Invoke(this); }
        public event AsyncEventHandler Connected;
        protected virtual void OnConnected() { Connected?.Invoke(this); }
        public event ErrorEventHandler ConnectionError;
        protected virtual void OnConnectionError(Exception ex) { ConnectionError?.Invoke(this, ex); }

        public Network(TcpClient socket) {
            Socket = socket;
            Buffer = new byte[BUFFER_SIZE];
            Data = string.Empty;

            RunScriptAsync = true;
            EventQueue = new Queue<Script>();

            timer = new Stopwatch();
            timeWaited = 0;

            stillResponding = false;
            delayer = new Timer(PINGRATE) {
                AutoReset = true
            };
            delayer.Start();
            delayer.Elapsed += Ping;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) return false;
            Network netObj = (Network)obj;
            return Socket.Client.RemoteEndPoint == netObj.Socket.Client.RemoteEndPoint;
        }

        private void Ping(object sender, System.Timers.ElapsedEventArgs e) {
            if (stillResponding) {
                stillResponding = false;
                try {
                    WritePing();
                } catch {
                    Console.WriteLine("Unknown address not responding.");
                }
            }
        }

        public async void Connect(IPEndPoint ipEndPoint) {
            if (!Socket.Connected)
                try {
                    await Socket.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
                } catch (Exception ex) {
                    OnConnectionError(ex);
                    return;
                }
            OnConnected();
            Socket.GetStream().BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(Read), null);
        }

        // Server initiates this function after completing handshake, its purposes are:
        // 1. To initiate the ping thread handling timeout and disconnection
        // 2. To start receiving responses from the client
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

                if (byteSize > 0) {
                    stillResponding = true;
                    Data += ENCODER.GetString(Buffer);

                    if (Data.IndexOf('\0') > -1) Data = Data.Replace("\0", string.Empty);
#if SHOW_RAW
                    Console.WriteLine("{0} raw data: {1}", Socket.Client.RemoteEndPoint.ToString(), Data);
#endif

                    if (Data.IndexOf(RequestData.EOT) > -1) {
                        string[] requests = Data.Split(RequestData.EOT);
                        for (int i = 0; i < requests.Length - 1; i++) {
#if SHOW_REQUEST
                            Console.WriteLine("{0} requested data: {1}", Socket.Client.RemoteEndPoint.ToString(), requests[i]);
#endif
                            RequestData request = RequestData.Destringify(requests[i]);
                            if (RunScriptAsync) {
                                // Run script directly in asynchronous mode (default)
                                Script.Run(this, request);
                            }  else {
                                // Instantiate and store script in queue which will run later in synchronous mode
                                Script syncScript = new Script(this, request);
                                EventQueue.Enqueue(syncScript);
                            }
                        }
                        // Store remaining request data after processing first request
                        Data = requests[requests.Length - 1];
                    }
#if SHOW_REMAIN
                    Console.WriteLine("{0} remaining data: {1}", Socket.Client.RemoteEndPoint.ToString(), Data);
#endif
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
                byte[] buffer = ENCODER.GetBytes(RequestData.Stringify(new RequestData {
                    { "header", "ping" },
                    { "time", timeWaited.ToString() }
                }));
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public async void Write(RequestData request) {
            try {
                NetworkStream stream = Socket.GetStream();
                byte[] buffer = ENCODER.GetBytes(RequestData.Stringify(request));
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
#if SHOW_WRITE
                Console.WriteLine(Socket.Client.RemoteEndPoint.ToString() + " wrote " + data);
#endif
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
