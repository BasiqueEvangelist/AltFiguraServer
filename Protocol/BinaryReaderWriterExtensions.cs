using System;
using System.IO;
using System.Text;

namespace AltFiguraServer.Protocol
{
    public static class BinaryReaderWriterExtensions
    {
        public static Guid ReadGuid(this BinaryReader br)
        {
            int length = br.ReadInt32();
            string idString = Encoding.UTF8.GetString(br.ReadBytes(length));
            return Guid.Parse(idString);
        }

        public static void Write(this BinaryWriter bw, Guid guid)
        {
            byte[] guidBytes = Encoding.UTF8.GetBytes(guid.ToString());
            bw.Write(guidBytes.Length);
            bw.Write(guidBytes);
        }
    }
}