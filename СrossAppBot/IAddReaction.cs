using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Entities;

namespace СrossAppBot
{
    public interface IAddReaction : IBotExtension
    {
        public Task AddReaction(ChatMessage message, string reaction); 
    }
}
