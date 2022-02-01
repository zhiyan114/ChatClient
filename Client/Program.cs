using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace Client
{
    class Program
    {
        static TcpClient client;
        static void Main(string[] args)
        {
            Console.Title = "CHAT CLIENT - by zhiyan114";
            Console.WriteLine("Select your username: ");
            string Username = Console.ReadLine();
            while (client == null)
            {
                Console.WriteLine("Type the server IP Connection (format: IP:PORT): ");
                string[] BindInfo = Console.ReadLine().Split(":");
                if(BindInfo.Length == 2)
                {
                    client = new TcpClient(BindInfo[0], int.TryParse(BindInfo[1], out int BindPort) ? BindPort: 42069);
                    if(client.Connected) break;
                    Console.WriteLine("Client is not able to connect, please try again");
                } else
                    Console.WriteLine("Invalid Server IP/PORT");
            }
            Thread ClientThread = new Thread(new ThreadStart(MessageReceiver));
            ClientThread.Start();
            Console.Clear();
            Console.WriteLine("Client Setup Completed, type a message to send or wait to receive the message...");
            while(true)
            {
                NetworkEncoder.Encode(client.GetStream(), new Message(Username, Console.ReadLine()));
            }
        }
        /// <summary>
        /// This function handles the packet from the server. Should be threaded.
        /// </summary>
        static void MessageReceiver()
        {
            
        }
    }
}
