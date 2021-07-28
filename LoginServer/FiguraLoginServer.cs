using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AltFiguraServer.LoginServer
{
    public class FiguraLoginServer : BackgroundService
    {
        private readonly TcpListener listener = new(IPAddress.Any, 25565);
        private readonly ILogger<MinecraftConnection> connLogger;

        public FiguraLoginServer(ILogger<MinecraftConnection> connLogger)
        {
            this.connLogger = connLogger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            listener.Start();
            try
            {
                using (stoppingToken.Register(() => listener.Stop()))
                {
                    while (true)
                    {
                        TcpClient client = await listener.AcceptTcpClientAsync();
                        MinecraftConnection connection = new(client, connLogger);
                        _ = connection.Run();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}