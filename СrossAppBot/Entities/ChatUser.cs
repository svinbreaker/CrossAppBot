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
        public object OriginalObject { get; set; }

        public ChatUser(string id, string name, AbstractBotClient client, object originalObject)
        {
            Id = id;
            Client = client;
            Name = name;
            OriginalObject = originalObject;
        }

        public List<UserRight> GetRights(ChatGuild guild) 
        {
            return Client.GetUserRights(this, guild).Result;
        }
        public async Task<List<UserRight>> GetRightsAsync(ChatGuild guild)
        {
            return await Client.GetUserRights(this, guild);
        }
    }
}
