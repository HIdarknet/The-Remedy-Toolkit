using UnityEditor;
using UnityEngine.UIElements;

public class PersistentFoldout : Foldout
{
    private readonly string _prefsKey;

    public PersistentFoldout(string assetGuid, string foldoutID)
    {
        _prefsKey = $"PersistentFoldout.{assetGuid}.{foldoutID}";
        this.viewDataKey = _prefsKey;
    }
}
