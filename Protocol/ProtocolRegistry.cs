using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AltFiguraServer.Protocol.Packets;

namespace AltFiguraServer.Protocol
{
    public class ProtocolRegistry
    {
        private readonly Dictionary<string, sbyte> remoteMap = new();

        public bool RemoteSupportsMessage(string protocolName) => remoteMap.ContainsKey(protocolName);

        public sbyte GetRemoteId(string protocolName) => remoteMap[protocolName];

        public void ReadFrom(BinaryReader br)
        {
            int messageCount = br.ReadInt32();
            sbyte current = sbyte.MinValue + 1;
            for (int i = 0; i < messageCount; i++)
            {
                string protocolName = Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32()));
                remoteMap[protocolName] = current;
                current++;
            }
        }
    }
}