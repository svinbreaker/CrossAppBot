using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Entities
{
    public class ChatUser
    {
        public string Id { get; set; }
        public AbstractBotClient Client { get; set; }
        public string Name { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAdmin { get; set; }
        public object OriginalObject { get; set; }

        public ChatUser(string id, string name, AbstractBotClient client, object originalObject, bool isOwner = false, bool isAdmin = false)
        {
            Id = id;
            Client = client;
            Name = name;
            OriginalObject = originalObject;
            IsOwner = isOwner;
            IsAdmin = isAdmin;
        }
    }
}
