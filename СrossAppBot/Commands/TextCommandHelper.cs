using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public class TextCommandHelper
    {
        public string Prefix { get; set; }
        private CommandManager CommandManager { get; set; }
        public Dictionary<Type, IArgumentParser> Parsers { get; set; }
        private string[] _splitters = new string[2];

        public TextCommandHelper(string prefix, CommandManager commandManager, List<IArgumentParser> parsers = null)
        {
            Prefix = prefix;
            CommandManager = commandManager;
            Parsers = DefaultArgumentParsers.Get;

            _splitters[0] = " ";
            _splitters[1] = prefix;

            if (parsers != null)
            {
                foreach (IArgumentParser parser in parsers)
                {
                    Parsers.Add(parser.Type, parser);
                }
            }
        }

        public bool StringIsTextCommand(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.StartsWith(Prefix))
            {
                return false;
            }

            string[] commandArgs = input.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);
            return (commandArgs.Length > 0 & CommandManager.GetCommandByName(commandArgs[0]) != null);
        }
        public AbstractCommand CreateCommandInstance(string input, CommandContext context)
        {
            if (input == null) return null;

            string[] commandArgs = input.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);

            if (!input.StartsWith(Prefix) | (commandArgs.Length < 1))
            {
                return null;
            }

            string commandName = commandArgs[0];
            AbstractCommand command = CommandManager.GetCommandByName(commandName);
            if (command == null)
            {
                return null;
            }

            List<PropertyInfo> properties = command.GetType().GetProperties().Where(property => property.IsDefined(typeof(CommandArgumentAttribute), false)).ToList();
            object[] arguments = new object[properties.Count];
            arguments = ParseArguments(commandArgs, command, context);

            return CommandManager.CreateExecutableCommandInstance(commandName, arguments, context);
        }

        private object[] ParseArguments(string[] commandArgs, AbstractCommand command, CommandContext context)
        {
            List<PropertyInfo> properties = command.GetType().GetProperties().Where(property => property.IsDefined(typeof(CommandArgumentAttribute), false)).ToList();
            object[] parsedArguments = new object[properties.Count];

            for (int i = 1; i < Math.Min(commandArgs.Length, properties.Count + 1); i++)
            {
                string argValue = commandArgs[i];
                PropertyInfo property = properties[i - 1];

                Type propertyType = property.PropertyType;
                if (Parsers.ContainsKey(propertyType))
                {
                    IArgumentParser parser = Parsers[propertyType];
                    parsedArguments[i - 1] = parser.Parse(argValue, context);
                }
            }
            return parsedArguments;
        }
    }
}
