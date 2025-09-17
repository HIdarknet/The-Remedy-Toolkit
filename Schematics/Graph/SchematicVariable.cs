using Remedy.Schematics.Utils;
using System;

[Serializable]
public class SchematicVariable
{ }

[Serializable]
public class SchematicVariable<T> : SchematicVariable
{
    public string Name;
    public T Value;

    public static implicit operator SchematicVariable<T>(T value)
    {
        return new SchematicVariable<T> { Value = value };
    }

    public static implicit operator T(SchematicVariable<T> schematicVar)
    {
        return schematicVar.Value;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }
}