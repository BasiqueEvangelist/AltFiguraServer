using System;
using System.IO;

namespace AltFiguraServer.Protocol.Packets.S2C
{
    public class ChannelAvatarUpdateS2CPacket : IFiguraS2CPacket
    {
        public string ProtocolName => "figura_v1:channel_avatar_update";

        public Guid SourceUser { get; set; }

        public void Write(BinaryWriter bw)
        {
            bw.Write(SourceUser);
        }
    }
}