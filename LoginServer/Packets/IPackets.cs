using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer
{
    public interface IServerboundPacket
    {
        void Read(MCDataReader mr);

        Task Handle(IMCState state);
    }

    public interface IClientboundPacket
    {
        int PacketID { get; }

        void Write(MCDataWriter mw);
    }
}