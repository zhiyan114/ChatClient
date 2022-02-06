using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetworkManager;

namespace Client
{
    class ServerHandler
    {
        public TcpClient Server = new TcpClient();
        private Thread ClientThread;
        public ServerHandler()
        {
            ClientThread = new Thread(new ThreadStart(MessageReceiver));
        }
        
        public bool sendMessage(Message messageObject)
        {
            if (!Server.Connected) return false;
            try
            {
                Encoder.Encode(Server.GetStream(), new NetworkMessage(MessageType.ChatMessage, messageObject));
                return true;
            } catch(IOException)
            {
                disconnect();
                return false;
            } catch(Exception ex)
            {
                Console.WriteLine("Exception Found: " + ex.Message);
                disconnect();
                return false;
            }
        }
        public bool tryConnect(IPAddress addr, int port)
        {
            try
            {
                Server.Connect(addr, port);
                ClientThread.Start();
                return true;
            } catch(SocketException)
            {
                return false;
            }
        }
        public void disconnect()
        {
            if (!Server.Connected) return;
            Console.WriteLine("Client has been disconnected from the server...");
            ClientThread.Interrupt();
            Server.Close();
            Server = new TcpClient();
        }
        /*
         * Internal Functions Below
         */

        /// <summary>
        /// This function handles the packet from the server. Should be threaded.
        /// </summary>
        private protected void MessageReceiver()
        {
            while (true)
            {
                try
                {
                    NetworkStream NetStream = Server.GetStream();
                    if (!NetStream.DataAvailable)
                    {
                        Thread.Sleep(17);
                        continue;
                    }
                    Message MsgObj = (Message)Encoder.Decode(NetStream).Data;
                    Console.WriteLine(string.Format("[{0}]: {1}", MsgObj.Name, MsgObj.Content));
                } catch(InvalidOperationException)
                {
                }
            }
        }
    }
}
