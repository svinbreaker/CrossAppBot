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
        public bool IsPrivate { get; set; }

        public object OriginalObject { get; set; }

        public ChatChannel(string id, string name, bool isPrivate, object originalObject, ChatGuild guild = null)
        {
            Id = id;
            Name = name;
            Guild = guild;
            IsPrivate = isPrivate;
            OriginalObject = originalObject;
        }
    }
}
