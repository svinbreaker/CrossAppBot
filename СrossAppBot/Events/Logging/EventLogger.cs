using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace СrossAppBot.Events.Logging
{
    public class EventLogger
    {
        public LogSettings Settings { get; set; }
        
        public EventLogger(LogSettings settings) 
        {
            Settings = settings;
        }

        public async Task LogEventAsync(AbstractClientEvent clientEvent)
        {
            await Console.Out.WriteLineAsync($"Event logged: {clientEvent.ToString()}");
        }    
    }
}
