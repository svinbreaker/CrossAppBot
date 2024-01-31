using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot
{
    public enum ClientTypes
    {
        [EnumMember(Value = "Telegram")]
        Telegram,

        [EnumMember(Value = "Discord")]
        Discord,

        [EnumMember(Value = "Vk")]
        Vk
    }
}
