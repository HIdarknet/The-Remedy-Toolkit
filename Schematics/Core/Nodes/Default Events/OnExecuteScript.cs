// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Events"), Tags("Object")]
	public class OnExecuteScript : SchematicEventNode
	{
		[Output]
		public List<object> Arguments;
	}
}