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

        public List<UserRight> GetRights(ChatGroup guild)
        {
            if (guild == null)
            {
                return null;
            }
            else
            {
                return Client.GetUserRights(this, guild).Result;
            }
        }
        public async Task<List<UserRight>> GetRightsAsync(ChatGroup guild)
        {
            if (guild == null)
            {
                return null;
            }
            else
            {
                return await Client.GetUserRights(this, guild);
            }
        }
    }
}
