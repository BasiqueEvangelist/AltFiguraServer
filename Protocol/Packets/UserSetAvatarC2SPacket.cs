using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets
{
    public class UserSetAvatarC2SPacket : IFiguraC2SPacket
    {
        public Guid AvatarId { get; private set; }
        public bool ShouldDelete { get; private set; }

        public void Read(BinaryReader br)
        {
            string idString = Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32()));
            AvatarId = Guid.Parse(idString);

            ShouldDelete = br.ReadSByte() == 1;
        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnUserSetAvatar(this);
        }
    }
}