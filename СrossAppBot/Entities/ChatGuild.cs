using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;

namespace СrossAppBot.Entities
{
    public class ChatGuild
    {
        public string Id { get; set; }

        public AbstractBotClient Client { get; set; }
        public string Name { get; set; }

        public object OriginalObject { get; set; }

        public ChatGuild(string id, AbstractBotClient client, string name, object originalObject)
        {
            Id = id;
            Client = client;
            Name = name;
            OriginalObject = originalObject;
        }

        public List<User> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}
