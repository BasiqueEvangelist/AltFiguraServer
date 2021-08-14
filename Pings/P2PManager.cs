using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AltFiguraServer.Pings
{
    public static class P2PManager
    {
        private static readonly ConcurrentDictionary<Guid, FiguraPeer> peers = new();

        public static void RegisterPeer(FiguraPeer peer)
        {
            if (!peers.TryAdd(peer.PlayerId, peer))
            {
                throw new InvalidOperationException($"Peer has already been registered under id {peer.PlayerId}!");
            }
        }

        public static void DropPeer(FiguraPeer peer)
        {
            peers.Remove(peer.PlayerId, out _);
        }

        public static FiguraPeer GetPeer(Guid playerId)
        {
            return peers.GetValueOrDefault(playerId);
        }
    }
}