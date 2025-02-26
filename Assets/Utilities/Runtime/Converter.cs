using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace CustomUtilities
{
    /// <summary>
    /// source = "https://stackoverflow.com/questions/43277722/expression-tree-create-dictionary-with-property-values-for-class"
    /// </summary>
    public static class Converter
    {
        private static readonly Dictionary<Type, Func<object, Dictionary<string, object>>> convertFunctions = new();

        public static Dictionary<string, object> AsDictionary([NotNull] object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var objType = obj.GetType();

            if (!convertFunctions.TryGetValue(objType, out var convertFunction))
            {
                convertFunction = CompileToDictionaryFunc(objType);
                convertFunctions[objType] = convertFunction;
            }

            return convertFunction(obj);
        }

        private static Func<object, Dictionary<string, object>> CompileToDictionaryFunc(Type objType)
        {
            var dict = Expression.Variable(typeof(Dictionary<string, object>));
            var par = Expression.Parameter(typeof(object), "obj");
            var parameterAsType = Expression.Convert(par, objType);

            var add = typeof(Dictionary<string, object>).GetMethod("Add",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] {typeof(string), typeof(object)},
                null);

            var body = new List<Expression>
            {
                Expression.Assign(dict, Expression.New(typeof(Dictionary<string, object>)))
            };

            var properties = objType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                // Skip write only or indexers
                if (!properties[i].CanRead || properties[i].GetIndexParameters().Length != 0)
                {
                    continue;
                }

                var key = Expression.Constant(properties[i].Name);
                var value = Expression.Property(parameterAsType, properties[i]);
                // Boxing must be done manually... For reference type it isn't a problem casting to object
                var valueAsObject = Expression.Convert(value, typeof(object));
                body.Add(Expression.Call(dict, add, key, valueAsObject));
            }

            // Return value
            body.Add(dict);

            var block = Expression.Block(new[] {dict}, body);

            var lambda = Expression.Lambda<Func<object, Dictionary<string, object>>>(block, par);
            return lambda.Compile();
        }
    }
}