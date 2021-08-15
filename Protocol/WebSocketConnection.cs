using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using AltFiguraServer.LoginServer;
using AltFiguraServer.Protocol.Packets;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AltFiguraServer.Protocol
{
    public class WebSocketConnection
    {
        private readonly WebSocket ws;
        private readonly ILogger<WebSocketConnection> logger;
        private IFiguraState state;

        public ProtocolRegistry Registry { get; } = new();
        public IFiguraState CurrentState
        {
            get => state;
            set
            {
                value.Attach(this);
                state = value;
            }
        }

        public WebSocketConnection(WebSocket ws, ILogger<WebSocketConnection> logger, IFiguraState state)
        {
            this.ws = ws;
            this.logger = logger;
            CurrentState = state;
        }

        public async Task Run()
        {
            try
            {
                if (!await Setup()) return;

                while (true)
                {
                    await ReadMessage();
                }
            }
            catch (EndOfStreamException) { }
            catch (Exception e)
            {
                logger.LogError(e, "Encountered network error");
            }
            finally
            {
                if (ws.State != WebSocketState.Aborted && ws.State != WebSocketState.Closed)
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing socket", default);
                CurrentState.Dispose();
            }
        }

        public async Task<bool> WritePacket(IFiguraS2CPacket packet)
        {
            if (!Registry.RemoteSupportsMessage(packet.ProtocolName))
            {
                logger.LogWarning($"Not sending packet {packet.ProtocolName} as remote doesn't support it");
                return false;
            }

            byte[] data;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(Registry.GetRemoteId(packet.ProtocolName));
                packet.Write(bw);

                data = ms.ToArray();
            }

            await ws.SendAsync(data.AsMemory(), WebSocketMessageType.Binary, true, default);
            return true;
        }

        private async Task ReadMessage()
        {
            IFiguraC2SPacket packet;

            using (var dataStream = await CollectMessage())
            using (var br = new BinaryReader(dataStream))
            {
                if (!await CurrentState.OnMessageReceived(dataStream))
                {
                    return;
                }

                int packetId = br.ReadSByte() - sbyte.MinValue - 1;
                if (CurrentState.PacketList.Count <= packetId)
                {
                    logger.LogError("Invalid Packet ID " + packetId);
                    return;
                }

                packet = CurrentState.PacketList[packetId].Item2();

                logger.LogDebug("Reading packet " + packet);
                packet.Read(br);
            }

            await packet.Handle(CurrentState);
        }

        private async Task<bool> Setup()
        {
            string jwt = Encoding.UTF8.GetString((await CollectMessage()).ToArray());
            if (!SessionUtils.ValidateToken(jwt, out var token))
                return false;

            await CurrentState.OnAuthenticated(Guid.Parse(token.Subject));

            using (var registryMessageStream = await CollectMessage())
            using (var br = new BinaryReader(registryMessageStream))
            {
                Registry.ReadFrom(br);
            }

            await SendServerRegistry();

            return true;
        }

        private async Task SendServerRegistry()
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(CurrentState.PacketList.Count);
            foreach (var (key, _) in CurrentState.PacketList)
            {
                bw.Write(Encoding.UTF8.GetByteCount(key));
                bw.Write(Encoding.UTF8.GetBytes(key));
            }

            await ws.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true, default);
        }

        private async Task<MemoryStream> CollectMessage()
        {
            MemoryStream ms = new();

            ValueWebSocketReceiveResult result;
            byte[] buffer = new byte[1024];
            do
            {
                if (ms.Length > 200 * 1024)
                {
                    logger.LogError("Websockets message is more than 200 KB! Something has gone terribly wrong.");
                    throw new EndOfStreamException();
                }

                if (ws.CloseStatus != null) throw new EndOfStreamException();
                result = await ws.ReceiveAsync(buffer.AsMemory(), default);
                ms.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            ms.Position = 0;
            return ms;
        }
    }
}