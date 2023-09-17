using LiteTCP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP_TestClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press {ENTER} to connect");
            Console.ReadLine();

            LiteTCPClient client = new LiteTCPClient();

            client.Connect(IPAddress.Parse("127.0.0.1"), 8080);
            client.DataReceived += Client_DataReceived;

            await Task.Delay(-1);
        }


        private static void Client_DataReceived(object sender, LiteTCP.Events.TCPDataReceivedEventArgs e)
        {
            Console.WriteLine($"Received: {e.GetDataAsString(Encoding.UTF8)}");
        }
    }
}
