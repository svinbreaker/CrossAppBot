using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Entities.Files
{
    public class ChatMessageFile
    {
        FileType Type { get; }
        
        public ChatMessageFile(FileType type)
        {
            Type = type;
        }
    }
}
