//using SaintsField;
using Remedy.Framework;
using System;
using System.Collections.Generic;

public class ScriptableVariable : ScriptableObjectWithID<ScriptableVariable>
{
    private object _value;
    public virtual object ValueAsObject { get => null; set => _value = value; }


    private static List<Type> _scriptableVariableTypes;
    /// <summary>
    /// A Property that caches the ScriptableEvent Types as an Array so it can be used in the Editor.
    /// </summary>
    public static List<Type> ScriptableVariableTypes => _scriptableVariableTypes ??= typeof(ScriptableVariable).GetInheritedTypes();

/*
    private static DropdownList<int> _sETypeDropdown;
    /// <summary>
    /// A Property that caches the Array of ScriptableEvent Types as a Dropdown for use in Editors.
    /// </summary>
    public static DropdownList<int> SETypeDropdown
    {
        get
        {
            if (_sETypeDropdown != null) return _sETypeDropdown;

            var list = new DropdownList<int>();
            var typeList = ScriptableVariable.ScriptableVariableTypes;

            int i = 0;
            foreach (var type in typeList)
            {
                list.Add(type.Name.FormatNicely(), i);
                i++;
            }

            _sETypeDropdown = list;
            return list;
        }
    }*/

}

public class ScriptableVariable<T> : ScriptableVariable
{
    public T Value;
    public override object ValueAsObject { get => Value; set => Value = (T)value; }
}
