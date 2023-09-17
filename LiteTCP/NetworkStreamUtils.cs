using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteTCP
{
    internal static class NetworkStreamUtils
    {
        public static async Task<byte[]> readDataFromNetworkStreamAsync(NetworkStream stream)
        {
            byte[] lengthBytes = await readWithLengthAsync(stream, 4);

            if (lengthBytes == null) return null;


            byte[] data = await readWithLengthAsync(stream, BitConverter.ToInt32(lengthBytes, 0));


            return data;
        }

        private static async Task<byte[]> readWithLengthAsync(NetworkStream stream, int length)
        {
            try
            {
                byte[] result = new byte[length];

                int bytesLeft = length;

                while (bytesLeft != 0)
                {
                    int offset = length - bytesLeft;
                    
                    int i = await stream.ReadAsync(result, offset, bytesLeft);
                    bytesLeft -= i;
                }

                return result;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static async Task<bool> writeDataToNetworkStreamAsync(NetworkStream stream, byte[] bytes)
        {
            try
            {
                byte[] lengthBytes = BitConverter.GetBytes(bytes.Length);

                byte[] data = new byte[lengthBytes.Length + bytes.Length];

                lengthBytes.CopyTo(data, 0);

                bytes.CopyTo(data, 4);

                await stream.WriteAsync(data, 0, data.Length);

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
