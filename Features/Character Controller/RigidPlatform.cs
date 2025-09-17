using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.CharacterControllers
{
    /// <summary>
    /// References the RigidParent, for easy use by the PhysicsBasedCharacterController.
    /// </summary>
    public class RigidPlatform : MonoBehaviour
    {
        public RigidParent rigidParent;
    }
}