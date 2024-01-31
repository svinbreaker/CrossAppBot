using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using СrossAppBot.Commands;
using СrossAppBot.Entities;
using СrossAppBot.Entities.Files;
using СrossAppBot.Events;

namespace СrossAppBot
{
    public abstract class AbstractBotClient
    {
        public string Name { get; }

        public string Token { get; set; }
        public string Id { get; set; }

        private EventManager eventManager = new EventManager();

        public EventManager EventManager
        {
            get
            {
                return eventManager;
            }
        }

        public AbstractBotClient(string name, string token)
        {
            Name = name;
            Token = token;
        }

        public TextCommandProcessor TextCommandProcessor { get; set; }
        public abstract Task StartAsync();
        public abstract Task SendMessageAsync(string channelId, string text = null, string messageReferenceId = null, List<string> files = null);


        public abstract string Mention(ChatUser user);
        public abstract string Mention(string userId);
        public abstract bool TextIsMention(ChatMessage message, string mention);

        public abstract Task<ChatUser> GetUserAsync(string userId, ChatGuild guild);
        public abstract Task<ChatUser> GetUserByMention(ChatMessage message, string mention);
        public abstract Task<ChatChannel> GetChannelAsync(string channeId);
        public abstract Task<ChatGuild> GetGuildAsync(string guildId);

    }
}
