using System;
using System.Net.Sockets;
using System.Text;

namespace LiteTCP.Events
{
    public class TCPDataReceivedEventArgs : EventArgs
    {
        public TcpClient Client { get; private set; }
        public byte[] Data { get; private set; }

        public string GetDataAsString(Encoding encoding)
        {
            if(encoding == null) throw new ArgumentNullException(nameof(encoding));
            return encoding.GetString(Data);
        }

        public TCPDataReceivedEventArgs(TcpClient client, byte[] data)
        {
            this.Client = client;
            this.Data = data;
        }
    }
}
