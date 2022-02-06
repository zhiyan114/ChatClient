using ProtoBuf;
using System;
using System.IO;
using System.Net.Sockets;

/*
* Name: Main.cs
* Description: Standardized communication protocol between the server and the client
* Author: zhiyan114
*/

/*
 * Library Implementation:
 * - Name: protobuf-net (2.4.6)
 *  - Description: Binary Seralizer for over-the-network communication
 *  - Repository: https://github.com/protobuf-net/protobuf-net
 */

namespace NetworkManager
{
    public static class NetEncoder
    {
        /// <summary>
        /// This function encodes Message Object into byte array that will be stream through NetStream
        /// </summary>
        /// <param name="NetStream">Network Stream</param>
        /// <param name="MsgObj">Message Object</param>
        public static void Encode(Stream NetStream, NetworkMessage MsgObj)
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
        public static NetworkMessage Decode(NetworkStream NetStream)
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
                MemStream.Seek(0, SeekOrigin.Begin);
                return Serializer.Deserialize<NetworkMessage>(MemStream); ;
            }
        }
    }
}
