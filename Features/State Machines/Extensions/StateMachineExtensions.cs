using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using Remedy.StateMachines.VS;

namespace Remedy.StateMachines
{
    public static class StateMachineExtensions
    {
        public static void ActivateStateMachine(this IStateMachine stateEnabledMB)
        {
            StateMachineManager.ActivateStateMachine(stateEnabledMB);
        }

        public static void SetState<TState>(this IStateMachine stateMachine) where TState : StateMachineManager.StateBase
        {
            if (stateMachine != null)
            {
                if(stateMachine.stateReferences == null)
                {
                    ActivateStateMachine(stateMachine);
                }

                StateMachineManager.SetState<TState>(stateMachine);
            }
            else
                Debug.LogError("This MonoBehaviour does not implement IFSM, it can not be used as a State Machine.", stateMachine.gameObject);
        }

        public static void SetState(this IStateMachine stateMachine, string stateName)
        {
            if (stateMachine != null)
            {
                if (stateMachine.stateReferences == null)
                {
                    ActivateStateMachine(stateMachine);
                }

                OnStateEnter.Invoke(stateMachine.gameObject, stateMachine.CurrentState.GetType().Name, stateName);
                StateMachineManager.SetState(stateMachine, stateName);
            }
        }

        public static StateMachineManager.StateBase GetCurrentState(this IStateMachine stateMachine)
        {
            return StateMachineManager.GetStateMachineReference(stateMachine).currentState;
        }

        public static List<StateMachineManager.StateReference> GetStateReferences(this IStateMachine stateMachine)
        {
            return StateMachineManager.GetStateMachineReference(stateMachine).stateReferences.Flatten().Select(x => x.value).OfType<StateMachineManager.StateReference>().ToList();
        }
    }
}