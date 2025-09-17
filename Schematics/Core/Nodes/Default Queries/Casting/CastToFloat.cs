// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Casts"), Tags("Default")]
    public class CastToFloat : Cast<float>
    { }
}