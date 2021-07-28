using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;
using Microsoft.Extensions.Logging;

namespace AltFiguraServer.LoginServer
{
    public class MinecraftConnection
    {
        private readonly TcpClient client;
        private readonly ILogger<MinecraftConnection> logger;
        private readonly NetworkStream stream;
        private ICryptoTransform encryptor = null;

        public IMCState CurrentState { get; set; }

        public MinecraftConnection(TcpClient client, ILogger<MinecraftConnection> logger)
        {
            this.client = client;
            this.logger = logger;
            stream = client.GetStream();
            CurrentState = new HandshakeState(this);
        }

        public void SetupEncryption(byte[] key)
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CFB;
            encryptor = aes.CreateEncryptor(key, key);
        }

        public async Task Run()
        {
            try
            {
                while (true)
                {
                    await ReadPacket();
                }
            }
            catch (EndOfStreamException) { }
            catch (Exception e)
            {
                logger.LogError(e, "Encountered network error");
            }
            finally
            {
                client.Close();
            }
        }

        public async Task WritePacket(IMinecraftS2CPacket packet)
        {
            // TODO: improve method
            byte[] data;
            using (MemoryStream ms = new())
            using (MCDataWriter mw = new(new(ms)))
            {
                packet.Write(mw);
                data = ms.ToArray();
            }

            byte[] packetIdBuf;
            using (MemoryStream ms = new())
            using (MCDataWriter mw = new(new(ms)))
            {
                mw.WriteVarInt32(packet.PacketID);
                packetIdBuf = ms.ToArray();
            }

            byte[] sizeBuf;
            using (MemoryStream ms = new())
            using (MCDataWriter mw = new(new(ms)))
            {
                mw.WriteVarInt32(data.Length + packetIdBuf.Length);
                sizeBuf = ms.ToArray();
            }

            byte[] finalBuf = new byte[data.Length + packetIdBuf.Length + sizeBuf.Length];
            sizeBuf.CopyTo(finalBuf.AsSpan());
            packetIdBuf.CopyTo(finalBuf.AsSpan(sizeBuf.Length));
            data.CopyTo(finalBuf.AsSpan(packetIdBuf.Length + sizeBuf.Length));

            if (encryptor != null)
                await stream.WriteAsync(encryptor.TransformFinalBlock(finalBuf, 0, finalBuf.Length));
            else
                await stream.WriteAsync(finalBuf.AsMemory());
        }

        private async Task ReadPacket()
        {
            var (packetLength, _) = await ReadVarInt32(stream);
            var (packetId, packetIdLen) = await ReadVarInt32(stream);

            byte[] packetData = new byte[packetLength - packetIdLen];
            await stream.ReadAsync(packetData.AsMemory());

            if (!CurrentState.PacketMap.TryGetValue(packetId, out var packetFactory))
            {
                logger.LogError("Invalid Packet ID " + packetId);
                return;
            }
            var packet = packetFactory();

            logger.LogDebug("Reading packet " + packet);

            using (MemoryStream ms = new(packetData))
            using (MCDataReader mr = new(new(ms)))
                packet.Read(mr);

            try
            {
                await packet.Handle(CurrentState);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception was thrown while handling packet " + packet.GetType().Name);
                throw;
            }
        }

        private static async Task<(int, int)> ReadVarInt32(Stream stream)
        {
            int length = 0;
            int value = 0;
            int offset = 0;
            byte[] readByteArr = new byte[1];
            while (true)
            {
                int readData = await stream.ReadAsync(new Memory<byte>(readByteArr));
                length++;
                if (readData == 0)
                    throw new EndOfStreamException();

                value |= (readByteArr[0] & 0b01111111) << offset;
                offset += 7;
                if ((readByteArr[0] & 0b10000000) == 0) break;
            }
            return (value, length);
        }

    }
}