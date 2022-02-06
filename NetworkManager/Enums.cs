using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkManager
{
    [ProtoContract]
    public enum MessageType
    {
        [ProtoEnum]
        ChatMessage,
        [ProtoEnum]
        Command,
    }
    [ProtoContract]
    public enum CommandType
    {
        [ProtoEnum]
        Shutdown,
    }
}
