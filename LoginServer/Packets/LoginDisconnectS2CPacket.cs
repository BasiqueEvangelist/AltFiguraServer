using AltFiguraServer.LoginServer.Chat;

namespace AltFiguraServer.LoginServer.Packets
{
    public class LoginDisconnectS2CPacket : IMinecraftS2CPacket
    {
        public int PacketID => 0;

        public ChatComponent Reason { get; set; }

        public void Write(MCDataWriter mw)
        {
            mw.Write(Reason.Serialize().ToString(Newtonsoft.Json.Formatting.None));
        }
    }
}