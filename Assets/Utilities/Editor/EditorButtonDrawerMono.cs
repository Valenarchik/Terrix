using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using CustomUtilities.Attributes;

namespace CustomUtilities.EditorNamespace
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class EditorButtonDrawerMono : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var methods = target.GetType()
                .GetMembers(BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                            BindingFlags.NonPublic)
                .Where(o => Attribute.IsDefined(o, typeof(EditorButtonAttribute)));

            foreach (var memberInfo in methods)
            {
                if (GUILayout.Button(memberInfo.Name))
                {
                    var method = memberInfo as MethodInfo;
                    method.Invoke(target, null);
                }
            }
        }
    }
}