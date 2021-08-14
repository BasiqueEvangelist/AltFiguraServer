using System;
using System.IO;
using System.Text;

namespace AltFiguraServer.Protocol.Packets
{
    public class UserAvatarHashProvideS2CPacket : IFiguraS2CPacket
    {
        public string ProtocolName => "figura_v1:user_avatar_hash_provide";

        public Guid PlayerId { get; set; }
        public string AvatarHash { get; set; }

        public void Write(BinaryWriter bw)
        {
            bw.Write(PlayerId);

            byte[] hashBytes = Encoding.UTF8.GetBytes(AvatarHash);
            bw.Write(hashBytes.Length);
            bw.Write(hashBytes);
        }
    }
}