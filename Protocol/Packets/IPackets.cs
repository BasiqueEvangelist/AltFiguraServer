using System.IO;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets
{
    public interface IFiguraC2SPacket
    {
        void Read(BinaryReader br);

        Task Handle(IFiguraState state);
    }

    public interface IFiguraS2CPacket
    {
        string ProtocolName { get; }

        void Write(BinaryWriter bw);
    }
}