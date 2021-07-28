using System;
using System.Collections;
using System.IO;
using System.Text;

namespace AltFiguraServer.LoginServer
{
    public class MCDataReader : IDisposable
    {
        private readonly BinaryReader br;
        private bool disposedValue;

        public MCDataReader(BinaryReader br)
        {
            this.br = br;
        }

        public ushort ReadUInt16()
        {
            ushort value = br.ReadUInt16();
            if (BitConverter.IsLittleEndian)
                return (ushort)(((value & 0xFF) << 8) | (value >> 8));
            else
                return value;
        }

        public int ReadVarInt32()
        {
            int value = 0;
            int offset = 0;
            while (true)
            {
                byte readByte = br.ReadByte();
                value |= (readByte & 0b01111111) << offset;
                offset += 7;
                if ((readByte & 0b10000000) == 0) break;
            }
            return value;
        }

        public string ReadString()
        {
            int size = ReadVarInt32();
            return Encoding.UTF8.GetString(br.ReadBytes(size));
        }

        public byte[] ReadBytes(int length)
        {
            return br.ReadBytes(length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    br.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MCDataReader()
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