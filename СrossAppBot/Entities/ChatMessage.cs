using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using СrossAppBot.Commands;
using СrossAppBot.Entities.Files;
using CommandContext = СrossAppBot.Commands.CommandContext;

namespace СrossAppBot.Entities
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public ChatUser Author { get; set; }
        public ChatGuild Guild { get; set; }
        public ChatChannel Channel { get; set; }
        public ChatMessage MessageReference { get; set; }

        public AbstractBotClient Client { get; set; }

        public List<ChatMessageFile> Files {get; set; }
        public object OriginalObject { get; set; }

        public ChatMessage(string id, ChatUser author, ChatGuild guild, AbstractBotClient client, ChatChannel channel, object originalObject, string text = null, ChatMessage messageReference = null, List<ChatMessageFile> files = null)
        {
            Id = id;
            Text = text;
            Author = author;
            Guild = guild;
            Channel = channel;
            MessageReference = messageReference;
            Client = client;
            Files = files;
            OriginalObject = originalObject;
        }

        public CommandContext GetAsCommandContext() 
        {
            return new CommandContext() { Channel = this.Channel, Client = Client, Guild = this.Guild, Message = this, Sender = Author };
        }

        public override string ToString()
        {
            return $"Client: {Client.Name}, Id: {Id}, Text: {Text}, AuthorId: {Author.Id}, AuthorName: {Author.Name}, GuildId: {Guild.Id}, ChannelId: {Channel.Id}, MessageReferanceId: {MessageReference}, Files: {Files.Count}";
        }
    }
}
