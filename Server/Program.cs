using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using NetworkManager;

namespace Server
{
    class Program
    {
        static TcpListener Listener;
        static bool TerminateServer = false;
        public static ClientHandler ClientManager = new ClientHandler();

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
                            ClientManager.broadcastMessage(new Message("Server", internalMessage));
                            break;
                        }
                    case "stop":
                        {
                            Console.WriteLine("Performing server termination...");
                            TerminateServer = true;
                            Listener.Stop();
                            ConnectionThread.Interrupt();
                            Console.WriteLine("Performing client termination...");
                            ClientManager.ServerExit();
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
                    ClientManager.AddClient(Client);
                    
                } catch(Exception)
                {
                }
            }
        }
        
        
    }
}
