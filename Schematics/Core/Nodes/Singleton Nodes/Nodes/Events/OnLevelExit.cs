// project armada

#pragma warning disable 0414

using BlueGraph;
using Remedy.Schematics;
using UnityEngine;

[Node(Path = "Events"), Tags("Runtime")]
public class OnLevelExit : SchematicEventNode
{
    [Output]
    public string LevelName;
    
}