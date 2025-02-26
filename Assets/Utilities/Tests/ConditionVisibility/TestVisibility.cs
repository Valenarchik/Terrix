using System.Collections;
using System.Collections.Generic;
using CustomUtilities.Attributes;
using UnityEngine;

namespace CustomUtilities.Tests
{
    public class TestVisibility : MonoBehaviour
    {
        [Header("Bool")]
        [SerializeField] private bool boolValue;

        [ConditionallyVisible(nameof(boolValue), true)]
        [SerializeField] private GameObject forBoolObject;

        [ConditionallyVisible(nameof(boolValue), true, true)]
        [SerializeField] private GameObject forBoolObjectInvert;

        [Header("String")]
        [SerializeField] private string stringValueIsHelloWorld;

        [ConditionallyVisible(nameof(stringValueIsHelloWorld), "HelloWorld")]
        [SerializeField] private GameObject forStringObject;

        [Header("Enum")]
        [SerializeField] private TestEnum enumValueIsValue1;

        [ConditionallyVisible(nameof(enumValueIsValue1), TestEnum.Value1)]
        [SerializeField] private GameObject forEnumObject;

        [Header("ManyConditions")]
        [SerializeField] private TestEnum condition1;
        [SerializeField] private bool condition2;

        [ConditionallyVisible(nameof(condition1), TestEnum.Value1, nameof(condition2), true)]
        [SerializeField] private GameObject forManyConditionsGameObject;
        
        [ConditionallyVisible("MyMethod")]
        [SerializeField] private GameObject gameObjectByMethod;

        private bool MyMethod()
        {
            return condition1 == TestEnum.Value1;
        }

        private enum TestEnum
        {
            Value1,
            Value2
        }
    }
}