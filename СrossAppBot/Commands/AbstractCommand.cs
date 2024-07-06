using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Commands.Parameters;
using СrossAppBot.Entities;

namespace СrossAppBot.Commands
{
    public abstract class AbstractCommand
    {
        public CommandContext Context { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        private Dictionary<Func<Task<bool>>, Func<Task>> CommandConditions { get; set; } = new Dictionary<Func<Task<bool>>, Func<Task>>();

        public AbstractCommand(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public virtual void Conditions()
        {
            
        }


        public void Condition(Func<Task<bool>> condition, Func<Task> action)
        {
            CommandConditions.Add(condition, action);
        }

        public void Condition(AbstractCommandCondition commandCondition)
        {
            CommandConditions.Add(commandCondition.Condition, commandCondition.Action);
        }


        public async Task<bool> Execute()
        {
            if (CommandConditions.Count == 0)
            {
                await Executee();
                return true;
            }
            else
            {
                foreach (KeyValuePair<Func<Task<bool>>, Func<Task>> condition in CommandConditions)
                {
                    if (await condition.Key() == false)
                    {
                        await condition.Value();
                        return false;
                    }
   
                }
                await Executee();
                return true;
            }
        }



        protected abstract Task Executee();

        public List<CommandArgument> GetArguments()
        {
            List<CommandArgument> arguments = new List<CommandArgument>();
            List<PropertyInfo> properties = GetArgumentsProperties();
            if (properties != null & properties.Count > 0)
            {
                foreach (PropertyInfo property in properties)
                {
                    arguments.Add(new CommandArgument(property.PropertyType, property.GetValue(this), GetArgumentAttributes(property)));
                }
            }
            return arguments;
        }

        private List<PropertyInfo> GetArgumentsProperties()
        {
            List<PropertyInfo> properties = this.GetType().GetProperties().Where(property => property.IsDefined(typeof(CommandArgumentAttribute), false)).ToList();
            return properties;
        }

        private CommandArgumentAttribute GetArgumentAttributes(PropertyInfo argumentProperties)
        {
            CommandArgumentAttribute attributes = (CommandArgumentAttribute)Attribute.GetCustomAttribute(argumentProperties, typeof(CommandArgumentAttribute));
            return attributes;
        }

        /* private CommandCondition CreateCommandConditionInstance(Type commandConditionType, CommandContext context, object[] arguments)
         {
             CommandCondition condition = (CommandCondition)Activator.CreateInstance(commandConditionType);

             condition.Context = context;
             for (int i = 0; i < arguments.Length; i++)
             {
                 object argument = arguments[i];
                 string name = argument.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<CommandConditionAttribute>().Name != null).FirstOrDefault()?.Name;
                 PropertyInfo property = commandConditionType.GetProperty(name);
                 property.SetValue(property, argument);
             }

             return condition;
         }*/
    }
}
