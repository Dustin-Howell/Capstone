using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

/**********************************************
 * Class: Network
 * Description: Contains all the functions 
 * necessary to create a working, networked 
 * game. 
 **********************************************/
namespace CreeperNetwork
{
    public static class Network
    {
        //Network Constants
        public const int PROTOCOL_VERSION = 1;
        public const int SERVER_PORT = 1875;
        public const int ACKNOWLEDGEMENT_TIMER = 100;
        public const int PACKET_LOSS = 1000;
        public const int CONNECTION_TIMEOUT = 1000;

        private const string BROADCAST_IP = "255.255.255.255";
        private const int MAX_PACKET_SIZE = 256;
        private const byte PACKET_SIGNATURE = 0xC0;
        private const byte CMD_FIND_SERVER = 0x01;
        private const byte CMD_OFFER_GAME = 0x02;
        private const byte CMD_JOIN_GAME = 0x03;
        private const byte CMD_START_GAME = 0x04;
        private const byte CMD_DISCONNECT = 0x05;
        private const byte CMD_MAKE_MOVE = 0x06;
        private const byte CMD_CHAT = 0x07;
        private const byte CMD_ACK = 0x08;

        //Network Variables
        private static int sequenceNumber = -1;
        private static int otherSequenceNumber = -1;
        private static Boolean isServer = false;
        private static Boolean gameRunning = false;
        private static UdpClient listener = new UdpClient(SERVER_PORT);
        private static UdpClient sender = new UdpClient();
        private static IPEndPoint ipOfLastPacket = new IPEndPoint(IPAddress.Any, SERVER_PORT);

        //Game variables
        static string hostGameName = "";
        static string hostPlayerName = "";
        static string clientPlayerName = "";


        /******************************
         * Function: hostGame
         * Description: 
         *****************************/
        public static void hostGame(string gameName, string playerName)
        {
            isServer = true;
            hostGameName = gameName;
            hostPlayerName = playerName;
            startServer();
        }

        /******************************
         * Function: findGames
         * Description: 
         *****************************/
        public static string[,] findGames()
        {
            isServer = false;
            return searchForGames();
        }

        public static void joinGame(string[] gameIn)
        {
            sendPacket(JoinGame(gameIn), gameIn[0]);
        }

        public static void startGame()
        {
            sendPacket(StartGame(), ipOfLastPacket.Address.ToString());
        }

        public static void runGame()
        {
            gameRunning = true;

            while (gameRunning)
            {
                //first let's check to see if there are any packets out there...
                byte[] packet = new byte[MAX_PACKET_SIZE];

                listener.Client.ReceiveTimeout = CONNECTION_TIMEOUT;

                try
                {
                    packet = receivePacket();
                }
                catch (Exception)
                {
                    packet = null;
                }

                listener.Client.ReceiveTimeout = 0;

                if (packet != null && packet[0] == PACKET_SIGNATURE)
                {
                    //certain commands shouldn't come at this point...let's ignore FindServers, OfferGame, JoinGame, StartGame
                    //only disconnect, ack, or a control command should arrive.
                    if (packet[1] == CMD_ACK)
                    {
                        otherSequenceNumber = BitConverter.ToInt32(packet, 2);

                        //to be erased....
                        Console.WriteLine("StartGame Acknowledged.");
                    }
                    else if (packet[1] == CMD_DISCONNECT)
                    {
                        //this is game over...should shut it all down now. 
                        gameRunning = false;
                    }
                    else if (packet[1] == CMD_CHAT)
                    {
                        otherSequenceNumber = BitConverter.ToInt32(packet, 2);
                        sendPacket(Ack(), ipOfLastPacket.Address.ToString());
                    }
                    else if (packet[1] == CMD_MAKE_MOVE)
                    {
                        otherSequenceNumber = BitConverter.ToInt32(packet, 2);
                        sendPacket(Ack(), ipOfLastPacket.Address.ToString());
                    }
                }

                //now we can send our control commands...
                if (sequenceNumber == otherSequenceNumber)
                {
                    //control command...make move, chat
                    //let's set some flags that can be set statically. 
                    //This'll make it easier to keep track. 


                }
            }
        }

