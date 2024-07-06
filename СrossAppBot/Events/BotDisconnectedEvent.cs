using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Events
{
    public class BotDisconnectedEvent : AbstractClientEvent
    {
        public AbstractBotClient Client { get; }
        public BotDisconnectedEvent(AbstractBotClient client)
        {
            Client = client;
        }
    }
}
