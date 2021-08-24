using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer.Packets
{
    public class StatusPongS2CPacket : IMinecraftS2CPacket
    {
        public int PacketID => 1;

        public long Payload { get; set; }

        public void Write(MCDataWriter mw)
        {
            mw.Write(Payload);
        }
    }
}