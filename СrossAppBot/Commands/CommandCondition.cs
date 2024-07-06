using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public class CommandCondition
    {
        private readonly Func<bool> condition;
        private readonly Func<Task> action;

        public CommandContext Context { get; set; }

        public CommandCondition(Func<bool> condition, Func<Task> action, CommandContext context)
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            Context = context;
        }

        public async Task<bool> ExecuteIfFalseAsync()
        {
            if (!condition())
            {
                await action();
                return true;
            }
            return false;
        }
    }
}
