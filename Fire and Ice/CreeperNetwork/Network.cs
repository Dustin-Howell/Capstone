using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Timers;

namespace CreeperNetwork
{
    class Network
    {
        //Network Constants
        public const int PROTOCOL_VERSION = 1;
        public const int SERVER_PORT = 1875;
        public const int ALT_SERVER_PORT = 1876;
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
        private bool isServer = false;
        private bool serverFull = false;
        private Timer broadcastTimer = new Timer();
        private UdpClient listener = new UdpClient(SERVER_PORT);
        private UdpClient listenerAlt = new UdpClient(ALT_SERVER_PORT);
        private UdpClient sender = new UdpClient();
        private IPEndPoint ipOfLastPacket = new IPEndPoint(IPAddress.Any, SERVER_PORT);
        private int lastReceivedHomeSeqNum = 0;
        private int homeSequenceNumber = 0;
        private int awaySequenceNumber = -1;
        private bool acknowledged = false;
        private bool gameRunning = true;
        private string currentMessage = "";
        private bool newMessage = false;

        //Game variables
        private string hostGameName = "";
        private string hostPlayerName = "";
        private string clientPlayerName = "";


        //SERVER functions

        public void server_hostGame(string gameNameIn, string playerNameIn)
        {
            isServer = true;
            hostGameName = gameNameIn;
            hostPlayerName = playerNameIn;

            byte[] packet = new byte[MAX_PACKET_SIZE];

            //okay, now what if someone wants to join?
            //uh...while...
            while (!serverFull)
            {
                packet = receivePacket_blocking();

                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_FIND_SERVER)
                {
                    sendPacket(packet_OfferGame(), ipOfLastPacket.Address.ToString());
                }
                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_JOIN_GAME)
                {
                    if (!server_acceptClient(packet))
                    {
                        sendPacket(packet_Disconnect(), ipOfLastPacket.Address.ToString());
                    }
                }
            }
        }

        public bool server_acceptClient(byte[] packetIn)
        {
            bool accepted = false;

            //valid game..for now, just accept all.
            if (!serverFull)
            {
                serverFull = true;
                accepted = true;
                clientPlayerName = BitConverter.ToString(packetIn, 23, 9);
            }

            return accepted;
        }

        public void server_startGame()
        {
            sendPacket(packet_StartGame(), ipOfLastPacket.Address.ToString());

            try
            {
                //As soon as the client ACK the packet, we begin.
                byte[] packet = new byte[MAX_PACKET_SIZE];

                while (!acknowledged)
                {
                    packet = receivePacket_blocking();

                    if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_ACK)
                    {
                        //does the sequence number they sent us back match ours?
                        if (homeSequenceNumber == BitConverter.ToInt32(packet, 6))
                        {
                            awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                            lastReceivedHomeSeqNum = homeSequenceNumber;
                            acknowledged = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //timeout...
            }
        }

        //client functions

        /******************************
         * Function: findGames
         * Description: 
         *****************************/
        public string[,] client_findGames()
        {
            string[,] games = new string[256, 7];
            int gameCounter = 0;
            Stopwatch serverStopwatch = new Stopwatch();

            broadcastPacket(packet_FindServers());

            serverStopwatch.Start();

            while (serverStopwatch.Elapsed < TimeSpan.FromMilliseconds(CONNECTION_TIMEOUT))
            {
                byte[] packet = new byte[MAX_PACKET_SIZE];
                packet = receivePacket_blockWithTimeout();

                if (packet == null)
                    break;

                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_OFFER_GAME)
                {
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

            serverStopwatch.Stop();

            return games;
        }

        public void client_joinGame(string[] gameIn)
        {
            sendPacket(packet_JoinGame(gameIn), gameIn[0]);
        }

        public void client_ackStartGame()
        {
            byte[] packet = new byte[MAX_PACKET_SIZE];
            Boolean gameStarted = false;

            while (!gameStarted)
            {
                packet = receivePacket_blocking();

                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_START_GAME)
                {
                    awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                    sendPacket(packet_Ack(), ipOfLastPacket.Address.ToString());
                    gameStarted = true;
                    acknowledged = true;
                }
            }
        }

        //Game functions

        public void playGame()
        {
            byte[] packet = new byte[MAX_PACKET_SIZE];

            Console.WriteLine("GAME STARTED.");

            gameRunning = true;

            while (gameRunning)
            {
                packet = receivePacket_blocking();

                if (packet[0] == PACKET_SIGNATURE)
                {
                    //Did someone disconnect?
                    if (packet[1] == CMD_DISCONNECT)
                    {
                        gameRunning = false;
                    }
                    //Are we waiting for an ACK?
                    else if (!acknowledged)
                    {
                        if (packet[1] == CMD_ACK && homeSequenceNumber == BitConverter.ToInt32(packet, 6))
                        {
                            acknowledged = true;
                            awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                        }

                        lastReceivedHomeSeqNum = BitConverter.ToInt32(packet, 6);
                    }
                    //something else...
                    else if (packet[1] == CMD_CHAT)
                    {
                        currentMessage = Encoding.ASCII.GetString(packet, 10, 14);
                        awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                        newMessage = true;
                        acknowledged = true;
                        sendPacket(packet_Ack(), ipOfLastPacket.Address.ToString());
                    }
                    else if (packet[1] == CMD_MAKE_MOVE)
                    {
                        awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                        sendPacket(packet_Ack(), ipOfLastPacket.Address.ToString());
                        acknowledged = true;
                    }

                }
            }


        }

        public void disconnect()
        {
            sendPacket(packet_Disconnect(), ipOfLastPacket.Address.ToString());
            listener.Close();
            sender.Close();
        }

        public void keepAlive()
        {
            Timer ackTimer = new Timer();
            ackTimer.Elapsed += new ElapsedEventHandler(sendAckOnTime);
            // Set the Interval to 5 seconds.
            ackTimer.Interval = ACKNOWLEDGEMENT_TIMER;
            ackTimer.Enabled = true;
            ackTimer.AutoReset = true;

            byte[] packet;
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            while (gameRunning)
            {
                packet = receivePacket_blockWithTimeout_altPort();

                if (packet == null)
                {
                    Console.WriteLine("We have connection issues");
                }
                else if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_ACK)
                {
                    if (homeSequenceNumber == BitConverter.ToInt32(packet, 6))
                    {
                        stopwatch.Restart();
                    }

                    awaySequenceNumber = BitConverter.ToInt32(packet, 2);

                    if (stopwatch.ElapsedMilliseconds > PACKET_LOSS)
                    {
                        //resend the last command...whatever it was...
                        Console.WriteLine("Packet lost. Resend the last command.");
                    }
                }
            }
        }

        public void chat(string messageIn)
        {
            if (lastReceivedHomeSeqNum == homeSequenceNumber)
            {
                sendPacket(packet_Chat(messageIn), ipOfLastPacket.Address.ToString());
                acknowledged = false;
            }
        }

        public void move()
        {
            if (lastReceivedHomeSeqNum == homeSequenceNumber)
            {
                sendPacket(packet_MakeMove(), ipOfLastPacket.Address.ToString());
                acknowledged = false;
            }
        }

        public string getChatMessage()
        {
            string message = "";

            newMessage = false;
            message = currentMessage;

            return message;
        }

        public bool newMessageAvailable()
        {
            return newMessage;
        }

        //BACKGROUND functions

        private void sendAckOnTime(object source, ElapsedEventArgs e)
        {
            sendPacket_altPort(packet_Ack(), ipOfLastPacket.Address.ToString());
        }

        private byte[] receivePacket_blocking()
        {
            byte[] data = new byte[256];

            data = listener.Receive(ref ipOfLastPacket);

            return data;
        }

        private byte[] receivePacket_blockWithTimeout()
        {
            byte[] data = new byte[256];

            listener.Client.ReceiveTimeout = CONNECTION_TIMEOUT;

            try
            {
                data = listener.Receive(ref ipOfLastPacket);
            }
            catch (Exception)
            {
                data = null;
            }

            listener.Client.ReceiveTimeout = 0;

            return data;
        }

        private byte[] receivePacket_blockWithTimeout_altPort()
        {
            byte[] data = new byte[256];

            listenerAlt.Client.ReceiveTimeout = CONNECTION_TIMEOUT;

            try
            {
                data = listenerAlt.Receive(ref ipOfLastPacket);
            }
            catch (Exception)
            {
                data = null;
            }

            listenerAlt.Client.ReceiveTimeout = 0;

            return data;
        }

        private byte[] receivePacket_noBlock()
        {
            byte[] data = new byte[256];

            data = listener.Receive(ref ipOfLastPacket);

            return data;
        }

        /******************************
         * Function: sendPacket
         * Description: Sends a packet 
         * destination ip and port
         *****************************/
        public void sendPacket(byte[] packetIn, String ipAddressIn)
        {
            sender.Send(packetIn, packetIn.Length, ipAddressIn, SERVER_PORT);
        }

        public void sendPacket_altPort(byte[] packetIn, String ipAddressIn)
        {
            sender.Send(packetIn, packetIn.Length, ipAddressIn, ALT_SERVER_PORT);
        }

        /******************************
         * Function: broadcastPacket
         * Description: broadcasts
         * a packet to all devices
         * on the network
         *****************************/
        public void broadcastPacket(byte[] packetIn)
        {
            sendPacket(packetIn, BROADCAST_IP);
        }


        //TIME TRIGGERED BACKGROUND functions



        //PACKETS

        private byte[] packet_OfferGame()
        {
            byte[] packet = new byte[33];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_OFFER_GAME;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

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

        private byte[] packet_FindServers()
        {
            byte[] packet = new byte[6];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_FIND_SERVER;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            return packet;
        }

        //note that the first element of gameIn is the ipAddress of the game
        private byte[] packet_JoinGame(string[] gameIn)
        {
            byte[] packet = new byte[33];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_JOIN_GAME;

            //sequence number
            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

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

        private byte[] packet_StartGame()
        {
            byte[] packet = new byte[6];

            homeSequenceNumber++;

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_START_GAME;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            return packet;
        }

        private byte[] packet_Disconnect()
        {
            byte[] packet = new byte[6];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_DISCONNECT;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            return packet;
        }

        private byte[] packet_MakeMove()
        {
            byte[] packet = new byte[11];

            homeSequenceNumber++;

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_MAKE_MOVE;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

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

        private byte[] packet_Chat(string messageIn)
        {
            byte[] packet = new byte[24];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            homeSequenceNumber++;

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_CHAT;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            //message length
            packet[6] = 0x00;
            packet[7] = 0x00;
            packet[8] = 0x00;
            packet[9] = 0x00;

            //message
            packet[10] = 0x00;
            packet[11] = 0x00;
            packet[12] = 0x00;
            packet[13] = 0x00;
            packet[14] = 0x00;
            packet[15] = 0x00;
            packet[16] = 0x00;
            packet[17] = 0x00;
            packet[18] = 0x00;
            packet[19] = 0x00;
            packet[20] = 0x00;
            packet[21] = 0x00;
            packet[22] = 0x00;
            packet[23] = 0x00;

            (encoding.GetBytes(messageIn)).CopyTo(packet, 10);

            return packet;
        }

        private byte[] packet_Ack()
        {
            byte[] packet = new byte[10];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_ACK;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);
            (BitConverter.GetBytes(awaySequenceNumber)).CopyTo(packet, 6);

            // Console.WriteLine("ACK.");

            return packet;
        }
    }

    class UdpState
    {
        public IPEndPoint ip;
        public UdpClient client;
    }
}