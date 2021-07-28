using System.Security.Cryptography;

namespace AltFiguraServer.LoginServer.Packets
{
    public class EncryptionRequestPacket : IClientboundPacket
    {
        public int PacketID => 1;

        public string ServerID { get; set; }
        public byte[] PrivateKey { get; set; }
        public byte[] VerifyToken { get; set; }

        public void Write(MCDataWriter mw)
        {
            mw.Write(ServerID);
            mw.WriteVarInt32(PrivateKey.Length);
            mw.Write(PrivateKey);
            mw.WriteVarInt32(VerifyToken.Length);
            mw.Write(VerifyToken);
        }
    }
}