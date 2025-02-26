using System.Collections.Generic;
using NUnit.Framework;

namespace CustomUtilities.Tests
{
    [TestFixture]
    public class AsDictionaryConverterTests
    {
        [Test]
        public void TestAsTestClass()
        {
            var test = new Test()
            {
                intField = 1,
                IntProperty = 2,
            };
            Test.intStaticField = 3;
            Test.IntStaticProperty = 4;

            var expected = new Dictionary<string, object>()
            {
                ["IntProperty"] = 2
            };
            var actual = Converter.AsDictionary(test);
            
            CollectionAssert.AreEquivalent(actual, expected);
        }
    }

    public class Test
    {
        public int intField;
        public int IntProperty { get; set; }
        public static int intStaticField;
        public static int IntStaticProperty { get; set; }
    }
}