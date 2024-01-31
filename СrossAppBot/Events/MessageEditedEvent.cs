using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Entities;

namespace СrossAppBot.Events
{
    public class MessageEditedEvent : AbstractClientEvent
    {
        public ChatMessage Message { get; set; }

        public MessageEditedEvent(ChatMessage message)
        {
            Message = message;
        }
    }
}
