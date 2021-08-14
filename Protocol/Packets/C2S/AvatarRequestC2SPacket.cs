using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets.C2S
{
    public class AvatarRequestC2SPacket : IFiguraC2SPacket
    {
        public Guid RequestedAvatarID { get; private set; }

        public void Read(BinaryReader br)
        {
            RequestedAvatarID = br.ReadGuid();
        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnAvatarRequest(this);
        }
    }
}