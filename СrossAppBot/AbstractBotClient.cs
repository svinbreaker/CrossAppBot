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

        public EventManager EventManager;
        public CommandManager CommandManager { get; set; }

        public AbstractBotClient(string name, string token, EventManager eventManager = null, CommandManager commandManager = null)
        {
            Name = name;
            Token = token;

            if (eventManager == null) 
            {
                EventManager = new EventManager();
            }
            else 
            {
                EventManager = eventManager;
            }

            if (commandManager == null)
            {
                CommandManager = new CommandManager();
            }
            else
            {
                CommandManager = commandManager;
            }
        }

        public abstract Task StartAsync();
        public abstract Task SendMessageAsync(string channelId, string text = null, string messageReferenceId = null, List<string> files = null);

        public abstract string Mention(ChatUser user);
        public abstract string Mention(string userId);
        public abstract bool TextIsMention(ChatMessage message, string mention);

        public abstract Task<ChatUser> GetUserAsync(string userId, ChatGroup guild);
        public abstract Task<ChatUser> GetUserByMention(ChatMessage message, string mention);
        public abstract Task<ChatChannel> GetChannelAsync(string channeId);
        public abstract Task<ChatGroup> GetGuildAsync(string guildId);

        public abstract Task<List<UserRight>> GetUserRights(ChatUser user, ChatGroup guild);

        public override string ToString()
        {
            return $"Name: {Name}, Token: {Token}, Id: {Id}";
        }
    }
}
