using System.Collections.Generic;

public static class GizmoManager
{
    private static HashSet<string> enabled = new();

    public static bool IsEnabled(string name) => enabled.Contains(name);
    public static void SetEnabled(string name, bool state)
    {
        if (state) enabled.Add(name);
        else enabled.Remove(name);
    }
}
