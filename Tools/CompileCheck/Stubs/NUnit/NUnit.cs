// A tiny stand-in for the parts of NUnit the tests use, so the same test files run here.
using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Class)] public sealed class TestFixtureAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public sealed class TestAttribute : Attribute { }

    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }

    public static class Assert
    {
        public static void IsTrue(bool condition, string message = null)
        {
            if (!condition) throw new AssertionException(message ?? "expected true");
        }

        public static void IsFalse(bool condition, string message = null) => IsTrue(!condition, message ?? "expected false");

        public static void AreEqual(float expected, float actual, float delta, string message = null)
        {
            if (Math.Abs(expected - actual) > delta)
                throw new AssertionException($"{message} expected {expected} ± {delta} but was {actual}");
        }

        public static void AreEqual(object expected, object actual, string message = null)
        {
            if (!Equals(expected, actual))
                throw new AssertionException($"{message} expected {expected} but was {actual}");
        }

        public static void Less(float a, float b, string message = null)
        {
            if (!(a < b)) throw new AssertionException($"{message} expected {a} < {b}");
        }

        public static void Greater(float a, float b, string message = null)
        {
            if (!(a > b)) throw new AssertionException($"{message} expected {a} > {b}");
        }

        public static void IsNotNull(object value, string message = null)
        {
            if (value == null) throw new AssertionException(message ?? "expected not null");
        }

        public static void Fail(string message) => throw new AssertionException(message);
    }
}
