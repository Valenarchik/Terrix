using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Terrix.DTO;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    public class TickGenerator: MonoBehaviour
    {
        public void InitializeLoop(IEnumerable<TickHandlerTuple> tickHandlerTuples)
        {
            foreach (var tuple in tickHandlerTuples.OrderBy(i => i.Settings.Priority))
            {
                StartCoroutine(Loop(tuple));
            }
        }

        public void StopLoop()
        {
            StopAllCoroutines();
        }
        
        private IEnumerator Loop(TickHandlerTuple tickHandlerTuple)
        {
            while (true)
            {
                tickHandlerTuple.Handler.HandleTick();
                if (tickHandlerTuple.Settings.EveryUpdate)
                {
                    yield return null;
                }
                else if (tickHandlerTuple.Settings.EveryFixedUpdate)
                {
                    yield return new WaitForFixedUpdate();
                }
                else
                { 
                    yield return new WaitForSeconds((float)tickHandlerTuple.Settings.TickDelta.TotalSeconds);
                }
            }
        }
        
        public class TickHandlerTuple
        {
            public ITickHandler Handler { get; }
            public TickHandlerSettings Settings { get; }
            
            public TickHandlerTuple([NotNull] ITickHandler handler, [NotNull] TickHandlerSettings settings)
            {
                Handler = handler ?? throw new ArgumentNullException(nameof(handler));
                Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            }
        }
    }
}