using System;

namespace MDO2.Core.Reflection
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PropertyOrderAttribute : Attribute
    {
        private readonly string displayName;
        private readonly int order_;
        public PropertyOrderAttribute([System.Runtime.CompilerServices.CallerMemberName] string displayName = "", [System.Runtime.CompilerServices.CallerLineNumber] int order = 0)
        {
            this.displayName = displayName;
            order_ = order;
        }

        public int Order { get { return order_; } }

        public string DisplayName { get { return displayName; } }
    }
}
