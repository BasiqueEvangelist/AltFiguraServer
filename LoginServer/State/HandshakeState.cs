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
            if (packet.NextState == 1)
            {
                throw new NotImplementedException();
            }

            connection.CurrentState = new LoginState(connection);
        }
    }
}