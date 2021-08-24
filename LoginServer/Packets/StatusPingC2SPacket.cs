using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer.Packets
{
    public class StatusPingC2SPacket : IMinecraftC2SPacket
    {
        public long Payload { get; set; }

        public void Read(MCDataReader mr)
        {
            Payload = mr.ReadInt64();
        }

        public Task Handle(IMCState state)
        {
            return (state as StatusState).OnPing(this);
        }
    }
}