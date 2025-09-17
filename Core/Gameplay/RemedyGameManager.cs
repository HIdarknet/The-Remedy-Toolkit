using Remedy.Framework;
using UnityEngine;

namespace Remedy.Common
{
    /// <summary>
    /// The Remedy Game Manager provides global access to common Components.
    /// </summary>
    public class RemedyGameManager : Singleton<RemedyGameManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            Initialize();
        }

        /// <summary>
        /// Returns Camera.main if the current Camera has not been set via the CameraSubsystem.
        /// The Subsystem is used to give more control over the active Camera in your game.
        /// </summary>
        public static Camera MainCamera => CameraSubsystem.Camera;

        /// <summary>
        /// Returns whether the current system is the Host of the Networked Match. 
        /// The Networking Subsystem overrides this value (defaulted to true) when a multiplayer match has begun.
        /// </summary>
        public static bool IsHost => true;

        public RemedyInput Input => RemedyInput.Instance;

        private void Start()
        {
            Input.InitializeInput();
        }

        private void Update()
        {
            Input.UpdateInputs();
        }
    }
}