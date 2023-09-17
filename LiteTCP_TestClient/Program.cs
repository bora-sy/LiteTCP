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

            while(true)
            {
                Console.Write("Data: ");
                string data = Console.ReadLine();

                await client.SendAsync(data, Encoding.UTF8);
                Console.WriteLine("Sent\n");
            }

        }
    }
}
