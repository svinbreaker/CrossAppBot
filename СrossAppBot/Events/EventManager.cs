using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using СrossAppBot.Entities;
using СrossAppBot.Events.Logging;

namespace СrossAppBot.Events
{
    public class EventManager
    {
        public delegate Task EventHandler<T>(T clientEvent) where T : AbstractClientEvent;

        // Use a dictionary to map event types to lists of event handlers
        private Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();

        EventLogger logger = new EventLogger(LogSettings.Auto);

        // Subscribe to an events
        public void Subscribe<T>(params EventHandler<T>[] handlers) where T : AbstractClientEvent
        {
            if (!eventHandlers.ContainsKey(typeof(T)))
            {
                eventHandlers[typeof(T)] = new List<Delegate>();
            }

            foreach (var handler in handlers)
            {
                eventHandlers[typeof(T)].Add(handler);
            }

        }

        // Unsubscribe from an events
        public void Unsubscribe<T>(params EventHandler<T>[] handlers) where T : AbstractClientEvent
        {
            if (eventHandlers.ContainsKey(typeof(T)))
            {
                foreach (var handler in handlers)
                {
                    eventHandlers[typeof(T)].Remove(handler);
                }
            }
        }

        // Trigger an event
        public async Task CallEvent<T>(T clientEvent) where T : AbstractClientEvent
        {
            if (eventHandlers.ContainsKey(typeof(T)))
            {
                foreach (var handler in eventHandlers[typeof(T)])
                {
                    await ((EventHandler<T>)handler)(clientEvent);

                    //temp logging solution
                    if (logger.Settings == LogSettings.Auto)
                    {
                        await logger.LogEventAsync(clientEvent);
                    }
                }
            }
        }

        /*// Trigger events
        public async Task CallEvent(params AbstractClientEvent[] clientEvents)
        {
            foreach (var clientEvent in clientEvents)
            {
                Type eventType = clientEvent.GetType();

                if (eventHandlers.ContainsKey(eventType))
                {
                    foreach (var handler in eventHandlers[eventType])
                    {
                        var method = (EventHandler<AbstractClientEvent>)handler;
                        await method(clientEvent);
                    }
                }
            }
        }*/
    }
}
