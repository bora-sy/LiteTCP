# LiteTCP
Lightweight TCP Server & Client for C#


## Installation
The package is available on [NuGet](https://nuget.org/packages/LiteTCP)

[![NuGet](https://img.shields.io/nuget/v/LiteTCP.svg?label=NuGet)](https://nuget.org/packages/LiteTCP)

## Usage
*Coming Soon*

## Examples
These examples are already built as a project. Check the [Demo Project](https://github.com/BoRa-SY/LiteTCP/tree/main/Demo) for more info


### Client:
```csharp
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using LiteTCP.Client;
using LiteTCP.Events;
namespace Demo_Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press {ENTER} to start the client");
            Console.ReadLine();

            LiteTCPClient client = new LiteTCPClient();

            client.DataReceived += Client_DataReceived;

            client.Connect(IPAddress.Parse("127.0.0.1"), 8080);
            Console.WriteLine("Connected to the server");

            while(true)
            {
                string dataToSend = Console.ReadLine();

                await client.SendAsync(dataToSend, Encoding.UTF8);
            }

        }

        private static void Client_DataReceived(object sender, TCPClientDataReceivedEventArgs e)
        {
            string data = e.GetDataAsString(Encoding.UTF8);
            Console.WriteLine("Received data: " + data);
        }
    }
}
```

<br>

### Server:
```csharp
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
```
## Documentation
*Coming Soon*

## TODO:

* Add Documentation
* Add Usage (Make it like a tutorial)
