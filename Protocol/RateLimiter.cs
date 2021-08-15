using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AltFiguraServer.Protocol
{
    public class RateLimiter
    {
        private readonly ConcurrentDictionary<Guid, DateTime> limits = new();
        private readonly int maximumTokenDebt;
        private readonly int tokensPerSecond;
        private readonly ReaderWriterLockSlim gcLock = new();

        public RateLimiter(int maximumTokenDebt, int tokensPerSecond)
        {
            this.maximumTokenDebt = maximumTokenDebt;
            this.tokensPerSecond = tokensPerSecond;
        }

        public bool TryPerform(Guid playerId, int tokens)
        {
            gcLock.EnterReadLock();
            try
            {
                var maxDebt = DateTime.Now + TimeSpan.FromSeconds((double)maximumTokenDebt / tokensPerSecond);
                bool failed = false;

                var debt = limits.AddOrUpdate(playerId,
                 _ =>
                 {
                     var calculated = DateTime.Now + TimeSpan.FromSeconds((double)tokens / tokensPerSecond);
                     if (calculated > maxDebt)
                     {
                         failed = true;
                         return DateTime.Now;
                     }
                     else
                         return calculated;
                 },
                 (_, old) =>
                 {
                     var calculated = old < DateTime.Now
                        ? DateTime.Now + TimeSpan.FromSeconds((double)tokens / tokensPerSecond)
                        : old + TimeSpan.FromSeconds((double)tokens / tokensPerSecond);
                     if (calculated > maxDebt)
                     {
                         failed = true;
                         return old;
                     }
                     else
                         return calculated;
                 });

                return !failed;
            }
            finally
            {
                gcLock.ExitReadLock();
            }
        }

        public void CollectExpired()
        {
            gcLock.EnterWriteLock();
            try
            {
                foreach (var pair in limits)
                {
                    if (pair.Value < DateTime.Now)
                    {
                        ((ICollection<KeyValuePair<Guid, DateTime>>)limits).Remove(pair);
                    }
                }
            }
            finally
            {
                gcLock.ExitWriteLock();
            }
        }
    }
}