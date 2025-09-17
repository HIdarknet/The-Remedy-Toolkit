using Remedy.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.LevelDesign
{
    public class LevelDesignData : SingletonData<LevelDesignData>
    {
        public TriggerInfo Triggers;

        /// <summary>
        /// Information about the Game's Trigger Volumes
        /// </summary>
        [Serializable]
        public class TriggerInfo
        {
            public TriggerDefinition[] TriggerTypes;

            [Serializable]
            public class TriggerDefinition
            {
                public string Name;
                public bool Filled = false;
                public Color Color;
            }
        }
    }
}