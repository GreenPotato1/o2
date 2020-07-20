using FluentAssertions;
using FluentAssertions.Equivalency;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker.Tests.Utilities
{
    public static class DoubleCompareHelper
    {
        public static EquivalencyAssertionOptions<T> CompareWithPrecision<T>([NotNull] this EquivalencyAssertionOptions<T> options, double precision)
        {
            return options
                .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, precision))
                .WhenTypeIs<double>();
        }
    }
}