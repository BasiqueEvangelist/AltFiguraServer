using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer.Packets
{
    public class EncryptionResponsePacket : IServerboundPacket
    {
        public byte[] SharedSecret { get; private set; }
        public byte[] VerifyToken { get; private set; }

        public void Read(MCDataReader mr)
        {
            SharedSecret = mr.ReadBytes(mr.ReadVarInt32());
            VerifyToken = mr.ReadBytes(mr.ReadVarInt32());
        }

        public Task Handle(IMCState state)
        {
            return (state as LoginState).OnEncryptionResponse(this);
        }
    }
}