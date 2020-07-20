using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public static class ArgumentValidationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static T NotNull<T>(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this T value,
            [InvokerParameterName] string name) where T : class
        {
            if (value is null)
                throw new ArgumentNullException(name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static ICollection<T> NotEmpty<T>(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this ICollection<T> value,
            [InvokerParameterName] string name)
        {
            if (value != null && value.Count == 0)
                throw new ArgumentException("Can't be empty", name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static T[] NotEmpty<T>(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this T[] value,
            [InvokerParameterName] string name)
        {
            if (value != null && value.Length == 0)
                throw new ArgumentException("Can't be empty", name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static IReadOnlyCollection<T> NotEmpty<T>(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this IReadOnlyCollection<T> value,
            [InvokerParameterName] string name)
        {
            if (value != null && value.Count == 0)
                throw new ArgumentException("Can't be empty", name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static ICollection<T> NotNullOrEmpty<T>(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this ICollection<T> value,
            [InvokerParameterName] string name)
        {
            if (value != null && value.Count == 0)
                throw new ArgumentException("Can't be null or empty", name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static string NotNullOrEmpty(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this string value,
            [InvokerParameterName] string name)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Can't be null or empty", name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static string NotNullOrWhitespace(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this string value,
            [InvokerParameterName] string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Can't be null or whitespace", name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [AssertionMethod]
        public static string IsCorrectEsIndexName(
            [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            this string value,
            [InvokerParameterName] string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Can't be null, have to  or whitespace", name);
            var msg = IdentifierHelper.LowerCase(value);
            if (msg != null)
                throw new ArgumentException(msg, name);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MustBePositive(this int value, [InvokerParameterName] string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name, value, "Must be positive");
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MustBePositive(this uint value, [InvokerParameterName] string name)
        {
            if (value == 0)
                throw new ArgumentOutOfRangeException(name, value, "Must be positive");
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long MustBePositive(this long value, [InvokerParameterName] string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name, value, "Must be positive");
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MustBePositive(this ulong value, [InvokerParameterName] string name)
        {
            if (value == 0)
                throw new ArgumentOutOfRangeException(name, value, "Must be positive");
            return value;
        }
    }
}