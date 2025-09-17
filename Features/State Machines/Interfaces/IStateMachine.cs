using System.Collections.Generic;
using UnityEngine;
using Remedy.Framework;
using static Remedy.StateMachines.StateMachineManager;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Remedy.StateMachines
{
    public interface IStateMachine
    {
        public GameObject gameObject { get; }
        public Tree<Reference> stateReferences { get { return GetStateMachineReference(this).stateReferences; } set { GetStateMachineReference(this).stateReferences = value; } }
        public StateBase CurrentState { get{ return GetStateMachineReference(this).currentState; } set{ GetStateMachineReference(this).currentState = value; }}
    }
/*
#if UNITY_EDITOR
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class StateMachineInspectorDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            var targetScript = (MonoBehaviour)target;

            if (targetScript is IStateMachine stateMachine)
            {
                var stateMachineReference = StateMachineManager.GetStateMachineReference(stateMachine);

                EditorGUILayout.LabelField("Current State", stateMachineReference.currentState.GetType().Name);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set State"))
                {
                    var availableStates = stateMachineReference.stateReferences.Flatten()
                        .Select(x => x.value)
                        .OfType<StateReference>()
                        .Select(x => x.state)
                        .ToArray();

                    var menu = new GenericMenu();
                    foreach (var state in availableStates)
                    {
                        menu.AddItem(new GUIContent(state.GetType().Name), false, () => SetState(stateMachine, state.GetType()));
                    }

                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private static void SetState(IStateMachine stateMachine, System.Type stateType)
        {
            StateMachineManager.SetState(stateMachine, stateType.Name);
            EditorUtility.SetDirty(stateMachine.gameObject);
        }
    }
#endif*/
}