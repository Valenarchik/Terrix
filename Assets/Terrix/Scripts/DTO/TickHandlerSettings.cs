using System;

namespace Terrix.DTO
{
    public class TickHandlerSettings
    {
        public TickHandlerType Type { get; }
        public TimeSpan TickDelta { get; }
        public int Priority { get; }
        
        public TickHandlerSettings(TickHandlerType type, TimeSpan tickDelta, int priority)
        {
            Type = type;
            TickDelta = tickDelta;
            Priority = priority;
        }
    }
}