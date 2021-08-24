using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltFiguraServer.LoginServer.Chat;
using AltFiguraServer.LoginServer.Packets;
using Newtonsoft.Json.Linq;

namespace AltFiguraServer.LoginServer.State
{
    public class StatusState : IMCState
    {
        private readonly MinecraftConnection connection;
        private readonly int protocolVersion;

        public Dictionary<int, Func<IMinecraftC2SPacket>> PacketMap { get; } = new()
        {
            { 0, () => new StatusRequestC2SPacket() },
            { 1, () => new StatusPingC2SPacket() },
        };

        public StatusState(MinecraftConnection connection, int protocolVersion)
        {
            this.connection = connection;
            this.protocolVersion = protocolVersion;
        }

        public async Task OnRequest(StatusRequestC2SPacket packet)
        {
            var desc = new TextChatComponent("AltFiguraServer Login Server");
            var statusResponse = new JObject
            {
                ["version"] = new JObject()
                {
                    ["name"] = "1.17",
                    ["protocol"] = protocolVersion,
                },
                ["players"] = new JObject()
                {
                    ["max"] = 1,
                    ["online"] = 0,
                },
                ["description"] = desc.Serialize()
            };
            await connection.WritePacket(new StatusResponseS2CPacket()
            {
                ResponseData = statusResponse.ToString(Newtonsoft.Json.Formatting.None)
            });
        }

        public async Task OnPing(StatusPingC2SPacket packet)
        {
            await connection.WritePacket(new StatusPongS2CPacket()
            {
                Payload = packet.Payload
            });
        }
    }
}