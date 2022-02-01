using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.IO;

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
                            Console.WriteLine("[Server]: " + cmdArgs[1]);
                            foreach(TcpClient cli in Clients)
                            {
                                NetworkEncoder.Encode(cli.GetStream(), new Message("Server", cmdArgs[1]));
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
                            Console.WriteLine(string.Format("[{0}]: {1}", MsgObj.Name, MsgObj.Content));
                            for(int i=0;i<Clients.Count;i++)
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
                                    cli.Close();
                                    Clients.Remove(cli);
                                    ClientThreads[cli].Interrupt();
                                    ClientThreads.Remove(cli);
                                    Console.WriteLine("Client Disconnected...");
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
    }
}
