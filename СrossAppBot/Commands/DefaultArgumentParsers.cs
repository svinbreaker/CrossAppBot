using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Entities;

namespace СrossAppBot.Commands
{
    public interface DefaultArgumentParsers
    {
        public static Dictionary<Type, IArgumentParser> Get
        {
            get
            {
                var dictionary = new Dictionary<Type, IArgumentParser>
                {
                    { typeof(int), new IntArgumentParser() },
                    { typeof(int?), new IntArgumentParser() },
                    { typeof(double), new DoubleArgumentParser() },
                    { typeof(double?), new DoubleArgumentParser() },
                    { typeof(float), new DoubleArgumentParser() },
                    { typeof(float?), new DoubleArgumentParser() },
                    { typeof(string), new StringArgumentParser() },
                    { typeof(string), new StringArgumentParser() },
                    { typeof(ChatUser), new ChatUserArgumentParser() }
                };
                return dictionary;
            }
        }


        public class IntArgumentParser : IArgumentParser
        {
            public IntArgumentParser() : base(typeof(int?)) { }

            public override object Parse(string value, CommandContext context = null)
            {
                if (int.TryParse(value, out int intValue))
                {
                    return intValue;
                }
                return null;
            }
        }

        public class DoubleArgumentParser : IArgumentParser
        {
            public DoubleArgumentParser() : base(typeof(double?)) { }
            public override object Parse(string value, CommandContext context = null)
            {
                if (double.TryParse(value, out double doubleValue))
                {
                    return doubleValue;
                }
                return null;
            }
        }

        public class FloatArgumentParser : IArgumentParser
        {
            public FloatArgumentParser() : base(typeof(float?)) { }
            public override object Parse(string value, CommandContext context = null)
            {
                if (float.TryParse(value, out float floatValue))
                {
                    return floatValue;
                }
                return null;
            }
        }

        public class StringArgumentParser : IArgumentParser
        {
            public StringArgumentParser() : base(typeof(string)) { }

            public override object Parse(string value, CommandContext context = null)
            {
                return value;
            }
        }

        public class ChatUserArgumentParser : IArgumentParser
        {
            public ChatUserArgumentParser() : base(typeof(ChatUser)) { }

            public override object Parse(string value, CommandContext context = null)
            {
                ChatUser user = null;
                if (context != null) 
                {
                    user = context.Client.GetUserByMention(context.Message, value).Result;
                }

                return user;
            }
        }
    }
}
