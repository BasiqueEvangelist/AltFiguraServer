using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltFiguraServer.LoginServer.Packets;

namespace AltFiguraServer.LoginServer.State
{
    public class HandshakeState : IMCState
    {
        private readonly MinecraftConnection connection;

        public Dictionary<int, Func<IServerboundPacket>> PacketMap { get; } = new()
        {
            { 0, () => new HandshakePacket() },
        };

        public HandshakeState(MinecraftConnection connection)
        {
            this.connection = connection;
        }

        public async Task OnHandshake(HandshakePacket packet)
        {
            if (packet.NextState == 1)
            {
                throw new NotImplementedException();
            }

            connection.CurrentState = new LoginState(connection);
        }
    }
}