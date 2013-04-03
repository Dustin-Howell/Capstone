using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Timers;
using Creeper;
using System.ComponentModel;
using Caliburn.Micro;
using CreeperMessages;

namespace CreeperNetwork
{
    public class Network : IHandle<NetworkErrorMessage>, IHandle<MoveMessage>, IHandle<ChatMessage>, IHandle<StartGameMessage>
    {
        //Network Constants
        public const int PROTOCOL_VERSION = 1;
        public const int SERVER_PORT = 1875;
        public const int ALT_SERVER_PORT = 1876;
        public const int ACKNOWLEDGEMENT_TIMER = 100;
        public const int PACKET_LOSS = 1000;
        public const int CONNECTION_TIMEOUT = 1000;
        public const int UNPLUGGED_INTERVAL = 1000;

        private const string BROADCAST_IP = "255.255.255.255";
        private const int MAX_PACKET_SIZE = 1024;
        private const byte PACKET_SIGNATURE = 0xC0;
        private const byte CMD_FIND_SERVER = 0x01;
        private const byte CMD_OFFER_GAME = 0x02;
        private const byte CMD_JOIN_GAME = 0x03;
        private const byte CMD_START_GAME = 0x04;
        private const byte CMD_DISCONNECT = 0x05;
        private const byte CMD_MAKE_MOVE = 0x06;
        private const byte CMD_CHAT = 0x07;
        private const byte CMD_ACK = 0x08;

        private const byte MOVETYPE_MOVE = 1;
        private const byte MOVETYPE_FORFEIT = 2;
        private const byte MOVETYPE_ILLEGAL = 3;

        //Network Variables
        private bool isServer = false;
        private bool serverFull = false;
        private bool hostGame = false;
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
        private Move currentMove;
        private bool newMove = false;
        private bool connectionIssue = false;
        private bool cableUnplugged = false;

        private byte[] lastCommand;

        private BackgroundWorker _keepAliveWorker;

        //Game variables
        private string hostGameName = "";
        private string hostPlayerName = "";
        private string clientPlayerName = "";
         
        //Class variables
        private static int instanceCount = 0;
        private IEventAggregator _eventAggregator;

        /**********************************************************
         * Constructor: Network
         * Parameters:  eventAggregator -- allows us to check how 
         *              many instances of the game have been made
         * Description: Constructor for Network class -- starts 
         *              the keep alive functionality and unplugged 
         *              network cable functionality
         *********************************************************/
        public Network(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            if (++instanceCount > 1) throw new InvalidOperationException();

            _keepAliveWorker = new BackgroundWorker();
            _keepAliveWorker.DoWork += new DoWorkEventHandler((s, e) => keepAlive());

            //This should be here -- if any problems checking unplugged cable, this is source
            checkUnpluggedNetwork();
        }

        //###############################################################
        //#PUBLIC SERVER FUNCTIONS                                      #
        //###############################################################

