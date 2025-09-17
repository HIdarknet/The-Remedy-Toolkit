using System;

[AttributeUsage(AttributeTargets.Class)]
public class FieldRendererTargetAttribute : Attribute
{
    public Type TargetType { get; }
    public FieldRendererTargetAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
