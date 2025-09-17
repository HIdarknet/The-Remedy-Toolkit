using Remedy.Framework;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Remedy.LevelDesign
{
    // TODO: Create a new Level Manager to handle spawning and Enemy Pools, but implement using ScriptableEvents
    [ExecuteAlways]
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("If True, this Spawn Point will be assigned to the Level it's inside of, instead of being us ed globally.")]
        public bool AssignToLevel = false;
        [Tooltip("If True, this Spawn Point can only be used when the orientation is close to normal.")]
        public bool MustNotBeRotated = false;
        [Tooltip("Offset from the Spawner that the Player is Spawned at. This value is shared with all other Spawn Points.")]
        public Vector3 SpawnOffset = Vector3.up * 1.5f;

        [Tooltip("Invoked when this Spawner is used.")]
        public UnityEvent OnSpawn;

        public Action<GameObject> ExtraSpawnBehaviour;
        /*
            private void Update()
            {
                if (Application.isPlaying)
                {
                    if (CanSpawn())
                        SystemManager.SpawnPoints.Add(this);
                    else
                        SystemManager.SpawnPoints.Remove(this);
                }
            }*/

        /// <summary>
        /// Customizable function used to determine whether this Spawn Point is valid or not. If it is, it will be added to the System Manager's List of Spawn Points. 
        /// </summary>
        protected virtual bool CanSpawn()
        {
            // Add other Spawn Point Conditions
            if (MustNotBeRotated || Vector3.Dot(transform.up, Vector3.up) > 0.99f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Spawn(GameObject gameObject)
        {
            gameObject.transform.position = transform.position + SpawnOffset;
            OnSpawn?.Invoke();
            ExtraSpawnBehaviour?.Invoke(gameObject);
        }
    }
}