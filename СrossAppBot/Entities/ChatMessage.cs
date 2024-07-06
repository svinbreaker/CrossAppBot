﻿using Discord.Commands;
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
        public ChatGroup Guild { get; set; }
        public ChatChannel Channel { get; set; }
        public ChatMessage MessageReference { get; set; }

        public AbstractBotClient Client { get; set; }

        public List<ChatMessageFile> Files { get; set; }
        public object OriginalObject { get; set; }

        public ChatMessage(string id, ChatUser author, ChatGroup guild, AbstractBotClient client, ChatChannel channel, object originalObject, string text = null, ChatMessage messageReference = null, List<ChatMessageFile> files = null)
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

        public List<string> GetMentions()
        {
            List<string> mentions = new List<string>();

            if (!string.IsNullOrEmpty(Text))
            {
                foreach (string word in Text.Split(" "))
                {
                    if (Client.TextIsMention(this, word))
                    {
                        mentions.Add(word);
                    }
                }
            }
       
            return mentions;
        }

        public override string ToString()
        {
            return $"Client: {Client.Name}, Id: {Id}, Text: {Text}, AuthorId: {Author.Id}, AuthorName: {Author.Name}, GuildId: {Guild.Id}, ChannelId: {Channel.Id}, MessageReferanceId: {MessageReference}, Files: {Files.Count}";
        }
    }
}
