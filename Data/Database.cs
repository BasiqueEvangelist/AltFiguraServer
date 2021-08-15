using System;
using System.Threading.Tasks;
using AltFiguraServer.Protocol.Packets.S2C;
using Dapper;
using MySqlConnector;

namespace AltFiguraServer.Data
{
    public class Database
    {
        private const int MAXIMUM_DATA_SIZE = 100 * 1024;

        private readonly string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString + ";TreatTinyAsBoolean=false;Allow User Variables=true";
        }

        public async Task CreateUserIfNeeded(Guid id)
        {
            using var c = GetConnection();

            await c.ExecuteAsync(
                "INSERT INTO user_data " +
                "(uuid, current_avatar, total_avatar_size, owned_avatars, owned_packs, karma, trust_data, favorite_data, config) " +
                "VALUES " +
                "(@Uuid, @CurrentAvatar, @TotalAvatarSize, @OwnedAvatars, @OwnedPacks, @Karma, @TrustData, @FavoriteData, @Config) " +
                "ON DUPLICATE KEY UPDATE uuid=uuid",
                new User()
                {
                    Uuid = id,
                    OwnedAvatars = new GuidList(),
                    OwnedPacks = new GuidList(),
                    Karma = 0,
                    TrustData = Array.Empty<byte>(),
                    FavoriteData = Array.Empty<byte>(),
                    Config = Array.Empty<byte>(),
                });
        }

        public async Task<byte[]> GetAvatarBytes(Guid avatarId)
        {
            using var c = GetConnection();
            return await c.QuerySingleOrDefaultAsync<byte[]>(
                "SELECT nbt " +
                "FROM avatar_data " +
                "WHERE uuid = @AvatarId",
                new { AvatarId = avatarId });
        }

        public async Task<byte[]> GetUserAvatarBytes(Guid userId)
        {
            using var c = GetConnection();
            return await c.QuerySingleOrDefaultAsync<byte[]>(
                "SELECT a.nbt " +
                "FROM user_data u " +
                "INNER JOIN avatar_data a " +
                "ON u.current_avatar = a.uuid " +
                "WHERE u.uuid = @UserId",
                new { UserId = userId });
        }

        public async Task<string> GetUserAvatarHash(Guid userId)
        {
            using var c = GetConnection();
            return await c.QuerySingleOrDefaultAsync<string>(
                "SELECT a.hash " +
                "FROM user_data u " +
                "INNER JOIN avatar_data a " +
                "ON u.current_avatar = a.uuid " +
                "WHERE u.uuid = @UserId",
                new { UserId = userId });
        }

        public async Task<int> GetTotalUserDataSize(Guid userId)
        {
            using var c = GetConnection();
            return await c.QuerySingleOrDefaultAsync<int>(
                "SELECT total_avatar_size " +
                "FROM user_data " +
                "WHERE uuid = @UserId",
                new { UserId = userId }
            );
        }

        public async Task<AvatarUploadS2CPacket.UploadReturnCode> PostAvatar(Guid userId, Avatar avatar)
        {
            using var c = GetConnection();
            await c.OpenAsync();
            using var tx = await c.BeginTransactionAsync();

            var user = await c.QuerySingleAsync<User>(
                "SELECT total_avatar_size TotalAvatarSize, owned_avatars OwnedAvatars " +
                "FROM user_data " +
                "WHERE uuid = @UserId",
                new { UserId = userId },
                transaction: tx
            );
            if (user.OwnedAvatars.Count >= 100)
            {
                return AvatarUploadS2CPacket.UploadReturnCode.TooManyAvatars;
            }
            if (user.TotalAvatarSize + avatar.Size > MAXIMUM_DATA_SIZE)
            {
                return AvatarUploadS2CPacket.UploadReturnCode.NotEnoughSpace;
            }

            await c.ExecuteAsync(
                "UPDATE user_data " +
                "SET total_avatar_size = total_avatar_size + @Size, " +
                "owned_avatars = IF(owned_avatars = '', @AvatarId, CONCAT(owned_avatars, ';', @AvatarId)) " +
                "WHERE uuid = @UserId",
                new { avatar.Size, AvatarId = avatar.Uuid, UserId = userId },
                transaction: tx
            );

            await c.ExecuteAsync(
                "INSERT INTO avatar_data " +
                "(uuid, nbt, size, hash, tags) " +
                "VALUES " +
                "(@Uuid, @Nbt, @Size, @Hash, @Tags)",
                avatar,
                transaction: tx
            );

            await tx.CommitAsync();
            return AvatarUploadS2CPacket.UploadReturnCode.Success;
        }

        public async Task SetUserAvatar(Guid userId, Guid avatarId, bool deleteOld = false)
        {
            using var c = GetConnection();
            await c.OpenAsync();
            using var tx = await c.BeginTransactionAsync();

            if (deleteOld)
            {
                await c.ExecuteAsync(
                    @"SELECT current_avatar
                    INTO @CurrentAvatar
                    FROM user_data
                    WHERE uuid = @UserId;

                    UPDATE user_data u
                    INNER JOIN avatar_data a
                    ON a.uuid = u.current_avatar
                    SET u.total_avatar_size = u.total_avatar_size - a.size,
                    owned_avatars = REPLACE(REPLACE(owned_avatars, a.uuid, ''), ',,', ',')
                    WHERE u.uuid = @UserId;

                    DELETE FROM avatar_data
                    WHERE uuid = @CurrentAvatar",
                    new { UserId = userId },
                    transaction: tx
                );
            }

            await c.ExecuteAsync(
                "UPDATE user_data " +
                "SET current_avatar = @AvatarId " +
                "WHERE uuid = @UserId",
                new { UserId = userId, AvatarId = avatarId },
                transaction: tx
            );

            await tx.CommitAsync();
        }

        MySqlConnection GetConnection() => new(connectionString);

        public class User
        {
            public Guid Uuid { get; set; }
            public Guid CurrentAvatar { get; set; }
            public int TotalAvatarSize { get; set; }
            public GuidList OwnedAvatars { get; set; }
            public GuidList OwnedPacks { get; set; }
            public double Karma { get; set; }
            public byte[] TrustData { get; set; }
            public byte[] FavoriteData { get; set; }
            public byte[] Config { get; set; }
        }

        public class Avatar
        {
            public Guid Uuid { get; set; }
            public byte[] Nbt { get; set; }
            public int Size { get; set; }
            public string Hash { get; set; }
            public string Tags { get; set; }
        }
    }
}