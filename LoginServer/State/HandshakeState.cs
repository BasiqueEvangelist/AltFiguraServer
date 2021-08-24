using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltFiguraServer.LoginServer.Packets;

namespace AltFiguraServer.LoginServer.State
{
    public class HandshakeState : IMCState
    {
        private readonly MinecraftConnection connection;

        public Dictionary<int, Func<IMinecraftC2SPacket>> PacketMap { get; } = new()
        {
            { 0, () => new HandshakeC2SPacket() },
        };

        public HandshakeState(MinecraftConnection connection)
        {
            this.connection = connection;
        }

        public async Task OnHandshake(HandshakeC2SPacket packet)
        {
            connection.CurrentState = packet.NextState switch
            {
                1 => new StatusState(connection, packet.ProtocolVersion),
                2 => new LoginState(connection),
                _ => throw new NotImplementedException()
            };
        }
    }
}