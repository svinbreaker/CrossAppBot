using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    //This class should not be used to create a command (Use properties with CommandArgumentAttribute instead). It is only internal used to retrieve command arguments.
    public class CommandArgument
    {
        public Type Type { get; set; }
        public object Value { get; set; }
        public CommandArgumentAttribute Attributes { get; set; }

        public CommandArgument(Type type, object value, CommandArgumentAttribute attributes) 
        {
            Type = type;
            Value = value;
            Attributes = attributes;
        }

        public object Unpack() 
        {
            object obj = Activator.CreateInstance(Type);
            obj = Value;

            return obj;
        }
    }
}
