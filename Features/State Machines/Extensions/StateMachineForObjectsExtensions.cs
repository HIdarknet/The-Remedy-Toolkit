using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Remedy.Framework;

namespace Remedy.StateMachines.ForGameObjects
{
    public static class StateMachineForGameObjectsExtensions
    {
        public static void SetState<TState>(this GameObject gameObject) where TState : StateMachineManager.StateBase
        {
            StateMachineManager.SetStateForGameObject<TState>(gameObject);
        }
        public static void SetState(this GameObject gameObject, string stateName)
        {
            StateMachineManager.SetStateForGameObject(gameObject, stateName);
        }

        public static List<StateMachineManager.StateBase> GetCurrentStates(this GameObject gameObject)
        {
            var stateMachines = gameObject.GetComponentsImplementing<IStateMachine>();
            var states = stateMachines.Select(stateMachine => stateMachine.CurrentState).ToList(); 
            return states;
        }
        public static List<string> GetAllStateNames(this GameObject gameObject)
        {
            var stateMachines = gameObject.GetComponentsImplementing<IStateMachine>();
            var states = stateMachines.Select(stateMachine => stateMachine.stateReferences.CurrentValue).ToList();
            return states;
        }
    }
}