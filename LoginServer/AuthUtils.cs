using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AltFiguraServer.LoginServer
{
    public static class AuthUtils
    {
        public static RSA ServerKey { get; } = RSA.Create(1024);
        private static readonly HttpClient client = new();

        // Taken from https://gist.github.com/ammaraskar/7b4a3f73bee9dc4136539644a0f27e63.
        public static string MinecraftShaDigest(byte[] input)
        {
            var hash = new SHA1Managed().ComputeHash(input);
            // Reverse the bytes since BigInteger uses little endian
            Array.Reverse(hash);

            BigInteger b = new BigInteger(hash);
            // very annoyingly, BigInteger in C# tries to be smart and puts in
            // a leading 0 when formatting as a hex number to allow roundtripping 
            // of negative numbers, thus we have to trim it off.
            if (b < 0)
            {
                // toss in a negative sign if the interpreted number is negative
                return "-" + (-b).ToString("x").TrimStart('0');
            }
            else
            {
                return b.ToString("x").TrimStart('0');
            }
        }

        public static async Task<HasJoinedResponse> HasPlayerJoined(string username, string hash)
        {
            var url = $"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={username}&serverId={hash}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var jsonReader = new JsonTextReader(streamReader);
            var jsonSerializer = new JsonSerializer();
            return jsonSerializer.Deserialize<HasJoinedResponse>(jsonReader);
        }

        public class HasJoinedResponse
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<ProfileProperty> Properties { get; set; }
        }

        public class ProfileProperty
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Signature { get; set; }
        }
    }
}