using System.IO;

namespace AltFiguraServer.Protocol.Packets.S2C
{
    public class ErrorS2CPacket : IFiguraS2CPacket
    {
        public string ProtocolName => "figura_v1:error";

        public ErrorCode Code { get; set; }

        public void Write(BinaryWriter bw)
        {
            bw.Write((short)Code);
        }

        public enum ErrorCode : short
        {
            ByteRateLimitHit = 0,
            MessageRateLimitHit = 1,
            AvatarsUploadedRateLimitHit = 2,
            AvatarsRequestedRateLimitHit = 3,
            PingBytesRateLimitHit = 4,
            PingsRateLimitHit = 5,
        }
    }
}