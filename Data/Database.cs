using System;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace AltFiguraServer.Data
{
    public class Database
    {
        private readonly string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<User> GetOrCreateUser(Guid id)
        {
            using var c = GetConnection();

            var user = await c.QuerySingleOrDefaultAsync<User>(
                "SELECT * " +
                "FROM user_data " +
                "WHERE uuid = @id",
                 new { id });
            if (user == null)
            {
                user = await c.QuerySingle(
                    "INSERT INTO user_data " +
                    "(uuid, current_avatar, total_avatar_size, owned_avatars, owned_packs, karma, trust_data, favorite_data, config) " +
                    "VALUES " +
                    "(@Uuid, @OwnedAvatars, @OwnedPacks, @Karma)",
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
            return user;
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
    }
}