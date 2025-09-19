using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CustomGizmos
{
    public static void DrawArrow(Vector3 start, Vector3 direction, float shaftLength, Color color)
    {
#if UNITY_EDITOR
        if (direction == Vector3.zero)
            return;

        Handles.color = color;

        Vector3 end = start + direction.normalized * shaftLength;

        Handles.DrawLine(start, end);

        float headSize = 0.3f;
        Handles.ConeHandleCap(
            0,
            end,
            Quaternion.LookRotation(direction),
            headSize,
            EventType.Repaint
        );
#endif
    }

    /// <summary>
    /// Draws a box-based arrow with an optional custom rotation around its axis.
    /// </summary>
    public static void DrawCircleArrow(Vector3 start, Vector3 direction, float shaftLength, Color color, float radius = 0.5f, Vector3 normal = default)
    {
#if UNITY_EDITOR
        if (direction == Vector3.zero)
            return;

        Handles.color = color;

        Vector3 dirNorm = direction.normalized;
        Vector3 end = start + dirNorm * shaftLength;

        Handles.DrawLine(start, end);

        if(normal == default)
            Handles.DrawSolidDisc(start += direction * shaftLength, direction, radius);
        else
            Handles.DrawSolidDisc(start += direction * shaftLength, normal, radius);
#endif
    }
}