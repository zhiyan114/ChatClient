using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Server
{

    class Program
    {
        static TcpListener Listener;
        static List<TcpClient> Clients = new List<TcpClient>();
        static bool TerminateServer = false;


        static void Main(string[] args)
        {
            Console.Title = "CHAT SERVER - by zhiyan114";
            Console.OutputEncoding = Encoding.UTF8;
            TcpAssigner(args);
            Thread ConnectionThread = new Thread(new ThreadStart(ConnectionHandler));
            ConnectionThread.Start();
            Console.WriteLine("Connection Thread: Started...");
            while (!TerminateServer)
            {
                string[] cmdArgs = Console.ReadLine().Split(" ");
                switch (cmdArgs[0].ToLower())
                {
                    case "help":
                        {
                            Console.WriteLine("Commands List: ");
                            Console.WriteLine("help - Show this help menu");
                            Console.WriteLine("clear - Clear all the console message");
                            Console.WriteLine("message - Send all client a message (args: message)");
                            Console.WriteLine("stop - End the server");
                            break;
                        }
                    case "clear":
                        {
                            Console.Clear();
                            Console.WriteLine("Console Cleared");
                            break;
                        }
                    case "message":
                        {
                            if (cmdArgs.Length < 2)
                            {
                                Console.WriteLine("Invalid args supplied, minimum 1 required.");
                                break;
                            }
                            string internalMessage = "";
                            for (int i = 1; i < cmdArgs.Length; i++)
                                internalMessage += cmdArgs[i]+" ";
                            Console.WriteLine("[Server]: " + internalMessage);
                            for (int i = 0; i < Clients.Count; i++)
                            {
                                TcpClient cli = Clients[i];
                                try
                                {
                                    NetworkEncoder.Encode(cli.GetStream(), new Message("Server", internalMessage));
                                }
                                catch (IOException)
                                {
                                    // Stream cannot be written therefore the client is disconnected
                                    disconnectClient(cli);
                                    i--;
                                }
                            }
                            break;
                        }
                    case "stop":
                        {
                            Console.WriteLine("Performing server termination...");
                            TerminateServer = true;
                            Listener.Stop();
                            ConnectionThread.Interrupt();
                            ConnectionThread.Join();
                            Console.WriteLine("Performing client termination...");
                            foreach (TcpClient cli in Clients)
                            {
                                cli.Close();
                            }
                            foreach(KeyValuePair<TcpClient,Thread> cliThread in ClientThreads)
                            {
                                cliThread.Value.Interrupt();
                                cliThread.Value.Join();
                            }
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Invalid command supplied, type help for list of commands.");
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// A function for Main thread to keep the code tidy. Esentially handle the assignment for TcpListener.
        /// </summary>
        /// <param name="args"></param>
        private static void TcpAssigner(string[] args)
        {
            /*
             * Create a listener object using binding IP/PORT based on the supplied arg, otherwise use default
             * Default IP: 0.0.0.0 (ANY)
             * Port: 42069 ( 69420 doesn't work because the maximum range is 65535 (or 16-bit))
             */
            IPAddress BindAddr = IPAddress.Any;
            int BindPort = 42069;
            switch (args.Length)
            {
                case 1:
                    {
                        // Only the IP Field are supplied
                        if (IPAddress.TryParse(args[0], out IPAddress Addr))
                            BindAddr = Addr;
                        break;
                    }
                case 2:
                    {
                        // Both IP Field and the Port are supplied
                        if (IPAddress.TryParse(args[0], out IPAddress Addr))
                            BindAddr = Addr;
                        if (int.TryParse(args[1], out int Port))
                            BindPort = Port;
                        break;
                    }
            }
            Listener = new TcpListener(BindAddr, BindPort);
            Console.WriteLine("Server is now binded to IP: " + BindAddr + " and port: " + BindPort);
        }
        /// <summary>
        /// This function handles all the client's connection and their individual threads. This function SHOULD BE Threaded
        /// </summary>
        static private Dictionary<TcpClient, Thread> ClientThreads = new Dictionary<TcpClient, Thread>();
        private static void ConnectionHandler()
        {
            Listener.Start();
            while(!TerminateServer)
            {
                try
                {
                    TcpClient Client = Listener.AcceptTcpClient();
                    Console.WriteLine("Client Connected...");
                    Clients.Add(Client);
                    // Thread handling for clients
                    Thread CliThread = new Thread(new ThreadStart(() => {
                        while (!TerminateServer && Client.Connected)
                        {
                            NetworkStream NetStream = Client.GetStream();
                            // Receives the message and relay it
                            if (!NetStream.DataAvailable)
                            {
                                Thread.Sleep(17);
                                continue;
                            }   
                            Message MsgObj = NetworkEncoder.Decode(NetStream);
                            
                            if(!UsernameFilter(MsgObj.Name))
                            {
                                NetworkEncoder.Encode(Client.GetStream(), new Message("SYSTEM","Invalid username detected, please make sure the username only contains letters and numbers and that it doesn't contain any blacklisted word."));
                                disconnectClient(Client);
                                Console.WriteLine("Client failed username requirement, will now be disconnected...");
                                continue;
                            }
                            if(!MessageFilter(MsgObj.Content))
                            {
                                string HexMsgID = Convert.ToHexString(MsgObj.MessageIdentifier);
                                NetworkEncoder.Encode(Client.GetStream(), new Message("SYSTEM", "The message has been moderated, please make sure your message doesn't contain blacklisted words or encoding and that the total character is under 3000. "+string.Format("(Message ID: {0})",HexMsgID)));
                                Console.WriteLine("Following Message has been moderated: "+string.Format("[{0}]: {1} (Message Hash ID: {2})",MsgObj.Name,MsgObj.Content, HexMsgID));
                                continue;
                            }
                            Console.WriteLine(string.Format("[{0}]: {1}", MsgObj.Name, MsgObj.Content));
                            for (int i=0;i<Clients.Count;i++)
                            {
                                TcpClient cli = Clients[i];
                                if (cli == Client) continue;
                                try
                                {
                                    NetworkEncoder.Encode(cli.GetStream(), MsgObj);
                                }
                                catch (IOException)
                                {
                                    // Stream cannot be written therefore the client is disconnected
                                    disconnectClient(cli);
                                    i--;
                                }
                            }
                        }
                    }));
                    ClientThreads.Add(Client, CliThread);
                    CliThread.Start();
                } catch(Exception)
                {

                }
            }
        }
        /// <summary>
        /// Simplified function to disconnect client and clean up.
        /// </summary>
        /// <param name="client"></param>
        static void disconnectClient(TcpClient client)
        {
            client.Close();
            Clients.Remove(client);
            ClientThreads[client].Interrupt();
            ClientThreads.Remove(client);
            Console.WriteLine("Client Disconnected...");
        }
        /// <summary>
        /// Simple Username Filteration System
        /// </summary>
        static string[] BlacklistUsername = new string[]
        { 
            "server",
            "admin",
            "mod",
            "administrator",
            "moderator",
            "system",
            "test",
        };
        static bool UsernameFilter(string name)
        {
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9]+$")) return false; // Can only contain letter and numbers
            if (BlacklistUsername.Any(name.ToLower().Contains)) return false; // Check the blacklist
            return true;
        }
        /// <summary>
        /// Simple Message Filteration System
        /// </summary>
        static string[] BlacklistWord = new string[]
        {
            "nigger",
            "faggot",
        };
        static bool MessageFilter(string msg)
        {
            if (!Regex.IsMatch(msg, "^[a-zA-Z0-9_!@#$%^&*()-+={}[\\]<>:;<>,.\\?~\\\" ']+$")) return false; // RegExp check the sentence to ensure it doesn't have any weird character
            if (BlacklistWord.Any(msg.ToLower().Contains)) return false; // Ban some extreme words
            if (msg.Length > 3000) return false; // If character exceed 3000. *Not Byte/Bit*
            return true;
        }
    }
}
