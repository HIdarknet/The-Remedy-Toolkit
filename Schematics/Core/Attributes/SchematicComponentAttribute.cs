using System;

[AttributeUsage(AttributeTargets.Class)]
public class SchematicComponentAttribute : Attribute
{
    public string Path { get; }

    public SchematicComponentAttribute(string path)
    {
        Path = path;
    }
}
