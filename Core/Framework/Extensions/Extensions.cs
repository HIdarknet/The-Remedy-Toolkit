using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T CopyComponent<T>(this T original, GameObject destination) where T : Component
    {
        T[] m_List = destination.GetComponents<T>();
        System.Type type = original.GetType();
        System.Reflection.FieldInfo[] fields = type.GetFields();

        foreach (T comp in m_List)
        { 
            // If we already have one of them
            if (original.GetType() == comp.GetType())
            {
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    field.SetValue(comp, field.GetValue(original));
                }
                return comp;
            }
        }

        // By here, we need to add it
        T copy = destination.AddComponent<T>();

        // Copied fields can be restricted with BindingFlags
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }

        return copy;
    }


    public static string FormatNicely(this string name)
    {
        return System.Text.RegularExpressions.Regex.Replace(name, "(\\B[A-Z])", " $1");
    }


    /// <summary>
    ///  Get's the relative path of a child within the Parent's Hierarchy. If it is not a child, simply returns an empty string.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <returns></returns>
    public static string GetRelativePath(this Transform parent, Transform child)
    {
        if (!child.IsChildOf(parent))
            return "";

        List<string> path = new List<string>();
        Transform current = child;

        while (current != parent)
        {
            path.Insert(0, current.name);
            current = current.parent;
        }

        return string.Join("/", path);
    }
}