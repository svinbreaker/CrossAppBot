using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;
using static СrossAppBot.Commands.DefaultArgumentParsers;

namespace СrossAppBot.Commands
{
    public class TextCommandProcessor
    {
        private Dictionary<Type, IArgumentParser> parsers = new Dictionary<Type, IArgumentParser>();
        public string Prefix { get; set; }
        public Dictionary<string, AbstractCommand> commands = new Dictionary<string, AbstractCommand>();

        private string[] _splitters = new string[2];

        public TextCommandProcessor(string prefix, List<AbstractCommand> commandTypes, List<IArgumentParser> parsers = null)
        {
            this.Prefix = prefix;

            if (commandTypes == null) 
            {
                return;
            }

            foreach(AbstractCommand command in commandTypes) 
            {
                commands.Add(command.Name, command);
            }

            this.parsers = DefaultArgumentParsers.Get;
            if (parsers != null)
            {
                foreach (IArgumentParser parser in parsers)
                {
                    this.parsers.Add(parser.Type, parser);
                }
            }

            _splitters[0] = prefix;
            _splitters[1] = " ";
        }

        public async Task ProcessCommand(string input, CommandContext context = null)
        {
            if (input == null) return;

            string[] commandArgs = input.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);

            if (!input.StartsWith(Prefix) | (commandArgs.Length < 1)) 
            {
                return;
            }         

            string commandName = commandArgs[0];
            AbstractCommand command = await GetCommandInstance(commandName);

            if (command == null)
            {
                return;
            }

            await ParseArguments(commandArgs, command, context);
            await command.Execute(context); 
        }

        public void AddCommand(AbstractCommand command) 
        {
            commands.Add(command.Name, command);
        }

        public void RemoveCommand(AbstractCommand command)
        {
            commands.Remove(command.Name);
        }

        public void AddCommands(IEnumerable<AbstractCommand> commands)
        {
            foreach (AbstractCommand command in commands)
            {
                AddCommand(command);
            }
        }

        public void RemoveCommands(IEnumerable<AbstractCommand> commands) 
        {
            foreach (AbstractCommand command in commands)
            {
                RemoveCommand(command);
            }
        }
 
        private async Task<AbstractCommand> GetCommandInstance(string commandName)
        {
            AbstractCommand commandType = commands[commandName];
            return await Task.FromResult(commandType == null ? null : (AbstractCommand)Activator.CreateInstance(commandType.GetType()));
        }

        private async Task ParseArguments(string[] commandArgs, AbstractCommand command, CommandContext context)
        {
            List<PropertyInfo> properties = command.GetType().GetProperties().Where(property => property.IsDefined(typeof(CommandArgumentAttribute), false)).ToList();

            for (int i = 1; i < Math.Min(commandArgs.Length, properties.Count + 1); i++)
            {
                string argValue = commandArgs[i]; 
                PropertyInfo property = properties[i - 1]; 

               

                Type propertyType = property.PropertyType;
                object parsedValue = null;
                if (parsers.ContainsKey(propertyType))
                {
                    IArgumentParser parser = parsers[propertyType];
                    parsedValue = parser.Parse(argValue, context);
                }
          

                await Task.Run(() => property.SetValue(command, parsedValue));
            }
        }
    }
}

