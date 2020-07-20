using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.O2Bionics.Utils
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> FindDerivedTypes(this Type baseType)
        {
            if (baseType == null) throw new ArgumentNullException("baseType");
            return baseType.Assembly.GetTypes().Where(x => x != baseType && baseType.IsAssignableFrom(x));
        }
    }
}