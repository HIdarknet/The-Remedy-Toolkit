using System;

[AttributeUsage(AttributeTargets.Class)]
public class IODockTargetAttribute : Attribute
{
    public Type TargetType { get; }
    public IODockTargetAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
