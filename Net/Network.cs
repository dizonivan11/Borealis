﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;

public delegate void AsyncEventHandler(Borealis.Net.Network sender);

namespace Borealis.Net {
    public class Network {
        public readonly int BUFFER_SIZE = 10240;
        public readonly int TIMEOUT = 30000;
        public readonly int PINGRATE = 5000;
        public readonly Encoding ENCODER = Encoding.UTF8;
        public readonly StringBuilder writeDataBuilder = new StringBuilder();

        public static readonly char[] PING = new char[] { '\0' };
        public static readonly char ENUMERATOR = '\t';
        public static readonly char ENUMERATOR_END = '\a';
        public static readonly char EOS = '\n';
        public static readonly char SPLITTER = '\v';

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
                    Console.WriteLine("Pinged {0} to keep connection alive.", Socket.Client.RemoteEndPoint.ToString());
                } catch {
                    Console.WriteLine("Unknown address not responding.");
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

                if (byteSize > 0) {
                    stillResponding = true;
                    Data += ENCODER.GetString(Buffer);

                    if (Data.IndexOf('\0') > -1) Data = Data.Replace("\0", string.Empty);
#if SHOW_RAW
                    Console.WriteLine("{0} raw data: {1}", Socket.Client.RemoteEndPoint.ToString(), Data);
#endif

                    if (Data.IndexOf(EOS) > -1) {
                        string[] requests = Data.Split(EOS);
                        for (int i = 0; i < requests.Length - 1; i++) {
#if SHOW_REQUEST
                            Console.WriteLine("{0} requested data: {1}", Socket.Client.RemoteEndPoint.ToString(), requests[i]);
#endif
                            ScriptEventArgs args = new ScriptEventArgs(requests[i]);
                            if (RunScriptAsync) Script.Run(this, args);
                            else {
                                Script syncScript = new Script(this, args);
                                EventQueue.Enqueue(syncScript);
                            }
                        }
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
                byte[] buffer = ENCODER.GetBytes(PING);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public async void Write(string header, string content) {
            try {
                NetworkStream stream = Socket.GetStream();
                writeDataBuilder.Clear();
                writeDataBuilder.Append(header);
                writeDataBuilder.Append(SPLITTER);
                writeDataBuilder.Append(content);
                writeDataBuilder.Append(EOS);
                byte[] buffer = ENCODER.GetBytes(writeDataBuilder.ToString());
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
