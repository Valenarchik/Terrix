using System;
using UnityEngine;

namespace Terrix.MainMap.AI
{
    [Serializable]
    public class BotSettings
    {
        [Header("Основные")]
        public float timeoutInSeconds;
        public float timeoutDeflection;
        
        [Header("Население")]
        [Range(0, 1)] public float minPercentOfPopulation = 0.2f;
        
        [Header("Атака")]
        [Range(0, 1)] public float chanceToAttackPlayer = 0.01f;
        [Range(0, 1)] public float chanceToAttackBot = 0.5f;
        [Range(0, 1)] public float minAttackCost = 0.1f;
        [Range(0, 1)] public float maxAttackCost = 1f;
        
        [Header("Модификаторы")]
        [Range(0, 1)] public float incomeMultiplier = 0.5f;
    }
}