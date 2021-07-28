using System;
using System.Collections.Generic;
using AltFiguraServer.Protocol.Packets;

namespace AltFiguraServer.Protocol
{
    public interface IFiguraState
    {
        List<(string, Func<IFiguraC2SPacket>)> PacketList { get; }
    }
}