using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Commands;

namespace СrossAppBot.Events
{
    public class CommandExecutedEvent : AbstractClientEvent
    {
        public AbstractCommand Command { get; }
        public CommandContext Context { get; }
        public CommandExecutedEvent(AbstractCommand command, CommandContext context)
        {
            Command = command;
            Context = context;
        }
    }
}
