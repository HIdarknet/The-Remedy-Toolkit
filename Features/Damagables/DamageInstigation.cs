using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Remedy.Common;

namespace Remedy.Damagables
{
    /// <summary>
    /// An instance of DamageInstigation is used to manage/track the progress of Damage/Healing
    /// </summary>
    [Serializable]
    public struct DamageInstigation
    {
        /// <summary>
        /// The position that the Instigation begun at
        /// </summary>
        public Vector3 Position;
        public int DataID;

        public DamageInstigation(Vector3 position, int dataID)
        {
            Position = position;
            DataID = dataID;
        }
    }
}