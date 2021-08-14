using System.IO;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets.C2S
{
    public class PingC2SPacket : IFiguraC2SPacket
    {
        public byte[] Data { get; set; }

        public void Read(BinaryReader br)
        {
            Data = br.ReadBytes(br.ReadInt32());
        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnPing(this);
        }
    }
}