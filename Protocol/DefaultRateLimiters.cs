using System;
using System.Threading;

namespace AltFiguraServer.Protocol
{
    public static class DefaultRateLimiters
    {
        public static RateLimiter BytesSent { get; } = new(1024 * 200, 1024 * 20);

        public static RateLimiter MessagesSent { get; } = new(2048, 256);

        public static RateLimiter AvatarsUploaded { get; } = new(4, 1);

        public static RateLimiter AvatarsRequested { get; } = new(2048, 1);

        public static RateLimiter PingBytes { get; } = new(2048, 1024);

        public static RateLimiter Pings { get; } = new(21, 21);

        private static readonly Timer gcTimer = new(_ =>
        {
            BytesSent.CollectExpired();
            MessagesSent.CollectExpired();
            AvatarsUploaded.CollectExpired();
            AvatarsRequested.CollectExpired();
            PingBytes.CollectExpired();
            Pings.CollectExpired();
        }, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
    }
}