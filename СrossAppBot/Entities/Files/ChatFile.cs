using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Entities.Files
{
    public class ChatFile : ChatMessageFile
    {
        public string Url { get; set; }

        public ChatFile(string Url) : base(FileType.File)
        {
            this.Url = Url;
        }
    }
}
