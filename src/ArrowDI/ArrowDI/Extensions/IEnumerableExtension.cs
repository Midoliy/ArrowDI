using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ArrowDI.Extensions
{
    internal static class IEnumerableExtension
    {
        public static PropertyInfo SelectPropetyOrDefault(this IEnumerable<PropertyInfo> @this, string name)
        {
            foreach (var property in @this)
            {
                var arrowhead = (ArrowheadAttribute)Attribute.GetCustomAttribute(property, typeof(ArrowheadAttribute));
                if (arrowhead == null)
                    continue;

                if (arrowhead.Name != name)
                    continue;

                return property;
            }

            return default;
        }
    }
}
