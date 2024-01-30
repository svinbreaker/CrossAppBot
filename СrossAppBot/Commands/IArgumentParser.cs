using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public abstract class IArgumentParser
    {
        public Type Type { get; }

        public IArgumentParser(Type type) 
        {
            this.Type = type;
        }
        public abstract object Parse(string value, CommandContext context = null);
    }
}

