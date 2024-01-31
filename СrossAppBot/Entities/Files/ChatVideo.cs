using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Entities.Files
{
    public class ChatVideo : ChatMessageFile
    {
        public string Url { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public ChatVideo(string Url, int Width, int Height) : base(FileType.Video)
        {
            this.Url = Url;
            this.Width = Width;
            this.Height = Height;
        }
    }
}
