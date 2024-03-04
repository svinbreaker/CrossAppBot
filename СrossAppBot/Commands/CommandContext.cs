using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Entities;

namespace СrossAppBot.Commands
{
    public class CommandContext
    {
        public ChatUser Sender { get; set; }
        public ChatGuild Guild { get; set; }
        public ChatChannel Channel { get; set; }
        public AbstractBotClient Client { get; set; }
        public ChatMessage Message { get; set; }

        public CommandContext(
        ChatUser sender = null,
        ChatGuild guild = null,
        ChatChannel channel = null,
        AbstractBotClient client = null,
        ChatMessage message = null)
        {
            Sender = sender;
            Guild = guild;
            Channel = channel;
            Client = client;
            Message = message;
        }
    }
}
