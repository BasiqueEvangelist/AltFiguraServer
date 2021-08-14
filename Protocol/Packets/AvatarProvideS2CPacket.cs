using System.IO;

namespace AltFiguraServer.Protocol.Packets
{
    public class AvatarProvideS2CPacket : IFiguraS2CPacket
    {
        public string ProtocolName => "figura_v1:avatar_provide";

        public byte[] ResponseData { get; set; }

        public void Write(BinaryWriter bw)
        {
            bw.Write(ResponseData.Length);
            bw.Write(ResponseData);
        }
    }
}