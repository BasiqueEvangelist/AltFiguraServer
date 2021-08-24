using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer.Packets
{
    public class StatusRequestC2SPacket : IMinecraftC2SPacket
    {
        public void Read(MCDataReader mr) { }

        public Task Handle(IMCState state)
        {
            return (state as StatusState).OnRequest(this);
        }
    }
}