        /******************************
         * Function: startServer
         * Description: 
         *****************************/
        public static void startServer()
        {
            while (isServer)
            {
                byte[] packet = new byte[MAX_PACKET_SIZE];
                packet = receivePacket();

                //Is someone looking for this server?
                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_FIND_SERVER)
                {
                    sendPacket(OfferGame(), ipOfLastPacket.Address.ToString());
                }

                //Did someone want to join this server?
                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_JOIN_GAME)
                {
                    //should probably authenticate here...or maybe when searching for games...or both? Yeah, probably both.
                    Boolean gameValid = true;

                    if (gameValid)
                    {
                        sequenceNumber = BitConverter.ToInt32(packet, 2);
                        otherSequenceNumber = sequenceNumber;
                        clientPlayerName = BitConverter.ToString(packet, 23, 9);
                        break;
                    }
                    else
                    {
                        sendPacket(Disconnect(), ipOfLastPacket.Address.ToString());
                    }
                }
            }
        }

        public static string[,] searchForGames()
        {
            string[,] games = new string[256, 7];
            int gameCounter = 0;
            Stopwatch serverStopwatch = new Stopwatch();

            sendPacket(FindServers(), BROADCAST_IP);

            //Start time...
            listener.Client.ReceiveTimeout = CONNECTION_TIMEOUT;
            serverStopwatch.Start();

            while (serverStopwatch.Elapsed < TimeSpan.FromMilliseconds(CONNECTION_TIMEOUT))
            {
                byte[] packet = new byte[MAX_PACKET_SIZE];
                packet = receivePacket();

                if (packet == null)
                    break;

                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_OFFER_GAME)
                {
                    //sequence number...not sure what to do with it.
                    BitConverter.ToInt32(packet, 2);

                    games[gameCounter, 0] = ipOfLastPacket.Address.ToString();
                    games[gameCounter, 1] = packet[6].ToString();
                    games[gameCounter, 2] = BitConverter.ToInt32(packet, 7).ToString();
                    games[gameCounter, 3] = Encoding.ASCII.GetString(packet, 11, 8);
                    games[gameCounter, 4] = BitConverter.ToInt32(packet, 19).ToString();
                    games[gameCounter, 5] = Encoding.ASCII.GetString(packet, 23, 9);
                    games[gameCounter, 6] = packet[32].ToString();

                    gameCounter++;
                }
            }

            listener.Client.ReceiveTimeout = 0;

            serverStopwatch.Stop();

            return games;
        }

        /******************************
         * Function: sendPacket
         * Description: Sends a packet 
         * destination ip and port
         *****************************/
        public static void sendPacket(byte[] packetIn, String ipAddressIn)
        {
            sender.Send(packetIn, packetIn.Length, ipAddressIn, SERVER_PORT);
        }

        /******************************
         * Function: broadcastPacket
         * Description: broadcasts
         * a packet to all devices
         * on the network
         *****************************/
        public static void broadcastPacket(byte[] packetIn)
        {
            sendPacket(packetIn, BROADCAST_IP);
        }

        //IN PROGRESS
        public static void closeServer()
        {
            isServer = false;
        }

        //IN PROGRESS
        public static void closeClient()
        {

        }

        /******************************
         * Function: broadcastPacket
         * Description: broadcasts
         * a packet to all devices
         * on the network
         *****************************/
        public static byte[] receivePacket()
        {
            byte[] data = new byte[256];

            try
            {
                data = listener.Receive(ref ipOfLastPacket);
            }
            catch (Exception) { data = null; }

            return data;
        }

        static byte[] OfferGame()
        {
            byte[] packet = new byte[33];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_OFFER_GAME;

            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);

            packet[6] = PROTOCOL_VERSION;

            //game name length
            packet[7] = 0;
            packet[8] = 0;
            packet[9] = 0;
            packet[10] = 0;

            (BitConverter.GetBytes(hostGameName.Length)).CopyTo(packet, 7);

            //game name
            packet[11] = 0;
            packet[12] = 0;
            packet[13] = 0;
            packet[14] = 0;
            packet[15] = 0;
            packet[16] = 0;
            packet[17] = 0;
            packet[18] = 0;

            encoding.GetBytes(hostGameName).CopyTo(packet, 11);

            //player's name length
            packet[19] = 0;
            packet[20] = 0;
            packet[21] = 0;
            packet[22] = 0;

            (BitConverter.GetBytes(hostPlayerName.Length)).CopyTo(packet, 19);

            //player name
            packet[23] = 0;
            packet[24] = 0;
            packet[25] = 0;
            packet[26] = 0;
            packet[27] = 0;
            packet[28] = 0;
            packet[29] = 0;
            packet[30] = 0;
            packet[31] = 0;

            encoding.GetBytes(hostPlayerName).CopyTo(packet, 23);

            //who moves first? 1 I do, 0 you do
            packet[32] = Convert.ToByte(new Random().Next(100) % 2 == 0);

            return packet;
        }

        static byte[] FindServers()
        {
            byte[] packet = new byte[6];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_FIND_SERVER;

            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);

            return packet;
        }

        //note that the first element of gameIn is the ipAddress of the game
        static byte[] JoinGame(string[] gameIn)
        {
            byte[] packet = new byte[33];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_JOIN_GAME;

            //sequence number
            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);

            //game protocol version
            packet[6] = Convert.ToByte(gameIn[1]);

            //game name length
            (BitConverter.GetBytes(Convert.ToInt32(gameIn[2]))).CopyTo(packet, 7);

            //game name
            packet[11] = 0;
            packet[12] = 0;
            packet[13] = 0;
            packet[14] = 0;
            packet[15] = 0;
            packet[16] = 0;
            packet[17] = 0;
            packet[18] = 0;

            (encoding.GetBytes(gameIn[3])).CopyTo(packet, 11);
            hostGameName = gameIn[3];

            //player name length NEED TO CHANGE
            packet[19] = 0;
            packet[20] = 0;
            packet[21] = 0;
            packet[22] = 0;

            (BitConverter.GetBytes(4)).CopyTo(packet, 19);

            //player name NEED TO CHANGE
            packet[23] = 0;
            packet[24] = 0;
            packet[25] = 0;
            packet[26] = 0;
            packet[27] = 0;
            packet[28] = 0;
            packet[29] = 0;
            packet[30] = 0;
            packet[31] = 0;

            (encoding.GetBytes("temp")).CopyTo(packet, 23);
            clientPlayerName = "temp";

            //who moves first? 1 I do, 0 you do
            packet[32] = Convert.ToByte(!Convert.ToBoolean(Convert.ToByte(gameIn[6])));

            return packet;
        }

        static byte[] StartGame()
        {
            byte[] packet = new byte[6];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_START_GAME;

            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);

            sequenceNumber++;

            return packet;
        }

        static byte[] Disconnect()
        {
            byte[] packet = new byte[6];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_DISCONNECT;

            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);

            return packet;
        }

        static byte[] MakeMove()
        {
            byte[] packet = new byte[11];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_MAKE_MOVE;

            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);

            //move type
            packet[6] = 0x01;

            //source
            packet[7] = 0x01;
            packet[8] = 0x01;

            //destination
            packet[9] = 0x01;
            packet[10] = 0x01;

            return packet;
        }

        static byte[] Chat()
        {
            byte[] packet = new byte[11];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_MAKE_MOVE;

            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);

            //message length
            packet[6] = 0x01;
            packet[7] = 0x01;
            packet[8] = 0x01;
            packet[9] = 0x01;

            //message
            packet[10] = 0x01;
            packet[11] = 0x01;
            packet[12] = 0x01;
            packet[13] = 0x01;
            packet[14] = 0x01;
            packet[15] = 0x01;
            packet[16] = 0x01;
            packet[17] = 0x01;
            packet[18] = 0x01;
            packet[19] = 0x01;
            packet[20] = 0x01;
            packet[21] = 0x01;
            packet[22] = 0x01;
            packet[23] = 0x01;

            return packet;
        }

        static byte[] Ack()
        {
            byte[] packet = new byte[10];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_ACK;

            (BitConverter.GetBytes(sequenceNumber)).CopyTo(packet, 2);
            (BitConverter.GetBytes(otherSequenceNumber)).CopyTo(packet, 6);

            return packet;
        }
    }
}
