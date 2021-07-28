using System.Threading.Tasks;
using AltFiguraServer.LoginServer.State;

namespace AltFiguraServer.LoginServer.Packets
{
    public class LoginStartPacket : IServerboundPacket
    {
        public string Username { get; private set; }

        public void Read(MCDataReader mr)
        {
            Username = mr.ReadString();
        }

        public Task Handle(IMCState state)
        {
            return (state as LoginState).OnLoginStart(this);
        }
    }
}