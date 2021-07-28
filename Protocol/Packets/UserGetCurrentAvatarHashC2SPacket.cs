using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets
{
    public class UserGetCurrentAvatarHashC2SPacket : IFiguraC2SPacket
    {
        public Guid RequestedPlayerID { get; private set; }

        public void Read(BinaryReader br)
        {
            string idString = Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32()));
            RequestedPlayerID = Guid.Parse(idString);
        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnUserGetCurrentAvatarHash(this);
        }
    }
}