       /**********************************************************
        * Function:    server_hostGame
        * Parameters:  gameNameIn - name of the hosted game
        *              playerNameIn - name of the host player
        * Description: Hosts a game on the server -- continually
        *              looks for checking if someone is looking 
        *              for servers or trying to join the server
        *********************************************************/
        public void server_hostGame(string gameNameIn, string playerNameIn)
        {
            isServer = true;
            hostGameName = gameNameIn;
            hostPlayerName = playerNameIn;

            byte[] packet = new byte[MAX_PACKET_SIZE];

            hostGame = true;

            //okay, now what if someone wants to join?.
            while (!serverFull  && hostGame)
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

        public void quitHostGame()
        {
            hostGame = false;
        }

        /**********************************************************
         * Function:    server_acceptClient
         * Parameters:  packetIn -- the join game packet
         * Returns:     Whether the server accepted the client
         *              or not
         * Description: Checks whether a client is valid and 
         *              determines whether they're accepted or
         *              not
         *********************************************************/
        public bool server_acceptClient(byte[] packetIn)
        {
            bool accepted = false;
            int clientPlayerNameLength = BitConverter.ToInt32(packetIn, 11 + hostGameName.Length);

            //valid game..for now, just accept all.
            if (!serverFull)
            {
                serverFull = true;
                accepted = true;
                clientPlayerName = BitConverter.ToString(packetIn, 11 + hostGameName.Length + 4, clientPlayerNameLength);
            }

            return accepted;
        }

        /**********************************************************
         * Function:    server_startGame
         * Description: Starts the game on the server -- at 
         *              this point, the game is playable. Waits 
         *              for client to acknowledge. 
         *********************************************************/
        public void server_startGame()
        {
            sendPacket(packet_StartGame(), ipOfLastPacket.Address.ToString());

            Console.WriteLine("GAME STARTED.");
            gameRunning = true;

            _keepAliveWorker.RunWorkerAsync();

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

        //###############################################################
        //#PUBLIC CLIENT FUNCTIONS                                      #
        //###############################################################

        /**********************************************************
         * Function:    client_findGames
         * Parameters:  clientNameIn -- name of the client player
         * Returns:     A list of the games found
         * Description: Finds all games currently on the network
         *********************************************************/
        public string[,] client_findGames(string clientNameIn)
        {
            string[,] games = new string[256, 7];
            int gameCounter = 0;
            Stopwatch serverStopwatch = new Stopwatch();

            clientPlayerName = clientNameIn;

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
                    int gameNameLength = BitConverter.ToInt32(packet, 7);
                    int playerNameLength = BitConverter.ToInt32(packet, 11 + gameNameLength);

                    //Server IP
                    games[gameCounter, 0] = ipOfLastPacket.Address.ToString();

                    //Protocol Version
                    games[gameCounter, 1] = packet[6].ToString();

                    //Game Name Length
                    games[gameCounter, 2] = gameNameLength.ToString();
                   
                    //Game Name
                    games[gameCounter, 3] = Encoding.ASCII.GetString(packet, 11, gameNameLength);
                    
                    //Player Name Length
                    games[gameCounter, 4] = BitConverter.ToInt32(packet, 11 + gameNameLength).ToString();

                    //Player Name
                    games[gameCounter, 5] = Encoding.ASCII.GetString(packet, 11 + gameNameLength + 4, playerNameLength);
                    
                    //Who moves first?
                    games[gameCounter, 6] = packet[packet.Length - 1].ToString();

                    gameCounter++;
                }
            }

            serverStopwatch.Stop();

            return games;
        }

        /**********************************************************
         * Function:    client_joinGame
         * Parameters:  gameIn -- the game to join
         * Description: Joins a game on a server
         *********************************************************/
        public void client_joinGame(string[] gameIn)
        {
            sendPacket(packet_JoinGame(gameIn), gameIn[0]);
        }

