using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Commands;
using СrossAppBot.Events;
using СrossAppBot.Events.Logging;

namespace СrossAppBot
{
    public class BotManager
    {
        List<AbstractBotClient> Bots { get; }
        public EventManager EventManager { get; }
        public EventLogger Logger { get; }
        public TextCommandProcessor TextCommandProcessor { get; }
        Dictionary<AbstractBotClient, List<AbstractCommand>> BotCommands { get; }
        Dictionary<AbstractBotClient, List<AbstractClientEvent>> BotEvents { get; }

        public BotManager AddBot<T>(string token, ulong? vkGroupId = null, string telegramConfigPath = null) where T : AbstractBotClient
        {
            T client = Activator.CreateInstance<T>();
            client.Token = token;

            if (client is VkBotClient vkBotClient) vkBotClient.GroupId = vkGroupId.Value;
            //if (client is TelegramBotClient telegramBotClient) telegramBotClient.configPath = telegramConfigPath;

            Bots.Add(client);
            return this;
        }

        public BotManager RegisterCommandForAllBots<TCommand>() where TCommand : AbstractCommand
        {
            foreach (AbstractBotClient bot in Bots)
            {
                RegisterCommand<TCommand>(bot);
            }
            return this;
        }

        public BotManager RegisterCommandForBot<TCommand, TBot>() where TCommand : AbstractCommand where TBot : AbstractBotClient
        {
            AbstractBotClient bot = Bots.OfType<TBot>().FirstOrDefault();
            if (bot != null)
            {
                RegisterCommand<TCommand>(bot);
            }
            return this;
        }

        public BotManager RegisterCommandForMultipleBots<TCommand, TBotExtension>() where TCommand : AbstractCommand where TBotExtension : IBotExtension
        {
            foreach (AbstractBotClient bot in Bots.Where(b => b is TBotExtension))
            {
                RegisterCommand<TCommand>(bot);
            }
            return this;
        }

        private void RegisterCommand<TCommand>(AbstractBotClient bot) where TCommand : AbstractCommand
        {
            var constructor = typeof(TCommand).GetConstructor(Type.EmptyTypes);

            if (constructor != null)
            {
                TCommand command = (TCommand)constructor.Invoke(null);
                BotCommands[bot].Add(command);
            }
            else
            {
                throw new InvalidOperationException($"No parameterless constructor found for type {typeof(TCommand)}");
            }
        }
    }
}
