using System;
using System.Linq;
using System.Text;

namespace MDO2.Core.Reflection
{
    public static class ReflectionExtensions
    {
        public static string GetPropertyOrderAsKeyValuePair<T>(this T obj) where T : class
        {
            if (obj == null) return string.Empty;

            var properties = obj.GetType()
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(PropertyOrderAttribute)))
                .Select(x =>
                {
                    var at = (PropertyOrderAttribute)x.GetCustomAttributes(typeof(PropertyOrderAttribute), false).Single();
                    return new
                    {
                        prop = x,
                        disp = at.DisplayName,
                        order = at.Order
                    };
                })
                .OrderBy(x => x.order)
                .ToArray();

            var srb = new StringBuilder();
            foreach (var property in properties)
            {
                if (property.prop.CanRead)
                {
                    if (srb.Length > 0)
                        srb.Append(",");

                    var value = property.prop.GetValue(obj)?.ToString() ?? "<NULL>";
                    srb.Append($"{property.disp}={value}");
                }
            }

            return srb.ToString();
        }
    }
}
