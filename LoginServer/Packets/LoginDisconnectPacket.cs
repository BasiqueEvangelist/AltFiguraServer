using AltFiguraServer.LoginServer.Chat;

namespace AltFiguraServer.LoginServer.Packets
{
    public class LoginDisconnectPacket : IClientboundPacket
    {
        public int PacketID => 0;

        public ChatComponent Reason { get; set; }

        public void Write(MCDataWriter mw)
        {
            mw.Write(Reason.Serialize().ToString(Newtonsoft.Json.Formatting.None));
        }
    }
}