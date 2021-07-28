using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer.Packets
{
    public class HandshakePacket : IServerboundPacket
    {
        public int ProtocolVersion { get; private set; }
        public string ServerAddress { get; private set; }
        public ushort ServerPort { get; private set; }
        public int NextState { get; private set; }

        public void Read(MCDataReader mr)
        {
            ProtocolVersion = mr.ReadVarInt32();
            ServerAddress = mr.ReadString();
            ServerPort = mr.ReadUInt16();
            NextState = mr.ReadVarInt32();
        }

        public Task Handle(IMCState state)
        {
            return (state as HandshakeState).OnHandshake(this);
        }
    }
}