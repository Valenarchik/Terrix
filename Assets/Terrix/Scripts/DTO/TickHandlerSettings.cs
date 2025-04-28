using System;

namespace Terrix.DTO
{
    public class TickHandlerSettings
    {
        public bool EveryUpdate { get; }
        public bool EveryFixedUpdate { get; }
        public TickHandlerType Type { get; }
        public TimeSpan TickDelta { get; }
        public int Priority { get; }
        
        public TickHandlerSettings(TickHandlerType type, TimeSpan tickDelta, int priority, bool everyUpdate, bool everyFixedUpdate)
        {
            Type = type;
            TickDelta = tickDelta;
            Priority = priority;
            EveryUpdate = everyUpdate;
            EveryFixedUpdate = everyFixedUpdate;
        }
    }
}