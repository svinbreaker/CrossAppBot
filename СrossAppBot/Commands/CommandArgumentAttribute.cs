using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{

    [AttributeUsage(AttributeTargets.Property)]
    public class CommandArgumentAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public bool Optional { get; }
        public bool IsAttachment { get; }

        public CommandArgumentAttribute(string name, string description, bool optional = false, bool isAttachment = false)
        {
            Name = name;
            Description = description;
            Optional = optional;
            this.IsAttachment = isAttachment;
        }
    }
}
