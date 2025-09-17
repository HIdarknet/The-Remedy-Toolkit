using UnityEditor;

public class GlobalSubsystem
{
    private static string _playerTag;
    public static string PlayerTag => _playerTag;

    /// <summary>
    /// Sets the Player Tag. 
    /// </summary>
    /// <param name="tag"></param>
    public static void SetPlayerTag(string tag)
    {
        _playerTag = tag;
    }
}