        /**********************************************************
         * Function:    client_ackStartGame
         * Description: Starts a game on the client by 
         *              acknowledging the server
         *********************************************************/
        public void client_ackStartGame()
        {
            byte[] packet = new byte[MAX_PACKET_SIZE];
            Boolean commandReceived = false;

            while (!commandReceived)
            {
                packet = receivePacket_blocking();

                if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_START_GAME)
                {
                    awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                    sendPacket(packet_Ack(), ipOfLastPacket.Address.ToString());
                    commandReceived = true;
                    acknowledged = true;

                    Console.WriteLine("GAME STARTED.");
                    gameRunning = true;

                    _keepAliveWorker.RunWorkerAsync();
                }
                else if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_DISCONNECT)
                {
                    Console.WriteLine("DISCONNECTED BY SERVER.");
                    commandReceived = true;
                }
            }
        }

        //###############################################################
        //#PUBLIC GAME FUNCTIONS                                        #
        //###############################################################

        /**********************************************************
         * Function:    runGame
         * Description: Runs the playable portion of the game 
         *              on a separate threading. 
         *********************************************************/
        public void runGame()
        {
            BackgroundWorker networkPlayGame;

            networkPlayGame = new BackgroundWorker();
            networkPlayGame.DoWork += new DoWorkEventHandler((s, e) => playGame());

            networkPlayGame.RunWorkerAsync();
        }

        /**********************************************************
         * Function:    disconnect
         * Description: closes the network game
         *********************************************************/
        public void disconnect()
        {
            sendPacket(packet_Disconnect(), ipOfLastPacket.Address.ToString());
            gameRunning = false;
            listener.Close();
            listenerAlt.Close();
            sender.Close();
        }

        /**********************************************************
         * Function:    chat
         * Parameters:  messageIn -- the message to send
         * Description: Sends a chat message to the other player
         *********************************************************/
        public void chat(string messageIn)
        {
            if (lastReceivedHomeSeqNum == homeSequenceNumber)
            {
                sendPacket(packet_Chat(messageIn), ipOfLastPacket.Address.ToString());
                acknowledged = false;
            }
        }

        /**********************************************************
         * Function:    chat
         * Parameters:  moveIn -- the move to send
         * Description: Sends a move to the other player
         *********************************************************/
        public void move(Move moveIn)
        {
            if (lastReceivedHomeSeqNum == homeSequenceNumber)
            {
                Console.WriteLine("Move Sent.");
                sendPacket(packet_MakeMove(moveIn, MOVETYPE_MOVE), ipOfLastPacket.Address.ToString());
                acknowledged = false;
            }
        }

        /**********************************************************
         * Function:    forfeit
         * Description: forfeits the network game
         *********************************************************/
        public void forfeit()
        {
            if (lastReceivedHomeSeqNum == homeSequenceNumber)
            {
                sendPacket(packet_MakeMove(new Move(new Position(0, 0), new Position(0, 0), CreeperColor.Empty), 
                    MOVETYPE_FORFEIT), ipOfLastPacket.Address.ToString());
                acknowledged = false;
            }
        }

        /**********************************************************
         * Function:    illegalMove
         * Description: Stops the network game because of an 
         *              illegal move
         *********************************************************/
        public void illegalMove()
        {
            if (lastReceivedHomeSeqNum == homeSequenceNumber)
            {
                sendPacket(packet_MakeMove(new Move(new Position(0, 0), new Position(0, 0), CreeperColor.Empty),
                    MOVETYPE_ILLEGAL), ipOfLastPacket.Address.ToString());
                acknowledged = false;
            }
        }

        //###############################################################
        //#UTILITY FUNCTIONS                                            #
        //###############################################################

        /**********************************************************
         * Function:    checkUnpluggedNetwork
         * Description: Checks if the network cable is unplugged
         *********************************************************/
        public void checkUnpluggedNetwork()
        {
            Timer checkTimer = new Timer();
            checkTimer.Elapsed += new ElapsedEventHandler(checkUnpluggedNetworkOnTime);
            checkTimer.Interval = UNPLUGGED_INTERVAL;
            checkTimer.Enabled = true;
            checkTimer.AutoReset = true;
        }

        //###############################################################
        //#PRIVATE BACKGROUND FUNCTIONS                                 #
        //###############################################################

        /**********************************************************
         * Function:    playGame
         * Description: the game loop for the network portion of
         *              the game. Listens for all the packets
         *              and responds appropriately
         *********************************************************/
        private void playGame()
        {
            byte[] packet = new byte[MAX_PACKET_SIZE];

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
                        int messageLength = BitConverter.ToInt32(packet, 6);
                        currentMessage = Encoding.ASCII.GetString(packet, 10, messageLength);
                        awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                        newMessage = true;
                        acknowledged = true;
                        sendPacket(packet_Ack(), ipOfLastPacket.Address.ToString());

                        _eventAggregator.Publish(new ChatMessage(currentMessage, ChatMessageType.Receive));

                    }
                    else if (packet[1] == CMD_MAKE_MOVE)
                    {
                        awaySequenceNumber = BitConverter.ToInt32(packet, 2);
                        sendPacket(packet_Ack(), ipOfLastPacket.Address.ToString());
                        acknowledged = true;

                        if (packet[6] == MOVETYPE_MOVE)
                        {
                            currentMove = new Move(new Position(packet[7], packet[8]), new Position(packet[9], packet[10]), CreeperColor.Empty);
                            //newMove = true;

                            _eventAggregator.Publish(new MoveMessage(){ PlayerType = PlayerType.Network, Type = MoveMessageType.Response,  Move = currentMove});
                        }
                        else if (packet[6] == MOVETYPE_FORFEIT || packet[6] == MOVETYPE_ILLEGAL)
                        {
                            if (packet[6] == MOVETYPE_FORFEIT)
                                _eventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.Forfeit));
                            //NetworkGameOver(this, new EndGameEventArgs(END_GAME_TYPE.FORFEIT));
                            else if (packet[6] == MOVETYPE_ILLEGAL)
                                _eventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.IllegalMove));
                            //NetworkGameOver(this, new EndGameEventArgs(END_GAME_TYPE.ILLEGAL_MOVE));

                            sendPacket(packet_Disconnect(), ipOfLastPacket.Address.ToString());
                        }
                    }
                    else if (packet[1] == CMD_JOIN_GAME)
                    {
                        sendPacket(packet_Disconnect(), ipOfLastPacket.Address.ToString());
                    }

                }
            }
        }

        /**********************************************************
         * Function:    isNetworkConnected
         * Returns:     Whether or not there is a network available
         * Description: Checks if a network is available or not
         *********************************************************/
        private bool isNetworkConnected()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        /**********************************************************
         * Function:    checkUnpluggedNetworkOnTime
         * Parameters:  source -- the source object
         *              e      -- the source event
         * Description: Checks if the network cable is unplugged
         *********************************************************/
        private void checkUnpluggedNetworkOnTime(object source, ElapsedEventArgs e)
        {
            if (cableUnplugged && isNetworkConnected())
            {
                cableUnplugged = false;

                //if (ConnectionIssue != null)
                //{
                //    ConnectionIssue(this, new ConnectionEventArgs(CONNECTION_ERROR_TYPE.CABLE_RECONNECTED));
                //}
                _eventAggregator.Publish(new ConnectionStatusMessage(CONNECTION_ERROR_TYPE.CABLE_RECONNECTED));

                Console.WriteLine("Cable reconnected. Connection OK.");
            }
            else if (!isNetworkConnected())
            {
                //if (ConnectionIssue != null)
                //{
                //    ConnectionIssue(this, new ConnectionEventArgs(CONNECTION_ERROR_TYPE.CABLE_UNPLUGGED));
                //}
                _eventAggregator.Publish(new ConnectionStatusMessage(CONNECTION_ERROR_TYPE.CABLE_UNPLUGGED));

                cableUnplugged = true;

                Console.WriteLine("A network cable is unplugged.");
            }
        }

        /**********************************************************
         * Function:    keepAlive
         * Description: Keeps the network connection open
         *********************************************************/
        private void keepAlive()
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
                    Console.WriteLine("Lost connection. Attemping reconnect.");

                    //if (ConnectionIssue != null)
                    //{
                    //    ConnectionIssue(this, new ConnectionEventArgs(CONNECTION_ERROR_TYPE.CONNECTION_LOST));
                    //}
                    _eventAggregator.Publish(new ConnectionStatusMessage(CONNECTION_ERROR_TYPE.CONNECTION_LOST));

                    connectionIssue = true;
                }
                else if (packet[0] == PACKET_SIGNATURE && packet[1] == CMD_ACK)
                {
                    if (homeSequenceNumber == BitConverter.ToInt32(packet, 6))
                    {
                        stopwatch.Restart();
                    }

                    if (stopwatch.ElapsedMilliseconds > PACKET_LOSS)
                    {
                        sendPacket(lastCommand, ipOfLastPacket.Address.ToString());
                        Console.WriteLine("Packet lost. Resent the last command to " + ipOfLastPacket.Address.ToString());
                    }
                }

                if (packet != null && connectionIssue)
                {
                    connectionIssue = false;

                    Console.WriteLine("Connection Reestablished.");

                    //if (ConnectionIssue != null)
                    //{
                    //    ConnectionIssue(this, new ConnectionEventArgs(CONNECTION_ERROR_TYPE.RECONNECTED));
                    //}
                    _eventAggregator.Publish(new ConnectionStatusMessage(CONNECTION_ERROR_TYPE.RECONNECTED));
                }
            }
        }

        /**********************************************************
         * Function:    sendAckOnTime
         * Parameters:  source -- the source object
         *              e -- the source event
         * Description: Sends an acknowledgement to the other 
         *              player machine
         *********************************************************/
        private void sendAckOnTime(object source, ElapsedEventArgs e)
        {
           sendPacket_altPort(packet_Ack(), ipOfLastPacket.Address.ToString());
        }

        /**********************************************************
         * Function:    receivePacket_blocking
         * Returns:     The packet received
         * Description: Waits for a packet -- blocks all other
         *              processes on thread
         *********************************************************/
        private byte[] receivePacket_blocking()
        {
            byte[] data = new byte[256];

            //Cannot access a disposed object.
            //Object name: 'System.Net.Sockets.UdpClient'.
            //this happens to the host of a networked game
            data = listener.Receive(ref ipOfLastPacket);

            return data;
        }

        /**********************************************************
         * Function:    receivePacket_blockWithTimeout
         * Returns:     The packet received
         * Description: Waits for the packet -- blocks all 
         *              other processes on thread. Exits on
         *              timeout
         *********************************************************/
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

        /**********************************************************
         * Function:    receivePacket_blockWithTimeout_altPort
         * Returns:     the packet received
         * Description: Waits for the packet on alternate port
         *              -- blocks all other processes on thread.
         *              Exits on timeout.
         *********************************************************/
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

        /**********************************************************
         * Function:    sendPacket
         * Parameters:  packetIn -- the packet to send
         *              ipAddressIn -- the destination IP
         * Description: Sends a packet to destination IP
         *********************************************************/
        private void sendPacket(byte[] packetIn, String ipAddressIn)
        {
            sender.Send(packetIn, packetIn.Length, ipAddressIn, SERVER_PORT);
        }

        /**********************************************************
         * Function:    sendPacket_altPort
         * Parameters:  packetIn -- the packet to send
         *              ipAddressIn -- the destination IP
         * Description: Sends a packet to destination IP on 
         *              alternate port
         *********************************************************/
        private void sendPacket_altPort(byte[] packetIn, String ipAddressIn)
        {
            sender.Send(packetIn, packetIn.Length, ipAddressIn, ALT_SERVER_PORT);
        }

        /**********************************************************
         * Function:    broadcastPacket
         * Parameters:  packetIn -- packet to broadcast
         * Description: Broadcasts a packet to all devices on 
         *              the network
         *********************************************************/
        private void broadcastPacket(byte[] packetIn)
        {
           sendPacket(packetIn, BROADCAST_IP);
        }

        //###############################################################
        //#PACKET FUNCTIONS                                             #
        //###############################################################

        /******************************
        * Function: packet_OfferGame
        * Description: Constructs the 
        * packet for the OfferGame
        * packet
        ******************************/
        private byte[] packet_OfferGame()
        {
            byte[] packet = new byte[16 + hostGameName.Length + hostPlayerName.Length];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_OFFER_GAME;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            packet[6] = PROTOCOL_VERSION;

            //Game Name Length
            (BitConverter.GetBytes(hostGameName.Length)).CopyTo(packet, 7);

            //Game Name 
            encoding.GetBytes(hostGameName).CopyTo(packet, 11);

            //Player Name Length
            (BitConverter.GetBytes(hostPlayerName.Length)).CopyTo(packet, 11 + hostGameName.Length);

            //Player Name
            encoding.GetBytes(hostPlayerName).CopyTo(packet, 11 + hostGameName.Length + 4);

            //who moves first? 1 I do, 0 you do
            packet[packet.Length - 1] = Convert.ToByte(1);

            return packet;
        }

        /******************************
        * Function: packet_FindServers
        * Description: Constructs the 
        * packet for the FindServers
        * packet
        ******************************/
        private byte[] packet_FindServers()
        {
            byte[] packet = new byte[6];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_FIND_SERVER;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            return packet;
        }

        /******************************
        * Function: packet_JoinGame
        * Description: Constructs the 
        * packet for the JoinGame
        * packet
        ******************************/
        private byte[] packet_JoinGame(string[] gameIn)
        {
            byte[] packet = new byte[16 + Convert.ToInt32(gameIn[2]) + clientPlayerName.Length];
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
            (encoding.GetBytes(gameIn[3])).CopyTo(packet, 11);
            hostGameName = gameIn[3];

            //player name length 
            (BitConverter.GetBytes(clientPlayerName.Length)).CopyTo(packet, 11 + hostGameName.Length);

            //player name 
            (encoding.GetBytes(clientPlayerName)).CopyTo(packet, 11 + hostGameName.Length + 4);
            
            //who moves first? 1 I do, 0 you do
            packet[packet.Length - 1] = Convert.ToByte(!Convert.ToBoolean(Convert.ToByte(gameIn[6])));

            return packet;
        }

        /******************************
        * Function: packet_StartGame
        * Description: Constructs the 
        * packet for the StartGame
        * packet
        ******************************/
        private byte[] packet_StartGame()
        {
            byte[] packet = new byte[6];

            homeSequenceNumber++;

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_START_GAME;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            lastCommand = packet;

            return packet;
        }

        /******************************
        * Function: packet_Disconnect
        * Description: Constructs the 
        * packet for the 
        * Disconnect packet
        ******************************/
        private byte[] packet_Disconnect()
        {
            byte[] packet = new byte[6];

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_DISCONNECT;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            return packet;
        }

        /******************************
        * Function: packet_MakeMove
        * Description: Constructs the 
        * packet for the MakeMove
        * packet
        ******************************/
        private byte[] packet_MakeMove(Move moveIn, byte moveTypeIn)
        {
            byte[] packet = new byte[11];

            homeSequenceNumber++;

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_MAKE_MOVE;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            //move type
            packet[6] = moveTypeIn;

            //source
            packet[7] = (byte)moveIn.StartPosition.Row;
            packet[8] = (byte)moveIn.StartPosition.Column;

            //destination
            packet[9] = (byte)moveIn.EndPosition.Row;
            packet[10] = (byte)moveIn.EndPosition.Column;

            lastCommand = packet;

            return packet;
        }

        /******************************
        * Function: packet_Chat
        * Description: Constructs the 
        * packet for the Chat packet
        ******************************/
        private byte[] packet_Chat(string messageIn)
        {
            byte[] packet = new byte[10 + messageIn.Length];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            homeSequenceNumber++;

            packet[0] = PACKET_SIGNATURE;
            packet[1] = CMD_CHAT;

            (BitConverter.GetBytes(homeSequenceNumber)).CopyTo(packet, 2);

            //Message length
            (BitConverter.GetBytes(messageIn.Length)).CopyTo(packet, 6);

            (encoding.GetBytes(messageIn)).CopyTo(packet, 10);

            lastCommand = packet;

            return packet;
        }

        /******************************
        * Function: packet_Ack
        * Description: Constructs the 
        * packet for the 
        * Acknowledgement packet
        ******************************/
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

        /******************************
        * Function: Handle
        * Overloaded: GameOverMessage
        * Description: Handles
        * GameOver messages
        ******************************/
        public void Handle(NetworkErrorMessage message)
        {
            disconnect();
        }

        /******************************
        * Function: Handle
        * Overloaded: GameOverMessage
        * Description: Handles
        * MoveRequest messages
        ******************************/
        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Request
                && message.PlayerType == PlayerType.Network)
            {
                //wait for move from opponent
            }
            else if (message.Type == MoveMessageType.Response
                && message.PlayerType != PlayerType.Network)
            {
                move(message.Move);
            }
        }

        public void Handle(ChatMessage message)
        {
            if (message.Type == ChatMessageType.Send)
            {
                chat(message.Message);
            }
        }

        public void Handle(StartGameMessage message)
        {
            if (message.Settings.Player1Type == PlayerType.Network
                || message.Settings.Player2Type == PlayerType.Network)
            {
                //we could handle the start of a networked game here, though I'm not sure what would go here (maybe a network settings object?)
            }
        }
    }
}



//Unused old functions

/******************************
* Class: UdpState
* Description: Necessary for 
* 
*******************************/
//class UdpState
// {
//     public IPEndPoint ip;
//      public UdpClient client;
//  }

//public string getChatMessage()
//{
//     newMessage = false;
//      return currentMessage;
//   }

//    public bool newMessageAvailable()
//     {
//         return newMessage;
//     }

//  public Move getNextMove()
//   {
//       newMove = false;
//       return currentMove;
//    }

//  public bool newMoveAvailable()
// {
//     return newMove;
//  }

//Events

//Convert to EventAggregator events
//public event EventHandler<MoveResponseMessage> MoveMade;
//public event EventHandler<ConnectionEventArgs> ConnectionIssue;
//public event EventHandler<ChatEventArgs> ChatMade;
//public event EventHandler<EndGameEventArgs> NetworkGameOver;