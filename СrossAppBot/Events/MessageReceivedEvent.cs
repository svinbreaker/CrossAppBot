using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Entities;

namespace СrossAppBot.Events
{
    public class MessageReceivedEvent : AbstractClientEvent
    {
        public ChatMessage Message { get; set; }

        public MessageReceivedEvent(ChatMessage message)
        {
            Message = message;
        }
    }
}
