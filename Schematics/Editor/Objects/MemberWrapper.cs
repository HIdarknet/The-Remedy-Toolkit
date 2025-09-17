using System;
using System.Collections.Generic;
using System.Reflection;

public class MemberWrapper
{
    private FieldInfo field = null;
    private PropertyInfo property = null;

    public MemberWrapper(FieldInfo field) => this.field = field;
    public MemberWrapper(PropertyInfo property) => this.property = property;

    public static implicit operator MemberWrapper(FieldInfo field) => new MemberWrapper(field);
    public static implicit operator MemberWrapper(PropertyInfo property) => new MemberWrapper(property);

    public Type MemberType => field != null ? field.FieldType : property.PropertyType;
    public string Name => field != null ? field.Name : property.Name;

    public object GetValue(object obj) => field != null ? field.GetValue(obj) : property.GetValue(obj);
    public void SetValue(object obj, object value)
    {
        if (field != null) field.SetValue(obj, value);
        else property.SetValue(obj, value);
    }
    public IEnumerable<Attribute> GetCustomAttributes()
    {
        return field != null ? field.GetCustomAttributes() : property.GetCustomAttributes();
    }
}