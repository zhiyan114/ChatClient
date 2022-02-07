using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetworkManager
{
    /// <summary>
    /// Primary Class to send message
    /// </summary>
    [ProtoContract]
    public class NetworkMessage
    {
        public NetworkMessage() { } // Protobuf Support
        public NetworkMessage(MessageType Type, object Data)
        {
            this.Type = Type;
            this.Data = Data;
        }
        [ProtoMember(1)]
        [DefaultValue(MessageType.ChatMessage)]
        public MessageType Type { get; set; } = MessageType.ChatMessage;
        [ProtoMember(2,DynamicType = true)]
        public object Data { get; set; }
    }
    [ProtoContract]
    public class Message
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
    [ProtoContract]
    public class Command
    {
        public Command() { } // Protobuf Support
        public Command(CommandType Type, params string[] Args)
        {
            this.Type = Type;
            this.Args = Args;
        }
        [ProtoMember(1)]
        public CommandType Type { get; set; }
        [ProtoMember(2)]
        [DefaultValue(new string[0])]
        public string[] Args { get; set; } = Array.Empty<string>();
    }
}
