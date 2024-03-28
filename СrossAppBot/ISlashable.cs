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
        public virtual async Task RegisterSlashCommands(List<AbstractCommand> commands) 
        {
            foreach(AbstractCommand command in commands) 
            {
                await this.RegisterSlashCommand(command);
            }
        }

        public abstract Task ExecuteSlashCommand(AbstractCommand command);
    }
}
