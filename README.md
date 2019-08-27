# Borealis

> A .NET Library which contains Tcp Socket Framework which handles socket connections in Client-Server Model. It also has helper methods for basic CRUD database operations which is handled inside DataManager. It also contains DataBase class which is the same as DataManager but database is handled in memory which is faster in runtime and has a method for automatically applying any changes made to the table at runtime into the source table.

> An example usage of the library:
```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Borealis.Data.SqlClient;

namespace BorealisDataTest {
    public partial class Form1 : Form {
        SqlDataManager sqlDM = new SqlDataManager("localhost\\sqlexpress", "db1");

        public Form1() {
            InitializeComponent();
        }

        private void ins_Click(object sender, EventArgs e) {
            Dictionary<string, object> row = new Dictionary<string, object>();
            row.Add("first", fn.Text);
            row.Add("middle", mn.Text);
            row.Add("last", ln.Text);
            row.Add("address", adr.Text);
            sqlDM.Insert("tbl1", row);
            MessageBox.Show("Insert successful");
        }

        private void upt_Click(object sender, EventArgs e) {
            Dictionary<string, object> row = new Dictionary<string, object>();
            if (fn.Text != string.Empty) row.Add("first", fn.Text);
            if (mn.Text != string.Empty) row.Add("middle", mn.Text);
            if (ln.Text != string.Empty) row.Add("last", ln.Text);
            if (adr.Text != string.Empty) row.Add("address", adr.Text);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", id.Text);
            sqlDM.Update("tbl1", row, "id=@id", parameters);
            MessageBox.Show("Update successful");
        }

        private void sel_Click(object sender, EventArgs e) {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", id.Text);
            List<Dictionary<string, object>> selectedRows = sqlDM.Select("tbl1", "id=@id", parameters, "first", "middle", "last", "address");
            if (selectedRows.Count > 0) {
                fn.Text = selectedRows[0]["first"].ToString();
                mn.Text = selectedRows[0]["middle"].ToString();
                ln.Text = selectedRows[0]["last"].ToString();
                adr.Text = selectedRows[0]["address"].ToString();
                MessageBox.Show("Select successful");
            }
            else MessageBox.Show("Select unsuccessful");
        }

        private void del_Click(object sender, EventArgs e) {
            if (id.Text != string.Empty) {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@id", id.Text);
                sqlDM.Delete("tbl1", "id=@id", parameters);
            }
            else {
                sqlDM.Delete("tbl1");
            }
            MessageBox.Show("Delete successful");
        }
    }
}
```

> Another example usage of the library:
```csharp
using Borealis.Net;
using System;
using System.Collections.Generic;
using System.Net;

namespace chatsserver {
    class Program {
        static readonly List<Network> clients = new List<Network>();

        static readonly IPAddress serverHost = IPAddress.Parse("127.0.0.1");
        static readonly int serverPort = 7447;
        static readonly IPEndPoint serverAddress = new IPEndPoint(serverHost, serverPort);
        static readonly Server server = new Server(serverAddress);

        static void Main(string[] args) {
            server.ClientAccepted += Server_ClientAccepted;
            InitializeServerScript();
            server.Start();
            Console.WriteLine("[STATUS] Server started.");

            // Input Loop
            // Note: This also prevents the server from exiting after starting.
            string cmd = string.Empty;
            while (cmd != "exit") {
                cmd = Console.ReadLine();
                
                switch (cmd) {
                    default:
                        Console.WriteLine("[INFO] Invalid command.");
                        break;
                    case "help":
                        Console.WriteLine("[INFO] Valid commands:");
                        Console.WriteLine("[INFO] 'host' : Display the server's host address.");
                        Console.WriteLine("[INFO] 'port' : Display the server's port number.");
                        Console.WriteLine("[INFO] 'clct' : Display the number of clients handled by the server.");
                        break;
                    case "host":
                        Console.WriteLine("[INFO] Server Host Address: {0}", serverHost.ToString());
                        break;
                    case "port":
                        Console.WriteLine("[INFO] Server Port: {0}", serverPort);
                        break;
                    case "clct":
                        Console.WriteLine("[INFO] Number of Clients Handled: {0}", clients.Count);
                        break;
                }
            }
        }

        static void InitializeServerScript() {
            // Add network events here.
            // Note: Script class is a static class for handling Async network events
            // Note: Use "EventQueue" in Network abstract class for handling network events syncrounously.

            // Ex. An event when some client sends a message to the server with header "SEND"
            Script.Events.Add("SEND", delegate (Network client, ScriptEventArgs e) {
                // Gets the content of the first row in first column and send to all connected clients with header "RECEIVE"
                Broadcast("RECEIVE", e.Content.Items[0][0]);
            });
        }

        static void Server_ClientAccepted(Network newClient) {
            // Occurs when a client (the "newClient" parameter) connects to the server
            Console.WriteLine("[ACCEPT] New client connected.");
            clients.Add(newClient);
            newClient.Respond();
            
            // Broadcast to all connected clients that a new client has joined the server and displays its IP address
            Broadcast("JOIN", newClient.Socket.Client.RemoteEndPoint.ToString());
        }

        static void Broadcast(string header, string message) {
            for (int i = 0; i < clients.Count; i++) {
                clients[i].Write(header, message);
            }
        }
    }
}
```
