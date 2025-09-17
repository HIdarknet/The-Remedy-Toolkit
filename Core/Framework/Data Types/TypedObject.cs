using System;

namespace Remedy.Framework
{
    /// <summary>
    /// Contains an Object Variable with it's Type
    /// </summary>
    public class TypedObject
    {
        public Type Type;
        public object Value;

        public TypedObject(Type type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}