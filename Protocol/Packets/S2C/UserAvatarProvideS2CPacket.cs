using System;
using System.IO;

namespace AltFiguraServer.Protocol.Packets.S2C
{
    public class UserAvatarProvideS2CPacket : IFiguraS2CPacket
    {
        public string ProtocolName => "figura_v1:user_avatar_provide";

        public Guid PlayerId { get; set; }
        public byte[] AvatarData { get; set; }

        public void Write(BinaryWriter bw)
        {
            bw.Write(PlayerId);
            bw.Write(AvatarData.Length);
            bw.Write(AvatarData);
        }
    }
}