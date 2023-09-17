using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LiteTCP;
using LiteTCP.Server;

namespace LiteTCP_TestServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            LiteTCPServer server = new LiteTCPServer(IPAddress.Parse("127.0.0.1"), 8080);

            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.DataReceived += Server_DataReceived;

            server.StartListening();
            Console.WriteLine("Started Listening");

            while(true)
            {
                string toBroadcast = Console.ReadLine();

                Dictionary<TcpClient, bool> successStatus = await server.BroadcastAsync(toBroadcast, Encoding.UTF8);

                bool[] successValues = successStatus.Values.ToArray();

                int successCount = successValues.Count(x => x == true);
                int failCount = successValues.Count(x => x == false);

                Console.WriteLine($"Broadcasted. Success: {successCount} // Fail: {failCount}");
            }
        }

        private static void Server_DataReceived(object sender, LiteTCP.Events.TCPDataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);
            Console.WriteLine("Received Data: " + data);
        }

        private static void Server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            int p = ((IPEndPoint)e.Client.RemoteEndPoint).Port;
            Console.WriteLine($"Client Disconnected (Port: {p})");
        }

        private static void Server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            int p = ((IPEndPoint)e.Client.RemoteEndPoint).Port;
            Console.WriteLine($"Client Connected (Port: {p})");
        }
    }
}
