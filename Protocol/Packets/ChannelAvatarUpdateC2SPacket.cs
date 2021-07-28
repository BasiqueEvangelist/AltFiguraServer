using System.IO;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets
{
    public class ChannelAvatarUpdateC2SPacket : IFiguraC2SPacket
    {
        public void Read(BinaryReader br) { }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnChannelAvatarUpdate(this);
        }
    }
}