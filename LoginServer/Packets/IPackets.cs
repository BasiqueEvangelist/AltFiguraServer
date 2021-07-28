using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer
{
    public interface IMinecraftC2SPacket
    {
        void Read(MCDataReader mr);

        Task Handle(IMCState state);
    }

    public interface IMinecraftS2CPacket
    {
        int PacketID { get; }

        void Write(MCDataWriter mw);
    }
}