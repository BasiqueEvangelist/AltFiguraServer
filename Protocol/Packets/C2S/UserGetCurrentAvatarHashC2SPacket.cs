using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets.C2S
{
    public class UserGetCurrentAvatarHashC2SPacket : IFiguraC2SPacket
    {
        public Guid RequestedPlayerID { get; private set; }

        public void Read(BinaryReader br)
        {
            RequestedPlayerID = br.ReadGuid();
        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnUserGetCurrentAvatarHash(this);
        }
    }
}