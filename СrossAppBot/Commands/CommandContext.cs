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
    }
}
