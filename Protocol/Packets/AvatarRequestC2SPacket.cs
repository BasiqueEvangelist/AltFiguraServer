using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets
{
    public class AvatarRequestC2SPacket : IFiguraC2SPacket
    {
        public Guid RequestedAvatarID { get; private set; }

        public void Read(BinaryReader br)
        {

        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnAvatarRequest(this);
        }
    }
}