using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Events
{
    public class BotConnectedEvent : AbstractClientEvent
    {
        public AbstractBotClient Client { get; }
        public BotConnectedEvent(AbstractBotClient client) 
        {
            Client = client;
        }
    }
}
