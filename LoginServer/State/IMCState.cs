using System;
using System.Collections.Generic;

namespace AltFiguraServer.LoginServer.State
{
    public interface IMCState
    {
        Dictionary<int, Func<IMinecraftC2SPacket>> PacketMap { get; }
    }
}