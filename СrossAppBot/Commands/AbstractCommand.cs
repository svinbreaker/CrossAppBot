using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Commands.Parameters;

namespace СrossAppBot.Commands
{
    public abstract class AbstractCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        //private List<PropertyInfo> properties = new List<PropertyInfo>();
        public List<CommandCondition> Conditions { get; set; }

        public AbstractCommand(string name, string description, List<CommandCondition> conditions = null)
        {
            Name = name;
            Description = description;
            Conditions = conditions;
            //properties = this.GetType().GetProperties().Where(
            //   prop => Attribute.IsDefined(prop, typeof(CommandParameterAttribute))).ToList();
        }




        /*public async Task TryExecute(string command, CommandContext context = null) 
        {
            string[] stringParameters = command.Split(' ');
            List<object> parameters = new List<object>();

            for (int i = 0; i < properties.Count; i++) 
            {
                PropertyInfo property = properties[i];
                Type type = property.GetType();

                string stringParameter = stringParameters[i];

                if (type == typeof(string)) 
                {
                    parameters.Add(stringParameter);
                }
                else if (type == typeof(int)) 
                {
                    parameters.Add(int.Parse(stringParameter));
                }
                else if (type == typeof(float))
                {
                    parameters.Add(float.Parse(stringParameter));
                }
                else if (type == typeof(double))
                {
                    parameters.Add(double.Parse(stringParameter));
                }
                else if (type == typeof(ChatUser))
                {
                    parameters.Add(context.Client.GetUserByMention(context.Message, stringParameter));                    
                }
                else 
                {
                    parameters.Add(stringParameters);
                }
            }

            await TryExecute(parameters, context);
        }*/

        public async Task TryExecute(CommandContext context)
        {
            foreach (CommandCondition condition in Conditions)
            {
                bool result = await condition.ExecuteIfTrueAsync(context);
                if (result == true)
                {
                    break;
                }
            }

            await Execute(context);
        }
        public abstract Task Execute(CommandContext context);

        public List<CommandArgument> GetArguments()
        {
            List<CommandArgument> arguments = new List<CommandArgument>();
            List<PropertyInfo> properties = GetArgumentsProperties();
            if (properties != null & properties.Count > 0)
            {
                foreach (PropertyInfo property in properties)
                {
                    arguments.Add(new CommandArgument(property.PropertyType, GetArgumentAttributes(property)));
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
    }
}
