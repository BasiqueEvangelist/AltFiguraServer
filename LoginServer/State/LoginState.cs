using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AltFiguraServer.LoginServer.Chat;
using AltFiguraServer.LoginServer.Packets;

namespace AltFiguraServer.LoginServer.State
{
    public class LoginState : IMCState
    {
        private readonly MinecraftConnection connection;
        private string username;
        private int verifyToken;

        public LoginState(MinecraftConnection connection)
        {
            this.connection = connection;
        }

        public Dictionary<int, Func<IServerboundPacket>> PacketMap => new()
        {
            { 0, () => new LoginStartPacket() },
            { 1, () => new EncryptionResponsePacket() },
        };

        public async Task OnLoginStart(LoginStartPacket packet)
        {
            // await connection.WritePacket(new LoginDisconnectPacket()
            // {
            //     Reason = new TextChatComponent("Test message from C# server")
            // });

            username = packet.Username;
            verifyToken = RandomNumberGenerator.GetInt32(int.MaxValue);

            EncryptionRequestPacket erp = new()
            {
                ServerID = "",
                PrivateKey = AuthUtils.ServerKey.ExportSubjectPublicKeyInfo(),
                VerifyToken = BitConverter.GetBytes(verifyToken)
            };

            await connection.WritePacket(erp);
        }

        public async Task OnEncryptionResponse(EncryptionResponsePacket packet)
        {
            byte[] decryptedVerifyToken = AuthUtils.ServerKey.Decrypt(packet.VerifyToken, RSAEncryptionPadding.Pkcs1);
            if (!decryptedVerifyToken.SequenceEqual(BitConverter.GetBytes(verifyToken)))
            {
                await connection.WritePacket(new LoginDisconnectPacket()
                {
                    Reason = new TextChatComponent("Invalid verify token")
                });
                return;
            }

            var decryptedSharedSecret = AuthUtils.ServerKey.Decrypt(packet.SharedSecret, RSAEncryptionPadding.Pkcs1);
            connection.SetupEncryption(decryptedSharedSecret);
            var publicKeyBytes = AuthUtils.ServerKey.ExportSubjectPublicKeyInfo();
            // This is concern.
            // TODO: fix
            string hash = AuthUtils.MinecraftShaDigest(decryptedSharedSecret.Concat(publicKeyBytes).ToArray());
            var response = await AuthUtils.HasPlayerJoined(username, hash);
            if (response == null)
            {
                await connection.WritePacket(new LoginDisconnectPacket()
                {
                    Reason = new TextChatComponent("Authentication failed")
                });
                return;
            }

            var token = SessionUtils.MintToken(Guid.Parse(response.Id));

            var disconnectMsg = new TextChatComponent("This is the Figura Auth Server V2.0!\n") { Color = "aqua" };
            disconnectMsg.Siblings.Add(new TextChatComponent("Here is your auth token.\n\n\n") { Color = "aqua" });
            disconnectMsg.Siblings.Add(new TextChatComponent(token) { Color = "aqua", Obfuscated = true });
            disconnectMsg.Siblings.Add(new TextChatComponent("(Just kidding! :D)") { Color = "aqua" });

            await connection.WritePacket(new LoginDisconnectPacket()
            {
                Reason = disconnectMsg
            });
            return;
        }
    }
}