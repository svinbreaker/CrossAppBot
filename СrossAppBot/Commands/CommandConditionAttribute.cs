using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandConditionAttribute : Attribute
    {
        public Type Condition { get; set; }
        public string Name { get; set; }

        public CommandConditionAttribute(Type condition, string name)
        {
            Condition = condition;
            Name = name;
        }
    }
}
