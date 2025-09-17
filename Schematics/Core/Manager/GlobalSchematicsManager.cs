using Remedy.Framework;
using Remedy.Schematics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalSchematicsManager : Singleton<GlobalSchematicsManager>
{
    private Dictionary<SchematicScope, List<SchematicInstanceController>> SchematicInstances = new();
    private List<SchematicInstanceController> _activeInstances = new();

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        // Adds logic to the Object Manager's Prefab Instantiation to determine whether to add the Instantiated Object as a Schematic Instance.
        ObjectManager.OnPrefabInstantiated += (GameObject prefab, GameObject instance) =>
        {
            if (GlobalSchematicManagerData.SchematicPrefabs.ContainsKey(prefab))
            {
                var schemInst = instance.GetComponent<SchematicInstanceController>();
                Instance.SchematicInstances[GlobalSchematicManagerData.SchematicPrefabs[prefab]].Add(schemInst);
                Instance.SetupSchematicInstance(schemInst);
            }
            if(GlobalSchematicManagerData.PrefabIOBases.ContainsKey(prefab))
            {
                foreach (var id in GlobalSchematicManagerData.PrefabIOBases[prefab])
                {
                    ScriptableEventBase.IOBase.OnIOBaseInstantiated(id, ScriptableEventsData.IOBases[id].IOEvents);
                }
            }
        };
    }

    // Okay, now for the good shit.
    private void SetupSchematicInstance(SchematicInstanceController instance)
    {

    }

}
