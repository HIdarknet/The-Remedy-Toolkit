
using UnityEngine;

/// <summary>
/// Represents a set of three boolean values corresponding to the X, Y, and Z axes.
/// Can be used to enable or disable operations along specific axes, such as constraining movement or rotation.
/// </summary>
[System.Serializable]
public struct Vector3Bool
{
    public bool x;
    public bool y;
    public bool z;

    private static Vector3Bool _true;
    public static Vector3Bool True => _true;

    private static Vector3Bool _false;
    public static Vector3Bool False => _false;


    static Vector3Bool()
    {
        _true = new Vector3Bool(true, true, true);
        _false = new Vector3Bool(false, false, false);
    }

    public Vector3Bool(bool x, bool y, bool z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }


    public static implicit operator Vector3(Vector3Bool v)
    {
        return new Vector3(v.x ? 1f : 0f, v.y ? 1f : 0f, v.z ? 1f : 0f);
    }

    // Overrides
    /// <summary>
    /// Turns the Vecto3Bool into floats to multiply by Vector3
    /// </summary>
    /// <param name="b"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 operator *(Vector3 v, Vector3Bool b)
    {
        return new Vector3(
            v.x * (b.x ? 1f : 0f),
            v.y * (b.y ? 1f : 0f),
            v.z * (b.z ? 1f : 0f)
        );
    }
    /// <summary>
    /// Turns the Vecto3Bool into floats to multiply by Vector3
    /// </summary>
    /// <param name="b"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 operator *(Vector3Bool b, Vector3 v) => v * b;

    /// <summary>
    /// Toggles all the flags of the Vector3Bool
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3Bool operator !(Vector3Bool v)
    {
        return new Vector3Bool(!v.x, !v.y, !v.z);
    }
}