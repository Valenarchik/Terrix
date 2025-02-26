using System;
using CustomUtilities.Extensions;
using UnityEngine;

namespace CustomUtilities.Attributes
{
    /// <summary>
    /// Use this attribute if you need to make a field visible depending on another field.
    /// </summary>
    public class ConditionallyVisibleAttribute : PropertyAttribute
    {
        public PropertyNameValuePair[] PropertyNameValuePairs { get; }
        public bool Negative { get; }

        public string MethodName { get; }
        
        public ConditionallyVisibleAttribute(string methodName)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }

        public ConditionallyVisibleAttribute(string propName, object value, bool negative = false)
        {
            PropertyNameValuePairs = new[] {new PropertyNameValuePair(propName, value)};
            Negative = negative;
        }

        public ConditionallyVisibleAttribute(
            string propName1,
            object value1,
            string propName2,
            object value2,
            bool negative = false)
        {
            PropertyNameValuePairs = new[]
            {
                new PropertyNameValuePair(propName1, value1),
                new PropertyNameValuePair(propName2, value2)
            };
            Negative = negative;
        }

        public ConditionallyVisibleAttribute(
            string propName1,
            object value1,
            string propName2,
            object value2,
            string propName3,
            object value3,
            bool negative = false)
        {
            PropertyNameValuePairs = new[]
            {
                new PropertyNameValuePair(propName1, value1),
                new PropertyNameValuePair(propName2, value2),
                new PropertyNameValuePair(propName3, value3)
            };
            Negative = negative;
        }
    }

    public struct PropertyNameValuePair
    {
        public string PropertyName { get; }
        public object Value { get; }

        public PropertyNameValuePair(string propertyName, object value)
        {
            PropertyName = !propertyName.IsNullOrEmpty()
                ? propertyName
                : throw new ArgumentException(nameof(propertyName));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}