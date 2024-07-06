using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public class CommandInfo
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<CommandArgument> Arguments { get; set; }

        public CommandInfo(Type type, string name, string description, List<CommandArgument> arguments) 
        {
            Type = type;
            Name = name;
            Description = description;
            Arguments = arguments;
        }
    }
}
