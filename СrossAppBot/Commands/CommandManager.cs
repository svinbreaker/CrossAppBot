using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public class CommandManager
    {
        public List<AbstractCommand> Commands = new List<AbstractCommand>();
        private Dictionary<Type, IArgumentParser> parsers = new Dictionary<Type, IArgumentParser>();

        public AbstractCommand CreateCommandInstance(string commandName, object[] arguments)
        {
            AbstractCommand command = GetCommandByName(commandName);

            if (arguments != null)
            {
                ParseCommandArguments(command, arguments);
            }

            return command;
        }

        public bool CommandExist(string commandName) 
        {
            return Commands.Where(c => c.Name == commandName).FirstOrDefault() != null; 
        }

        private void ParseCommandArguments(AbstractCommand command, object[] arguments)
        {
            List<PropertyInfo> properties = command.GetType().GetProperties().Where(property => property.IsDefined(typeof(CommandArgumentAttribute), false)).ToList();

            for (int i = 1; i < Math.Min(arguments.Length, properties.Count + 1); i++)
            {
                PropertyInfo property = properties[i - 1];
                object argument = arguments[i - 1];
                Type propertyType = property.PropertyType;

                if (!argument.GetType().Equals(propertyType))
                {
                    throw new InvalidCastException("Exception occured while parsing arguments types");
                }

                property.SetValue(command, argument);
            }
        }


        public AbstractCommand GetCommandByName(string commandName)
        {
            AbstractCommand command = Commands.Where(c => c.Name == commandName).FirstOrDefault();
            if (command == null)
            {
                throw new KeyNotFoundException($"Command '{commandName}' not exists");
            }

            return (AbstractCommand)Activator.CreateInstance(command.GetType());

        }

        public void AddCommand(AbstractCommand command)
        {
            Commands.Add(command);
        }

        public void RemoveCommand(AbstractCommand command)
        {
            Commands.Remove(command);
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
    }
}
