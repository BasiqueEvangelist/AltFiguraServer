using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AltFiguraServer.Protocol.Packets
{
    public class UserEventUnsubscribeC2SPacket : IFiguraC2SPacket
    {
        public Guid[] Targets { get; set; }

        public void Read(BinaryReader br)
        {
            int total = br.ReadInt32();

            Targets = new Guid[total];
            for (int i = 0; i < total; i++)
            {
                Targets[i] = br.ReadGuid();
            }
        }

        public Task Handle(IFiguraState state)
        {
            return (state as FiguraState).OnUserEventUnsubscribe(this);
        }
    }
}