using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AltFiguraServer.LoginServer.Chat
{
    public abstract class ChatComponent
    {
        private readonly List<ChatComponent> siblings = new();

        public IList<ChatComponent> Siblings => siblings;

        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public bool? Underlined { get; set; }
        public bool? Strikethrough { get; set; }
        public bool? Obfuscated { get; set; }

        public string Color { get; set; }


        public JObject Serialize()
        {
            JObject obj = SerializeSelf();

            if (Bold != null)
                obj["bold"] = Bold.Value;
            if (Italic != null)
                obj["italic"] = Italic.Value;
            if (Underlined != null)
                obj["underlined"] = Underlined.Value;
            if (Strikethrough != null)
                obj["strikethrough"] = Strikethrough.Value;
            if (Obfuscated != null)
                obj["obfuscated"] = Obfuscated.Value;

            if (Color != null)
                obj["color"] = Color;

            if (siblings.Count > 0)
            {
                JArray array = new();
                foreach (var sibling in siblings)
                {
                    array.Add(sibling.Serialize());
                }
                obj["extra"] = array;
            }

            return obj;
        }

        protected abstract JObject SerializeSelf();
    }
}