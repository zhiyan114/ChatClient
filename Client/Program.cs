using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using NetworkManager;

namespace Client
{
    class Program
    {
        static ServerHandler ClientManager = new ServerHandler();
        static void Main(string[] args)
        {
            Console.Title = "CHAT CLIENT - by zhiyan114";
            Console.OutputEncoding = Encoding.UTF8;
            
            Console.WriteLine("Select your username: ");
            string Username = Console.ReadLine();
            while (!ClientManager.Server.Connected)
            {
                Console.WriteLine("Type the server IP Connection (format: IP:PORT): ");
                string UserInputIP = Console.ReadLine();
                string[] BindInfo = string.IsNullOrWhiteSpace(UserInputIP) ? Array.Empty<string>() : UserInputIP.Split(":");
                IPAddress Addr = IPAddress.Any;
                int Port = 42069;

                if (BindInfo.Length == 1 && IPAddress.TryParse(BindInfo[0], out IPAddress AddrBind))
                {
                    Addr = AddrBind;
                } else if(BindInfo.Length == 2 && IPAddress.TryParse(BindInfo[0], out IPAddress AddrBindb) && int.TryParse(BindInfo[1], out int BindPort))
                {
                    Addr = AddrBindb;
                    Port = BindPort;
                } else
                {
                    // Do a domain name to validation before calling it invalid
                    try
                    {
                        IPAddress[] ResolvedIP;
                        if (BindInfo.Length > 0 && (ResolvedIP = Dns.GetHostAddresses(BindInfo[0])).Length > 0)
                        {
                            foreach(IPAddress IP in ResolvedIP)
                            {
                                // Get the first available IPv4 because IPv6 is not supported yet
                                if(IP.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    Addr = IP;
                                    break;
                                }
                            }
                        } else
                        {
                            Console.WriteLine("Invalid Server IP/PORT");
                            continue;
                        }
                    } catch(SocketException)
                    {
                        Console.WriteLine("Invalid Server IP/PORT");
                        continue;
                    }
                    
                }
                    
                if (ClientManager.tryConnect(Addr,Port))
                {
                    Console.Title = string.Format("Connected... ({0}:{1})", BindInfo[0], 42069);
                    break;
                }
                else
                    Console.WriteLine("Server Connection failed, please try again...");
            }
            Console.Clear();
            Console.WriteLine("Client Setup Completed, type a message to send or wait to receive the message...");
            while(true)
            {
                string message = Console.ReadLine();
                if (!ClientManager.sendMessage(new Message(Username, message)))
                {
                    // Server is unavailable
                    Console.WriteLine("Server has been disconnected...");
                    Console.ReadLine();
                    break;
                }
                Console.WriteLine(string.Format("[{0}]: {1}", Username, message));
            }
        }
    }
}
