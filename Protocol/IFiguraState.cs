using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AltFiguraServer.Protocol.Packets;

namespace AltFiguraServer.Protocol
{
    public interface IFiguraState : IDisposable
    {
        List<(string, Func<IFiguraC2SPacket>)> PacketList { get; }

        void Attach(WebSocketConnection connection);

        Task OnAuthenticated(Guid playerId);

        Task<bool> OnMessageReceived(MemoryStream ms);
    }
}