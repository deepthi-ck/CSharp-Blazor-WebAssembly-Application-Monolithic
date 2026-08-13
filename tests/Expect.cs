using System;

namespace BlazorWasmMonolith.Tests
{
    internal static class Expect
    {
        public static void True(bool value, string message)
        {
            if (!value)
            {
                throw new Exception(message);
            }
        }

        public static void Equal<T>(T expected, T actual)
        {
            if (!object.Equals(expected, actual))
            {
                throw new Exception("Expected " + expected + " but was " + actual);
            }
        }
    }
}
