using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Entities.Files
{
    public class ChatAudio : ChatMessageFile
    {
        public string Url { get; set; }

        public ChatAudio(string Url) : base(FileType.Audio)
        {
            this.Url = Url;           
        }
    }
}
