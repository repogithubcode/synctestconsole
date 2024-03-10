using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.TimedEvents
{
    public class TimedEventManager
    {

        public void ProcessEvents(System.Text.StringBuilder messageBuilder)
        {
            foreach (TimedEvent timedEvent in GetTimedEvents())
            {
                if (timedEvent.ShouldExecute())
                {
                    try
                    {
                        timedEvent.DoWork(messageBuilder);
                    }
                    catch (Exception ex)
                    {
                        messageBuilder.AppendLine("Process Events error: " + ex.Message);
                    }
                    
                }
            }
        }

        public void CancelAllEvents()
        {
            foreach (TimedEvent timedEvent in GetTimedEvents())
            {
                timedEvent.Cancel();
            }
        }

        private List<TimedEvent> GetTimedEvents()
        {
            if (_timedEvents == null)
            {
                _timedEvents = typeof(TimedEvent)
                    .Assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(TimedEvent)) && !t.IsAbstract)
                    .Select(t => (TimedEvent)Activator.CreateInstance(t)).ToList();
            }

            return _timedEvents;
        }
        private List<TimedEvent> _timedEvents;

    }
}