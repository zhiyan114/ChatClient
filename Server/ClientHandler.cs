using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using NetworkManager;

namespace Server
{
    class ClientHandler
    {
        public ClientHandler() { } // Empty Init because this is only a helper class
        public List<TcpClient> Clients = new List<TcpClient>();
        private protected Dictionary<TcpClient, Thread> ClientThreads = new Dictionary<TcpClient, Thread>();
        public void AddClient(TcpClient client)
        {
            Clients.Add(client);
            // Thread handling for clients
            Thread CliThread = new Thread(new ThreadStart(()=> { ClientMessageHandler(client); }));
            ClientThreads.Add(client, CliThread);
            CliThread.Start();
        }
        /// <summary>
        /// Simplified function to disconnect client and clean up.
        /// </summary>
        /// <param name="client"></param>
        public void disconnectClient(TcpClient client)
        {
            ClientThreads[client].Interrupt();
            //ClientThreads[client].Join();
            ClientThreads.Remove(client);
            client.Close();
            Clients.Remove(client);
            Console.WriteLine("Client Disconnected...");
        }
        public void broadcastMessage(Message messageObject, TcpClient clientIgnore = null)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                TcpClient cli = Clients[i];
                if (clientIgnore == cli) continue;
                try
                {
                    NetEncoder.Encode(cli.GetStream(), new NetworkMessage(MessageType.ChatMessage, messageObject));
                }
                catch (IOException)
                {
                    // Stream cannot be written therefore the client is disconnected
                    disconnectClient(cli);
                    i--;
                }
            }
        }
        public void sendMessage(TcpClient client, Message messageObject)
        {
            try
            {
                NetEncoder.Encode(client.GetStream(), new NetworkMessage(MessageType.ChatMessage, messageObject));
            } catch(IOException)
            {
                disconnectClient(client);
            }
        }
        /// <summary>
        /// This function allows all the client to gracefully exit
        /// </summary>
        public void ServerExit()
        {
            foreach (TcpClient cli in Clients)
            {
                cli.Close();
            }
            foreach (KeyValuePair<TcpClient, Thread> cliThread in ClientThreads)
            {
                cliThread.Value.Interrupt();
            }
        }
        /*
         * Internal Functions Below, do not expose to public.
         */
        // This function checks for valid username, message, and relay it to all the other clients
        private protected void ClientMessageHandler(TcpClient client)
        {
            while (client.Connected)
            {
                NetworkStream NetStream = client.GetStream();
                // Receives the message and relay it
                if (!NetStream.DataAvailable)
                {
                    Thread.Sleep(17);
                    continue;
                }
                NetworkMessage NetMsg = NetEncoder.Decode(NetStream);

                
                
                switch(NetMsg.Type)
                {
                    case MessageType.ChatMessage:
                        Message MsgObj = (Message)NetMsg.Data;
                        if (!FilterSys.Username(MsgObj.Name))
                        {
                            sendMessage(client, new Message("disconnect", "Invalid username detected, please make sure the username only contains letters and numbers and that it doesn't contain any blacklisted word."));
                            disconnectClient(client);
                            Console.WriteLine("Client failed username requirement, will now be disconnected...");
                            continue;
                        }
                        if (!FilterSys.Message(MsgObj.Content))
                        {
                            string HexMsgID = Convert.ToHexString(MsgObj.MessageIdentifier);
                            sendMessage(client, new Message("SYSTEM", "The message has been moderated, please make sure your message doesn't contain blacklisted words or encoding and that the total character is under 3000. " + string.Format("(Message ID: {0})", HexMsgID)));
                            Console.WriteLine("Following Message has been moderated: " + string.Format("[{0}]: {1} (Message Hash ID: {2})", MsgObj.Name, MsgObj.Content, HexMsgID));
                            continue;
                        }
                        Console.WriteLine(string.Format("[{0}]: {1}", MsgObj.Name, MsgObj.Content));
                        broadcastMessage(MsgObj, client);
                        break;
                    case MessageType.Command:
                        Command CmdObj = (Command)NetMsg.Data;
                        incomingCommandHandler(client, CmdObj.Type, CmdObj.Args);
                        break;
                    default:
                        // Probably some toxic client who is sending bad packets
                        disconnectClient(client);
                        break;
                }
            }
        }
        /// <summary>
        /// Command Handler
        /// </summary>
        /// <param name="CommandType">string version of the command</param>
        /// <param name="MetaData">Additional Data to supply through (using JSON is preferred)</param>
        private protected void incomingCommandHandler(TcpClient ReqCli, CommandType CmdType, object[] CmdArgs)
        {
            switch (CmdType)
            {
                case CommandType.Shutdown:
                    {
                        disconnectClient(ReqCli);
                        return;
                    }
            }
        }
    }
}
