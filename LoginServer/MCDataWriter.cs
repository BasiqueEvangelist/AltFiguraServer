using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace AltFiguraServer.LoginServer
{
    public class MCDataWriter : IDisposable
    {
        private readonly BinaryWriter bw;
        private bool disposedValue;

        public MCDataWriter(BinaryWriter bw)
        {
            this.bw = bw;
        }

        public void Write(ushort value)
        {
            Span<byte> data = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(data, value);
            bw.Write(data);
        }

        public void Write(long value)
        {
            Span<byte> data = stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(data, value);
            bw.Write(data);
        }

        public void WriteVarInt32(int value)
        {
            uint current = (uint)value;
            do
            {
                byte writtenByte = (byte)(current & 0b01111111);
                current >>= 7;
                if (current != 0) writtenByte |= 0b10000000;
                bw.Write(writtenByte);
            } while (current != 0);
        }

        public void Write(string value)
        {
            WriteVarInt32(Encoding.UTF8.GetByteCount(value));
            bw.Write(Encoding.UTF8.GetBytes(value));
        }
        public void Write(ReadOnlySpan<byte> value)
        {
            bw.Write(value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    bw.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MCDataWriter()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}