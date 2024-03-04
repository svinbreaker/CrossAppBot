using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Commands;

namespace СrossAppBot
{
    public interface ISlashable
    {
        public abstract Task RegisterSlashCommand(AbstractCommand command);
        public async Task RegisterSlashCommands(List<AbstractCommand> commandsd) 
        {
            foreach(AbstractCommand command in commandsd) 
            {
                await this.RegisterSlashCommand(command);
            }
        }
        public abstract Task ExecuteSlashCommand(AbstractCommand command, CommandContext context);
    }
}
