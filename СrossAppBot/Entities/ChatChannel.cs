using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Entities
{
    public class ChatChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ChatGuild Guild { get; set;}

        public object OriginalObject { get; set; }

        public ChatChannel(string id, string name, ChatGuild guild, object originalObject)
        {
            Id = id;
            Name = name;
            Guild = guild;
            OriginalObject = originalObject;
        }
    }
}
