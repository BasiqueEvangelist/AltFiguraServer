using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltFiguraServer.Protocol;
using AltFiguraServer.Protocol.Packets;

namespace AltFiguraServer.Pings
{
    public class FiguraPeer : IDisposable
    {
        private readonly WebSocketConnection connection;
        private readonly FiguraState state;
        private readonly HashSet<FiguraPeer> subscribers = new();
        private readonly object subscribersLock = new();

        public FiguraPeer(WebSocketConnection connection, FiguraState state)
        {
            this.connection = connection;
            this.state = state;
        }

        public Guid PlayerId => state.PlayerId;

        public void Subscribe(FiguraPeer peer)
        {
            lock (subscribersLock)
                subscribers.Add(peer);
        }

        public void Unsubscribe(FiguraPeer peer)
        {
            lock (subscribersLock)
                subscribers.Remove(peer);
        }

        public async Task Publish(IFiguraS2CPacket packet)
        {
            HashSet<FiguraPeer> snapshot;
            lock (subscribersLock)
                snapshot = new(subscribers);

            foreach (FiguraPeer peer in subscribers)
            {
                if (peer.HasSubscriber(this))
                    await peer.connection.WritePacket(packet);
            }
        }

        private bool HasSubscriber(FiguraPeer peer)
        {
            lock (subscribersLock)
                return subscribers.Contains(peer);
        }

        public void Dispose()
        {
            P2PManager.DropPeer(this);
        }
    }
}