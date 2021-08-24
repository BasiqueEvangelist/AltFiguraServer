using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer.Packets
{
    public class StatusResponseS2CPacket : IMinecraftS2CPacket
    {
        public int PacketID => 0;

        public string ResponseData { get; set; }

        public void Write(MCDataWriter mw)
        {
            mw.Write(ResponseData);
        }
    }
}