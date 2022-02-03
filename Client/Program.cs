using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Client
{
    class Program
    {
        static TcpClient client;
        static void Main(string[] args)
        {
            Console.Title = "CHAT CLIENT - by zhiyan114";
            Console.OutputEncoding = Encoding.UTF8;
            
            Console.WriteLine("Select your username: ");
            string Username = Console.ReadLine();
            while (client == null)
            {
                Console.WriteLine("Type the server IP Connection (format: IP:PORT): ");
                string[] BindInfo = Console.ReadLine().Split(":");
                if(BindInfo.Length == 1)
                {
                    client = new TcpClient(BindInfo[0], 42069);
                    if (client.Connected) {
                        Console.Title = string.Format("Connected... ({0}:{1})",BindInfo[0],42069);
                        break; 
                    };
                    Console.WriteLine("Client is not able to connect, please try again");
                } if(BindInfo.Length == 2)
                {
                    client = new TcpClient(BindInfo[0], int.TryParse(BindInfo[1], out int BindPort) ? BindPort: 42069);
                    if (client.Connected) {
                        Console.Title = string.Format("Connected... ({0}:{1})", BindInfo[0], BindPort);
                        break;
                    };
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
                try
                {
                    string message = Console.ReadLine();
                    if (client.Connected)
                        NetworkEncoder.Encode(client.GetStream(), new Message(Username, message));
                    Console.WriteLine(string.Format("[{0}]: {1}", Username, message));
                } catch(IOException)
                {
                    Console.WriteLine("Server is current offline, please try reconnecting later...");
                    Console.ReadLine();
                    break;
                } catch(Exception ex)
                {
                    Console.WriteLine("An Error Occured: " + ex.Message);
                    Console.ReadLine();
                    break;
                }
            }
        }
        static void Console_Exit(object sender,EventArgs e)
        {

        }
        /// <summary>
        /// This function handles the packet from the server. Should be threaded.
        /// </summary>
        static void MessageReceiver()
        {
            while(true)
            {
                NetworkStream NetStream = client.GetStream();
                if (!NetStream.DataAvailable) {
                    Thread.Sleep(17);
                    continue;
                }
                Message MsgObj = NetworkEncoder.Decode(NetStream);
                Console.WriteLine(string.Format("[{0}]: {1}", MsgObj.Name, MsgObj.Content));
            }
        }
    }
}
