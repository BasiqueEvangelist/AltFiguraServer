using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AltFiguraServer.Data;
using AltFiguraServer.Pings;
using AltFiguraServer.Protocol.Packets;
using AltFiguraServer.Protocol.Packets.C2S;
using AltFiguraServer.Protocol.Packets.S2C;
using Microsoft.Extensions.Logging;

namespace AltFiguraServer.Protocol
{
    public class FiguraState : IFiguraState
    {
        private static readonly SHA256 sha256 = SHA256.Create();

        private readonly Database db;
        private readonly ILogger<FiguraState> logger;
        private WebSocketConnection connection;
        private Guid playerId;
        private FiguraPeer peer;

        public Guid PlayerId => playerId;

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

            await db.CreateUserIfNeeded(playerId);

            peer = new FiguraPeer(connection, this);
            P2PManager.RegisterPeer(peer);
        }

        public async Task OnAvatarRequest(AvatarRequestC2SPacket packet)
        {
            if (!DefaultRateLimiters.AvatarsRequested.TryPerform(playerId, 1))
            {
                await connection.WritePacket(new ErrorS2CPacket()
                {
                    Code = ErrorS2CPacket.ErrorCode.AvatarsRequestedRateLimitHit
                });
                return;
            }

            var data = await db.GetAvatarBytes(packet.RequestedAvatarID);
            if (data != null)
                await connection.WritePacket(new AvatarProvideS2CPacket() { ResponseData = data });
        }

        public async Task OnAvatarUpload(AvatarUploadC2SPacket packet)
        {
            if (!DefaultRateLimiters.AvatarsUploaded.TryPerform(playerId, 1))
            {
                await connection.WritePacket(new ErrorS2CPacket()
                {
                    Code = ErrorS2CPacket.ErrorCode.AvatarsUploadedRateLimitHit
                });
                return;
            }

            if (packet.AvatarData.Length == 0)
            {
                await connection.WritePacket(new AvatarUploadS2CPacket() { ReturnCode = AvatarUploadS2CPacket.UploadReturnCode.EmptyAvatar });
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

            var errCode = await db.PostAvatar(playerId, avatar);

            await connection.WritePacket(new AvatarUploadS2CPacket()
            {
                ReturnCode = errCode,
                AvatarId = avatarId
            });
        }

        public async Task OnUserSetAvatar(UserSetAvatarC2SPacket packet)
        {
            await db.SetUserAvatar(playerId, packet.AvatarId, packet.ShouldDelete);
        }

        public async Task OnUserDeleteCurrentAvatar(UserDeleteCurrentAvatarC2SPacket packet)
        {
            await db.SetUserAvatar(playerId, Guid.Empty, true);
        }

        public async Task OnUserGetCurrentAvatar(UserGetCurrentAvatarC2SPacket packet)
        {
            if (!DefaultRateLimiters.AvatarsRequested.TryPerform(playerId, 1))
            {
                await connection.WritePacket(new ErrorS2CPacket()
                {
                    Code = ErrorS2CPacket.ErrorCode.AvatarsRequestedRateLimitHit
                });
                return;
            }

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
            foreach (Guid target in packet.Targets)
            {
                P2PManager.GetPeer(target).Subscribe(peer);
            }
        }

        public async Task OnUserEventUnsubscribe(UserEventUnsubscribeC2SPacket packet)
        {
            foreach (Guid target in packet.Targets)
            {
                P2PManager.GetPeer(target).Unsubscribe(peer);
            }
        }

        public async Task OnChannelAvatarUpdate(ChannelAvatarUpdateC2SPacket packet)
        {
            await peer.Publish(new ChannelAvatarUpdateS2CPacket()
            {
                SourceUser = playerId
            });
        }

        public async Task OnPing(PingC2SPacket packet)
        {
            if (!DefaultRateLimiters.Pings.TryPerform(playerId, 1))
            {
                await connection.WritePacket(new ErrorS2CPacket()
                {
                    Code = ErrorS2CPacket.ErrorCode.PingsRateLimitHit
                });
                return;
            }

            if (!DefaultRateLimiters.PingBytes.TryPerform(playerId, packet.Data.Length - sizeof(short)))
            {
                await connection.WritePacket(new ErrorS2CPacket()
                {
                    Code = ErrorS2CPacket.ErrorCode.AvatarsRequestedRateLimitHit
                });
                return;
            }

            await peer.Publish(new PingHandleS2CPacket()
            {
                SourceUser = playerId,
                PingData = packet.Data
            });
        }

        public async Task<bool> OnMessageReceived(MemoryStream ms)
        {
            if (!DefaultRateLimiters.BytesSent.TryPerform(playerId, (int)ms.Length))
            {
                await connection.WritePacket(new ErrorS2CPacket()
                {
                    Code = ErrorS2CPacket.ErrorCode.ByteRateLimitHit
                });
                return false;
            }

            if (!DefaultRateLimiters.MessagesSent.TryPerform(playerId, 1))
            {
                await connection.WritePacket(new ErrorS2CPacket()
                {
                    Code = ErrorS2CPacket.ErrorCode.MessageRateLimitHit
                });
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            peer?.Dispose();
        }
    }
}