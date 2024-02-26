using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public class CommandCondition
    {
        private readonly Func<CommandContext, bool> condition;
        private readonly Func<CommandContext, Task> action;

        public CommandCondition(Func<CommandContext, bool> condition, Func<CommandContext, Task> action)
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public async Task<bool> ExecuteIfTrueAsync(CommandContext context)
        {
            if (condition(context))
            {
                await action(context);
                return true;
            }
            return false;
        }
    }
}
