using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Remedy.Framework;
using Remedy.StateMachines.Timeline;
using Remedy.StateMachines.VS;

namespace Remedy.StateMachines
{
    public sealed class StateMachineManager : Singleton<StateMachineManager>
    {
        private Dictionary<IStateMachine, StateMachineReference> stateMachineReferences = new();

        /// <summary>
        /// Activates the StateMachine for the class that implements IFSM.
        /// </summary>
        /// <param name="machine"></param>
        public static void ActivateStateMachine(IStateMachine machine)
        {
            var machineReference = new StateMachineReference(machine.GetType(), new());
            machineReference.machine = machine;
            var nestedStates = new Tree<Reference>(machineReference);

            nestedStates.RootNode.AddNodes(GetNestedStates(nestedStates.RootNode));

            machineReference.stateReferences = nestedStates;
            machineReference.currentState = ((StateReference)nestedStates[0]).state;
            Instance.stateMachineReferences.Add(machine, machineReference);

            machine.CurrentState.OnEnter();
            machine.CurrentState.OnEnterEvent?.Invoke();
        }

        private static Node<Reference>[] GetNestedStates(Node<Reference> currentNode)
        {
            var nestedTypes = currentNode.value.type.GetNestedTypes().Where(x => x.IsSubclassOf(typeof(StateBase))).ToArray();
            var nestedStates = new Node<Reference>[nestedTypes.Length];

            for (int i = 0; i < nestedTypes.Length; i++)
            {
                var createdReference = new StateReference(nestedTypes[i]);
                createdReference.state._context = currentNode.tree.RootNode.value.machine;
                createdReference.state.machine = currentNode.tree.RootNode.value.machine;
                nestedStates[i] = new Node<Reference>(currentNode.tree, createdReference, currentNode.index.Append(i).ToArray(), currentNode);
            }

            foreach (var state in nestedStates)
            {
                state.AddNodes(GetNestedStates(state));
            }

            return nestedStates;
        }

        /// <summary>
        /// Set the State of the MonoBehaviour to TState.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="machine"></param>
        public static void SetState<TState>(IStateMachine machine) where TState : StateBase
        {
            var stateReference = Instance.stateMachineReferences[machine].stateReferences.Flatten().Select(x => x.value).OfType<StateReference>().FirstOrDefault(x => x.state is TState);
            CompleteSetState(machine, stateReference);
        }

        /// <summary>
        /// Set the State of the MonoBehaviour to the StateReference with State Name, if it exists.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="machine"></param>
        public static void SetState(IStateMachine machine, string stateName)
        {
            var stateReference = Instance.stateMachineReferences[machine].stateReferences.Flatten().Select(x => x.value).OfType<StateReference>().FirstOrDefault(x => x.state.GetType().Name == stateName);
            
            if(stateReference != null)
                CompleteSetState(machine, stateReference);
        }

        private static void CompleteSetState(IStateMachine machine, StateReference stateReference)
        {
            var previousState = Instance.stateMachineReferences[machine].currentState;
            Instance.stateMachineReferences[machine].currentState = stateReference.state;

            previousState.OnExit();
            previousState.OnExitEvent?.Invoke();

            Instance.stateMachineReferences[machine].currentState.OnEnter();
            Instance.stateMachineReferences[machine].currentState.OnEnterEvent?.Invoke();
            OnStateEnter.Invoke(machine.gameObject, machine.CurrentState.GetType().Name, stateReference.stateName);

        }

        public static void SetStateForGameObject<TState>(GameObject gameObject) where TState : StateBase
        {
            var machines = Instance.stateMachineReferences.Keys.Where(x => x.gameObject == gameObject);
            foreach(var machine in machines)
            {
                machine.SetState<TState>();
            }
        }

        public static void SetStateForGameObject(GameObject gameObject, string stateName)
        {
            var machines = Instance.stateMachineReferences.Keys.Where(x => x.gameObject == gameObject);
            foreach (var machine in machines)
            {
                machine.SetState(stateName);
            }

            var timelineStateMachine = gameObject.GetComponent<TimelineStateReciever>();
            if (timelineStateMachine != null)
            {
                timelineStateMachine.GoToState(stateName);
                OnStateEnter.Invoke(gameObject, timelineStateMachine.CurrentState, stateName);
            }
        }

        public static List<StateBase> GetCurrentStatesForGameObject(GameObject gameObject)
        {
            List<StateBase> states = new();
            var machines = Instance.stateMachineReferences.Keys.Where(x => x.gameObject == gameObject);
            foreach(var machine in machines)
            {
                states.Add(machine.CurrentState);
            }
            return states;
        }

        void Update()
        {
            foreach(var machine in stateMachineReferences.Keys)
            {
                if(machine.gameObject.activeSelf)
                {
                    machine.CurrentState.OnUpdate();
                    machine.CurrentState.OnUpdateEvent?.Invoke();
                }
            }
        }

        public static StateMachineReference GetStateMachineReference(IStateMachine machine)
        {
            if(!Instance.stateMachineReferences.ContainsKey(machine))
                ActivateStateMachine(machine);
            return Instance.stateMachineReferences[machine];
        }

        
        public class Reference
        {
            public IStateMachine machine; 
            public Type type;
        }

        public class StateMachineReference : Reference
        {
            public StateBase currentState;
            public Tree<Reference> stateReferences;

            public StateMachineReference(Type type, Tree<Reference> stateReferences)
            {
                currentState = null;
                this.stateReferences = stateReferences;
                this.type = type;
            }
        }

        [Serializable]
        public class StateBase
        {
            public UnityEvent OnEnterEvent = new();
            public UnityEvent OnUpdateEvent = new();
            public UnityEvent OnExitEvent = new();
            public object _context;
            public IStateMachine machine;
            public GameObject gameObject { get { return machine.gameObject; } }

            

            public virtual void OnEnter() { }
            public virtual void OnUpdate() { }
            public virtual void OnExit() { }
        }


        [Serializable]
        public class StateReference : Reference
        {
            public string stateName;
            public StateBase state;

            public StateReference(Type type)
            {
                stateName = type.Name;
                state = (StateBase)Activator.CreateInstance(type);
                
                this.type = type;
            }
        }
    }
    [Serializable]
    public class State<T> : StateMachineManager.StateBase
    {
        public T Context { get{return (T)_context;} set{_context = value;}}
        public void SetState<TState>() where TState : StateMachineManager.StateBase
        {
            machine.SetState<TState>();
        }
    }
}