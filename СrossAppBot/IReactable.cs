﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot
{
    public interface IEmojiable : IBotExtension
    {
        public bool IsReactableEmoji(string content);
    }
}
