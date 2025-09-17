using Remedy.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableEventsData : SingletonData<ScriptableEventsData>
{
    public SerializableDictionary<int, ScriptableEventBase.IOBase> IO = new();
    /// <summary>
    /// The IO Bases as that were are set in Edit Mode. This Dictionary is not modified in Runtime, and actually
    /// acts as a reference point for accessing the IO of a Prefab as it was set up within the Schematics Flow Graph.
    /// </summary>
    public static SerializableDictionary<int, ScriptableEventBase.IOBase> IOBases => Instance.IO;
}