using System;
using System.IO;

namespace AltFiguraServer.Protocol.Packets.S2C
{
    public class AvatarUploadS2CPacket : IFiguraS2CPacket
    {
        public string ProtocolName => "figura_v1:avatar_upload";

        public UploadReturnCode ReturnCode { get; set; }
        public Guid AvatarId { get; set; }

        public void Write(BinaryWriter bw)
        {
            bw.Write((sbyte)ReturnCode);

            if (ReturnCode == UploadReturnCode.Success)
                bw.Write(AvatarId);
        }

        public enum UploadReturnCode : sbyte
        {
            Success = 0,
            TooManyAvatars = 1,
            EmptyAvatar = 2,
            NotEnoughSpace = 3
        }
    }
}