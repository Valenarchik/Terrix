using System;
using UnityEngine;

namespace CustomUtilities.Attributes
{
    public class ContainsComponentAttribute : PropertyAttribute
    {
        public readonly Type ComponentType;

        public ContainsComponentAttribute(Type componentType)
        {
            if (!typeof(Component).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"The type {componentType} not inherited from {typeof(Component)}");
            }

            ComponentType = componentType;
        }
    }
}