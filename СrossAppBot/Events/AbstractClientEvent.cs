using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Events
{
    public class AbstractClientEvent : EventArgs
    {
        public override string ToString()
        {
            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties();
            StringBuilder builder = new StringBuilder();
            builder.Append($"{type.Name}: ");

            foreach (PropertyInfo prop in properties)
            {
                object value = prop.GetValue(this);
                builder.Append($"{prop.Name}={value}, ");
            }

            // Remove the trailing comma and space
            if (properties.Length > 0)
                builder.Length -= 2;

            return builder.ToString();
        }
    }
}
