using Newtonsoft.Json.Linq;

namespace AltFiguraServer.LoginServer.Chat
{
    public class TextChatComponent : ChatComponent
    {
        public string Text { get; set; } = "";

        public TextChatComponent(string text)
        {
            Text = text;
        }

        protected override JObject SerializeSelf()
        {
            JObject obj = new();
            obj["text"] = Text;
            return obj;
        }
    }
}