using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public abstract class AbstractCommandCondition
    {
        public abstract Task<bool> Condition();
        public abstract Task Action();
    }
}
