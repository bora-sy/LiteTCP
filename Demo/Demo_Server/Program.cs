using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using LiteTCP.Events;
using LiteTCP.Server;

namespace Demo_Server
{
    internal class Program
    {
        private static LiteTCPServer server;
        static async Task Main(string[] args)
        {
            server = new LiteTCPServer(IPAddress.Parse("127.0.0.1"), 8080);

            Console.WriteLine("Started Listening");

            server.DataReceived += Server_DataReceived;

            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            server.StartListening();

            await Task.Delay(-1); // To prevent the console from closing
        }

        private static async void Server_DataReceived(object sender, TCPServerDataReceivedEventArgs e)
        {
            string data = e.GetDataAsString(Encoding.UTF8);
            Console.WriteLine($"Received data: " + data);

            string response = "Echo " + data;

            await e.CreateResponseAsync(response, Encoding.UTF8); // Creating response to the incoming data

        }

        private static async void Server_ClientConnected(object sender, TcpClient e)
        {
            Console.WriteLine($"New client connected");

            await server.SendAsync(e, "Welcome!", Encoding.UTF8); // Sending a welcome message to the client
        }

        private static void Server_ClientDisconnected(object sender, TcpClient e)
        {
            Console.WriteLine($"A client disconnected");
        }
    }
}
