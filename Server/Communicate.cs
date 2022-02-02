using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.Security.Cryptography;
using System.Net.Sockets;
/*
* Name: Communicate.cs
* Description: Standardized communication protocol between the server and the client
* Author: zhiyan114
*/

/*
 * Library Implementation:
 * - Name: protobuf-net (2.4.6)
 *  - Repository: https://github.com/protobuf-net/protobuf-net
 */
namespace Server
{
    /// <summary>
    /// This class defines the message structure that will be sent through the network
    /// </summary>
    [ProtoContract]
    class Message
    {
        [ProtoMember(1)]
        public byte[] MessageIdentifier { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public string Content { get; set; }
        public Message(string Name, string Content)
        {
            // Do the obvious by assigning all the objects to its property in the class
            this.Name = Name;
            this.Content = Content;
            /*
             * Use SHA512 (sounds better than SHA256 lol) to create a unique message indentifier
             * Hash is computed by using the both the supplied parameter Name and Content as well as the current unix timestamp. Supplied string/int will be converted into UTF8 byte standards.
             */
            using (SHA1 Hasher = new SHA1Managed())
                MessageIdentifier = Hasher.ComputeHash(Encoding.UTF8.GetBytes(Name + Content + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));
        }
        public Message() { } // Protobuf Support
    }
    /// <summary>
    /// This class defines the encoding structure that the message will be using
    /// </summary>
    static class NetworkEncoder
    {
        /// <summary>
        /// This function encodes Message Object into byte array that will be stream through NetStream
        /// </summary>
        /// <param name="NetStream">Network Stream</param>
        /// <param name="MsgObj">Message Object</param>
        public static void Encode(Stream NetStream, Message MsgObj)
        {
            // Message are all handled in plaintext because I can't be bother with encryption, especially on school's lockdown device where CA complaint will be hard to deal with...
            using (MemoryStream MemStream = new MemoryStream())
            {
                Serializer.Serialize(MemStream, MsgObj);
                NetStream.Write(MemStream.ToArray(), 0, MemStream.ToArray().Length);
            }
        }
        /// <summary>
        /// This function decodes the stream and returns the Message Object
        /// </summary>
        /// <param name="NetStream">Network Stream</param>
        /// <returns>Message Object</returns>
        public static Message Decode(NetworkStream NetStream)
        {
            using (MemoryStream MemStream = new MemoryStream())
            {
                byte[] data = new byte[1024];
                int StreamByte = 0;
                while (NetStream.DataAvailable)
                {
                    StreamByte = NetStream.Read(data, 0, data.Length);
                    MemStream.Write(data, 0, StreamByte);
                }
                MemStream.Seek(0,SeekOrigin.Begin);
                return Serializer.Deserialize<Message>(MemStream); ;
            }
        }
    }
}
