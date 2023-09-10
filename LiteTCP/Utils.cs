using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP
{
    internal static class Utils
    {
        public static byte[] readDataFromNetworkStream(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];
            int lengthDataLengthLeft = lengthBytes.Length;
            while (lengthDataLengthLeft != 0)
            {
                int startIndex = lengthBytes.Length - lengthDataLengthLeft;
                int i = stream.Read(lengthBytes, startIndex, lengthDataLengthLeft);
                lengthDataLengthLeft -= i;
            }

            int dataLength = BitConverter.ToInt32(lengthBytes, 0);

            byte[] data = new byte[dataLength];

            int dataLengthLeft = dataLength;
            while (dataLengthLeft != 0)
            {
                int startIndex = dataLength - dataLengthLeft;
                int i = stream.Read(data, startIndex, dataLengthLeft);
                dataLengthLeft -= i;
            }

            return data;
        }

        public static void writeDataToNetworkStream(byte[] data, NetworkStream stream)
        {
            int length = data.Length;
            byte[] lengthBytes = BitConverter.GetBytes(length);

            byte[] toSend = new byte[lengthBytes.Length + data.Length];

            lengthBytes.CopyTo(toSend, 0);
            data.CopyTo(toSend, lengthBytes.Length);


            stream.Write(toSend, 0, toSend.Length);
        }
    }
}
