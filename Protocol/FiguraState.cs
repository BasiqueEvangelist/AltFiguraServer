using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AltFiguraServer.Data;
using AltFiguraServer.Protocol.Packets;
using AltFiguraServer.Protocol.Packets.C2S;
using AltFiguraServer.Protocol.Packets.S2C;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;

namespace AltFiguraServer.Protocol
{
    public class FiguraState : IFiguraState
    {
        private const int MAXIMUM_DATA_SIZE = 100 * 1024;
        private static readonly SHA256 sha256 = SHA256.Create();

        private readonly Database db;
        private readonly ILogger<FiguraState> logger;
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

        public FiguraState(Database db, ILogger<FiguraState> logger)
        {
            this.db = db;
            this.logger = logger;
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
            var data = await db.GetAvatarBytes(packet.RequestedAvatarID);
            if (data != null)
                await connection.WritePacket(new AvatarProvideS2CPacket() { ResponseData = data });
        }

        public async Task OnAvatarUpload(AvatarUploadC2SPacket packet)
        {
            if (user.OwnedAvatars.Count >= 100)
            {
                await connection.WritePacket(new AvatarUploadS2CPacket() { ReturnCode = AvatarUploadS2CPacket.UploadReturnCode.TooManyAvatars });
                return;
            }
            if (packet.AvatarData.Length == 0)
            {
                await connection.WritePacket(new AvatarUploadS2CPacket() { ReturnCode = AvatarUploadS2CPacket.UploadReturnCode.EmptyAvatar });
                return;
            }
            if (await db.GetTotalUserDataSize(playerId) + packet.AvatarData.Length > MAXIMUM_DATA_SIZE)
            {
                await connection.WritePacket(new AvatarUploadS2CPacket() { ReturnCode = AvatarUploadS2CPacket.UploadReturnCode.NotEnoughSpace });
                return;
            }

            Guid avatarId = Guid.NewGuid();

            Database.Avatar avatar = new()
            {
                Uuid = avatarId,
                Nbt = packet.AvatarData,
                Hash = Encoding.UTF8.GetString(sha256.ComputeHash(packet.AvatarData)),
                Tags = ""
            };

            await db.PostAvatar(user.Uuid, avatar);
            user.OwnedAvatars.Add(avatar.Uuid);
            user.TotalAvatarSize += avatar.Size;

            await connection.WritePacket(new AvatarUploadS2CPacket()
            {
                ReturnCode = AvatarUploadS2CPacket.UploadReturnCode.Success,
                AvatarId = avatarId
            });
        }

        public async Task OnUserSetAvatar(UserSetAvatarC2SPacket packet)
        {
            await db.SetUserAvatar(user.Uuid, packet.AvatarId, packet.ShouldDelete);
        }

        public async Task OnUserDeleteCurrentAvatar(UserDeleteCurrentAvatarC2SPacket packet)
        {
            await db.SetUserAvatar(user.Uuid, Guid.Empty, true);
        }

        public async Task OnUserGetCurrentAvatar(UserGetCurrentAvatarC2SPacket packet)
        {
            var data = await db.GetUserAvatarBytes(packet.RequestedPlayerID);
            if (data != null)
                await connection.WritePacket(new UserAvatarProvideS2CPacket()
                {
                    PlayerId = packet.RequestedPlayerID,
                    AvatarData = data
                });
        }

        public async Task OnUserGetCurrentAvatarHash(UserGetCurrentAvatarHashC2SPacket packet)
        {
            var hash = await db.GetUserAvatarHash(packet.RequestedPlayerID);
            if (hash != null)
                await connection.WritePacket(new UserAvatarHashProvideS2CPacket()
                {
                    PlayerId = packet.RequestedPlayerID,
                    AvatarHash = hash
                });
        }

        public async Task OnUserEventSubscribe(UserEventSubscribeC2SPacket packet)
        {
            logger.LogDebug("UserEventSubscribe");
        }

        public async Task OnUserEventUnsubscribe(UserEventUnsubscribeC2SPacket packet)
        {
            logger.LogDebug("UserEventUnsubscribe");
        }

        public async Task OnChannelAvatarUpdate(ChannelAvatarUpdateC2SPacket packet)
        {
            logger.LogDebug("ChannelAvatarUpdate");
        }

        public async Task OnPing(PingC2SPacket packet)
        {
            logger.LogDebug("Ping");
        }
    }
}