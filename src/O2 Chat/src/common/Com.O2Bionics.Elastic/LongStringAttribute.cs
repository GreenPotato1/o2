using System;

namespace Com.O2Bionics.Elastic
{
    /// <inheritdoc />
    /// <summary>
    /// String fields and properties, marked with this attribute,
    /// will have "keyword.ignore_above" value set to <seealso cref="F:Com.O2Bionics.Utils.FieldConstants.IgnoreAbove" />.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class LongStringAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
    }
}