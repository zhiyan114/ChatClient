using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkManager;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;

namespace NETMGR_Test
{
    [TestClass]
    public class NetworkCommunication
    {
        private static TcpClient Client; // Client's perspective
        private static TcpClient Server; // Server's perspective
        [AssemblyInitialize]
        public static void SetupEnvironment(TestContext context)
        {
            // Setup Real Network in a local environment (127.0.0.1)
            TcpListener ServerListen = new TcpListener(IPAddress.Loopback, 12345);
            ServerListen.Start();
            // Connect the client
            Client = new TcpClient(IPAddress.Loopback.ToString(), 12345);
            Server = ServerListen.AcceptTcpClient();
            // Stop the server
            ServerListen.Stop();
            if (Client == null || Server == null) throw new Exception("Server or Client was not established");
        }
        [TestMethod]
        public void Client_To_Server_Message()
        {
            // Generate a random 5-20 characters long name with message that are 1-3000 length long.
            Message RandomMessage = new Message(RandomTool.String(RandomTool.Int(5,20)),RandomTool.String(RandomTool.Int(1,3000)));
            // Send Message from Client to Server
            NetEncoder.Encode(Client.GetStream(), new NetworkMessage(MessageType.ChatMessage, RandomMessage));
            // Receive the message from Client.
            NetworkMessage ReceivedNetMessage_Server = NetEncoder.Decode(Server.GetStream());
            // Do Assert Cheks
            Message ReceivedMessage = (Message)ReceivedNetMessage_Server.Data;
            Assert.AreEqual(Encoding.UTF8.GetString(RandomMessage.MessageIdentifier), Encoding.UTF8.GetString(ReceivedMessage.MessageIdentifier), "Received Message Identifier is not matched");
            Assert.AreEqual(RandomMessage.Name, ReceivedMessage.Name, "Received Message Name is not matched");
            Assert.AreEqual(RandomMessage.Content, ReceivedMessage.Content, "Received Message Content is not matched");
        }
        [TestMethod]
        public void Server_To_Client_Message()
        {
            // Generate a random message that are 1-3000 length long.
            Message RandomMessage = new Message("Server", RandomTool.String(RandomTool.Int(1, 3000)));
            // Send Message from Client to Server
            NetEncoder.Encode(Server.GetStream(), new NetworkMessage(MessageType.ChatMessage, RandomMessage));
            // Receive the message from Client.
            NetworkMessage ReceivedNetMessage_Server = NetEncoder.Decode(Client.GetStream());
            // Do Assert Cheks
            Message ReceivedMessage = (Message)ReceivedNetMessage_Server.Data;
            Assert.AreEqual(Encoding.UTF8.GetString(RandomMessage.MessageIdentifier), Encoding.UTF8.GetString(ReceivedMessage.MessageIdentifier), "Received Message Identifier is not matched");
            Assert.AreEqual(RandomMessage.Name, ReceivedMessage.Name, "Received Message Name is not matched");
            Assert.AreEqual(RandomMessage.Content, ReceivedMessage.Content, "Received Message Content is not matched");
        }
    }
}
