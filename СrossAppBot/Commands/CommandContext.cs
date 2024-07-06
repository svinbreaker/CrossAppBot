using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace СrossAppBot.Commands
{
    public class CommandContext
    {
        public ChatUser Sender { get; set; }
        public ChatGroup ChatGroup { get; set; }
        public ChatChannel Channel { get; set; }
        public AbstractBotClient Client { get; set; }
        public ChatMessage Message { get; set; }
        private Func<string, bool, List<string>, Task> AnswerMethod { get; set; }

        public CommandContext(
            Func<string, bool, List<string>, Task> answerMethod,
        ChatUser sender = null,
        ChatGroup guild = null,
        ChatChannel channel = null,
        AbstractBotClient client = null,
        ChatMessage message = null)
        {
            Sender = sender;
            ChatGroup = guild;
            Channel = channel;
            Client = client;
            Message = message;
            AnswerMethod = answerMethod;
        }

        public static CommandContext FromMessage(ChatMessage message)
        {
            return new CommandContext(
               async (text, reply, files) =>
            {
                string messageReferenceId = null;
                if (reply)
                {
                    messageReferenceId = message.Id;
                }
                await message.Client.SendMessageAsync(message.Channel.Id, text, messageReferenceId, files);
            })
            {
                Channel = message.Channel,
                Client = message.Client,
                ChatGroup = message.Guild,
                Message = message,
                Sender = message.Author,

            };
        }

        public async Task AnswerAsync(string text, List<string> files, bool reply)
        {
            Console.WriteLine("a");
            await AnswerMethod.Invoke(text, reply, files);
        }
    }
}
