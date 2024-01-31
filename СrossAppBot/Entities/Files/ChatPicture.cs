using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Entities.Files
{
    public class ChatPicture : ChatMessageFile
    {
        public string Url { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public ChatPicture(string Url, int Width, int Height) : base(FileType.Picture)
        {
            this.Url = Url;
            this.Width = Width;
            this.Height = Height;
        }
    }
}
