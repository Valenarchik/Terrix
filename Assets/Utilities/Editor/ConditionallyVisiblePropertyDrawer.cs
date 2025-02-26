using System;
using System.Linq;
using System.Reflection;
using CustomUtilities.Attributes;
using CustomUtilities.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomUtilities.EditorNamespace
{
    [CustomPropertyDrawer(typeof(ConditionallyVisibleAttribute))]
    public class ConditionallyVisiblePropertyDrawer : PropertyDrawer
    {
        private new ConditionallyVisibleAttribute attribute => (ConditionallyVisibleAttribute) base.attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldDisplay(property))
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUI.PropertyField(position, property, label, includeChildren: true);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldDisplay(property)
                ? EditorGUI.GetPropertyHeight(property, label, includeChildren: true)
                : 0;
        }

        private bool ShouldDisplay(SerializedProperty property)
        {
            return attribute.MethodName != null
                ? ShouldDisplayByMethod(attribute.MethodName, property)
                : attribute.Negative
                    ? !ShouldDisplayByDependentProperty(property)
                    : ShouldDisplayByDependentProperty(property);
        }

        private bool ShouldDisplayByMethod(string methodName, SerializedProperty property)
        {
            var splitPropertyPath = property.propertyPath.Split(".");
            var obj = splitPropertyPath.Length < 2
                ? property.serializedObject.targetObject
                : property.serializedObject
                    .FindProperty(splitPropertyPath.Take(splitPropertyPath.Length - 1).JoinStrings("."))
                    .boxedValue;

            var objectType = obj.GetType();
            var method = objectType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new InvalidOperationException($"Method | bool {objectType}.{methodName}() | not found!");
            }

            if (method.ReturnType != typeof(bool))
            {
                throw new InvalidOperationException($"Method {objectType}.{methodName} return type is not bool!");
            }

            return (bool) method.Invoke(obj, Array.Empty<object>());
        }

        private bool ShouldDisplayByDependentProperty(SerializedProperty property)
        {
            var splitPropertyPath = property.propertyPath.Split(".");
            splitPropertyPath = splitPropertyPath.Take(splitPropertyPath.Length - 1).ToArray();

            return attribute.PropertyNameValuePairs.All(pair =>
            {
                var refProperty = property.serializedObject.FindProperty(
                    splitPropertyPath.Concat(new[] {pair.PropertyName}).JoinStrings("."));
                return ShouldDisplayByDependentProperty(refProperty, pair.Value);
            });
        }

        private bool ShouldDisplayByDependentProperty(SerializedProperty dependentProp, object attributeValue)
        {
            return dependentProp.propertyType switch
            {
                SerializedPropertyType.Boolean => attributeValue is bool value && value == dependentProp.boolValue,
                SerializedPropertyType.Enum => attributeValue is Enum && (int) attributeValue == dependentProp.intValue,
                SerializedPropertyType.Integer => attributeValue is int value && value == dependentProp.intValue,
                SerializedPropertyType.Float => attributeValue is float value &&
                                                Math.Abs(value - dependentProp.floatValue) < 0.1e-5,
                SerializedPropertyType.String => attributeValue is string value && value == dependentProp.stringValue,
                _ => throw new NotImplementedException()
            };
        }
    }
}