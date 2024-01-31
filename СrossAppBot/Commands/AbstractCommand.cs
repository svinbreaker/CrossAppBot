using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public AbstractCommand(string name, string description) 
        {
            Name = name;
            Description = description;
            //properties = this.GetType().GetProperties().Where(
             //   prop => Attribute.IsDefined(prop, typeof(CommandParameterAttribute))).ToList();
        }


        

        /*public async Task Execute(string command, CommandContext context = null) 
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

            await Execute(parameters, context);
        }*/

        public abstract Task Execute(CommandContext context = null);
    }
}
