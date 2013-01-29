using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace CreeperNetwork
{
    public static class TcpNetwork
    {
        private static TcpListener portListener;
        private static TcpClient homeClient;
        private static TcpClient awayClient;

        public static void InitializeTcpNetwork(String ipAddress = "10.1.25.146", int port = 29334)
        {
            InitializeListener(port);
            InitializeClients(ipAddress, port);
        }

        private static void InitializeListener(int port)
        {
            portListener = new TcpListener(IPAddress.Any, port);
            portListener.Start();
        }

        private static void InitializeClients(String ipAddress, int port)
        {
            homeClient = new TcpClient(ipAddress, port);
            awayClient = portListener.AcceptTcpClient();
        }

        public static void sendString(String dataIn)
        {
            NetworkStream networkStream = homeClient.GetStream();
            byte[] data = null;
            
            data = Encoding.ASCII.GetBytes(dataIn);
            networkStream.Write(data, 0, data.Length);

            //This may cause a problem, to be tested. 
            //If so, use homeClient.GetStream().Close() in closeConnection()
            networkStream.Close();
        }

        public static String receiveString()
        {
            String receivedData = "";
            byte[] data = new byte[256];
            int bytesRead = awayClient.GetStream().Read(data, 0, data.Length);
            MemoryStream memoryStream = new MemoryStream(data, 0, bytesRead);
            
            using (BinaryReader br = new BinaryReader(memoryStream))
            {
                data = br.ReadBytes((int)memoryStream.Length);
            }

            for (int i = 0; i < data.Length; i++)
            {
                receivedData += (char)data[i];
            }

            return receivedData;
        }

        public static void closeConnection()
        {
            portListener.Stop();
            homeClient.Close();
            awayClient.Close();
        }
    }
}
