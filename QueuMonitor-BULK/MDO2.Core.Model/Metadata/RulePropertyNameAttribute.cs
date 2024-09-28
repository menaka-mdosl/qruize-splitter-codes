using System;
using System.Collections.Generic;
using System.Linq;

namespace MDO2.Core.Model.Metadata
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class RulePropertyNameAttribute : Attribute
    {
        public RulePropertyNameAttribute(params string[] rulePropertyName)
        {
            var hashSet = new HashSet<string>();
            if (rulePropertyName != null)
            {
                foreach (var name in rulePropertyName)
                {
                    hashSet.Add(name);
                }
            }

            RulePropertyName = hashSet.ToArray();
        }

        public string[] RulePropertyName { get; private set; }
    }
}
