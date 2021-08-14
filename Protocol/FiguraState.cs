using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltFiguraServer.Data;
using AltFiguraServer.Protocol.Packets;

namespace AltFiguraServer.Protocol
{
    public class FiguraState : IFiguraState
    {
        private readonly Database db;
        private WebSocketConnection connection;
        private Guid playerId;
        private Database.User user;

        public List<(string, Func<IFiguraC2SPacket>)> PacketList { get; } = new()
        {
            ("figura_v1:avatar_request", () => new AvatarRequestC2SPacket()),
            ("figura_v1:avatar_upload", () => new AvatarUploadC2SPacket()),
            ("figura_v1:user_set_avatar", () => new UserSetAvatarC2SPacket()),
            ("figura_v1:user_delete_current_avatar", () => new UserDeleteCurrentAvatarC2SPacket()),
            ("figura_v1:user_get_current_avatar", () => new UserGetCurrentAvatarC2SPacket()),
            ("figura_v1:user_get_current_avatar_hash", () => new UserGetCurrentAvatarHashC2SPacket()),
            ("figura_v1:user_event_sub", () => new UserEventSubscribeC2SPacket()),
            ("figura_v1:user_events_unsub", () => new UserEventUnsubscribeC2SPacket()),
            ("figura_v1:channel_avatar_update", () => new ChannelAvatarUpdateC2SPacket()),
            ("figura_v1:ping", () => new PingC2SPacket())
        };

        public FiguraState(Database db)
        {
            this.db = db;
        }

        public void Attach(WebSocketConnection connection)
        {
            this.connection = connection;
        }

        public async Task OnAuthenticated(Guid playerId)
        {
            this.playerId = playerId;

            user = await db.GetOrCreateUser(playerId);
        }

        public async Task OnAvatarRequest(AvatarRequestC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnAvatarUpload(AvatarUploadC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnUserSetAvatar(UserSetAvatarC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnUserDeleteCurrentAvatar(UserDeleteCurrentAvatarC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnUserGetCurrentAvatar(UserGetCurrentAvatarC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnUserGetCurrentAvatarHash(UserGetCurrentAvatarHashC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnUserEventSubscribe(UserEventSubscribeC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnUserEventUnsubscribe(UserEventUnsubscribeC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnChannelAvatarUpdate(ChannelAvatarUpdateC2SPacket packet)
        {
            throw new NotImplementedException();
        }

        public async Task OnPing(PingC2SPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}