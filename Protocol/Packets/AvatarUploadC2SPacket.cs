using System.IO;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets
{
    public class AvatarUploadC2SPacket : IFiguraC2SPacket
    {
        public byte[] AvatarData { get; private set; }

        public void Read(BinaryReader br)
        {
            AvatarData = br.ReadBytes(br.ReadInt32());
        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnAvatarUpload(this);
        }
    }
}