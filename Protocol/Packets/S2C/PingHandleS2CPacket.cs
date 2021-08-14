using System;
using System.IO;

namespace AltFiguraServer.Protocol.Packets.S2C
{
    public class PingHandleS2CPacket : IFiguraS2CPacket
    {
        public string ProtocolName => "figura_v1:ping_handle";

        public Guid SourceUser { get; set; }
        public byte[] PingData { get; set; }

        public void Write(BinaryWriter bw)
        {
            bw.Write(SourceUser);
            bw.Write(PingData.Length);
            bw.Write(PingData);
        }
    }
}