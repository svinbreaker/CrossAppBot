using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Commands
{
    public class CommandManager
    {
        public List<AbstractCommand> Commands = new List<AbstractCommand>();
        public TextCommandHelper TextCommandHelper { get; set; }


        public AbstractCommand CreateExecutableCommandInstance(string commandName, object[] arguments, CommandContext context)
        {
            AbstractCommand command = GetCommandByName(commandName);

            command.Context = context;
            if (arguments != null)
            {
                try
                {
                    ParseCommandArguments(command, arguments);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " " + e.ToString());
                }
            }
            command.Conditions();

            return command;
        }

        public bool CommandExist(string commandName)
        {
            return Commands.Where(c => c.Name == commandName).FirstOrDefault() != null;
        }

        private void ParseCommandArguments(AbstractCommand command, object[] arguments)
        {
            List<PropertyInfo> properties = command.GetType().GetProperties().Where(property => property.IsDefined(typeof(CommandArgumentAttribute), false)).ToList();

            for (int i = 1; i < Math.Min(arguments.Length, properties.Count) + 1; i++)
            {
                PropertyInfo property = properties[i - 1];
                object argument = arguments[i - 1];
                if (argument == null)
                {
                    return;
                }

                Type propertyType = property.PropertyType;
                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                if (!argument.GetType().Equals(propertyType))
                {
                    throw new InvalidCastException("Exception occured while parsing arguments types");
                }
                //tert
                property.SetValue(command, argument);
            }
        }


        public AbstractCommand GetCommandByName(string commandName)
        {
            AbstractCommand command = Commands.Where(c => c.Name == commandName).FirstOrDefault();
            if (command == null)
            {
                return null;
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

        public void SetTextCommandHelper(string prefix, List<IArgumentParser> parsers)
        {
            TextCommandHelper = new TextCommandHelper(prefix, this, parsers);
        }
    }
}
