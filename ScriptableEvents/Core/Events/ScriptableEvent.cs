using System;
using System.Collections.Generic;
using UnityEngine;
/*using SaintsField.Playa;
using SaintsField;*/
using Remedy.Framework;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Remedy.Schematics.Utils;

public class ScriptableEventBase : ScriptableObjectWithID<ScriptableEvent>
{
    private static List<Type> _scriptableEventTypes;
    /// <summary>
    /// A Property that caches the ScriptableEvent Types as an Array so it can be used in the Editor.
    /// </summary>
    public static List<Type> ScriptableEventTypes => _scriptableEventTypes ??= typeof(ScriptableEventBase).GetInheritedTypes();
    /// <summary>
    /// If true, the Schematics system will use the original Scriptable Event Asset instead of instantiating one per instance that contains it. 
    /// </summary>
    public bool Global;

    /// <summary>
    /// Return the Event Type for the given Argument Type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Type GetEventTypeForArgumentType<T>()
    {
        foreach(var eventType in ScriptableEventTypes)
        {
            var baseType = eventType.BaseType;
            if(baseType.GetGenericArguments().Length > 0)
            {
                if (baseType.GetGenericArguments()[0] == typeof(T))
                    return eventType;
            }
        }
        return null;
    }
    /// <summary>
    /// Return the Event Type for the given Argument Type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Type GetEventTypeForArgumentType(Type type)
    {
        foreach (var eventType in ScriptableEventTypes)
        {
            var baseType = eventType.BaseType;
            if (baseType.GetGenericArguments().Length > 0)
            {
                if (baseType.GetGenericArguments()[0] == type)
                    return eventType;
            }
        }
        return null;
    }/*

    private static DropdownList<int> _sETypeDropdown;
    /// <summary>
    /// A Property that caches the Array of ScriptableEvent Types as a Dropdown for use in Editors.
    /// </summary>
    public static DropdownList<int> SETypeDropdown 
    {
        get
        {
            if (_sETypeDropdown != null) return _sETypeDropdown;

            var list = new DropdownList<int>();
            var typeList = ScriptableEventBase.ScriptableEventTypes;

            int i = 0;
            foreach (var type in typeList)
            {
                list.Add(type.Name.FormatNicely(), i);
                i++;
            }

            _sETypeDropdown = list;
            return list;
        }
    }*/

    private static Dictionary<int, UnityEngine.Object> _allSubscribers;
    public static Dictionary<int, UnityEngine.Object> AllSubscribers
    {
        get
        {
            if (_allSubscribers == null) _allSubscribers = new();
            LastModTime = Time.time;
            return _allSubscribers;
        }
    }

    private static float LastModTime = 0;
    protected static bool SubscribersAccessible => LastModTime != Time.time;

    [Tooltip("A time cap (in seconds) for which this Event can be called.")]
    public float MinimumTimeBetweenCalls = 0;
    private float _lastTime = 0;
    [Tooltip("Customizable message to Log when this Event is Invoked.")]
    public string Log = "";

    private object _valueAsObject;
    /// <summary>
    /// Converts the current value into an Object so that it can be used by certain systems, prominently Analytics.
    /// </summary>
    public virtual object ValueAsObject { get => _valueAsObject; }


    /// <summary>
    /// Returns the ID of the MonoBehaviour instance within the Subscriber ecosystem. If it isn't already in the system, 
    /// it is added and it's new ID is returned.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    protected void AddGlobalSubscriberID(UnityEngine.Object instance)
    {
        int instanceID = instance.GetInstanceID();
        if (!AllSubscribers.ContainsKey(instanceID))
            AllSubscribers[instanceID] = instance;
    }

    public virtual void Subscribe(UnityEngine.Object instance, Action<Union> action, AwaitCollection await, Type parent = null)
    { }

    public virtual void UnSubscribe(UnityEngine.Object instance)
    { }

    protected virtual void InternalInvoke(Union value)
    { }

    public void Invoke(Union value = default)
    {
        if (_lastTime + MinimumTimeBetweenCalls < Time.time)
        {
            if (!string.IsNullOrEmpty(Log)) Debug.Log(Log, this);

            InternalInvoke(value);
            _lastTime = Time.time;
        }

        //_valueAsObject = value;
    }

    void OnValidate()
    {
        _lastTime = 0;
        ValidateSubscribers();
    }

    protected virtual void ValidateSubscribers()
    { }

    [Serializable]
    public class EventToggler
    {
        public ScriptableEventBase Event;
        public bool Value = false;
    }

    [Serializable]
    public class IOBase
    {
        private static int IDCounter = 0;
        [HideInInspector]
        public string Name;
        [HideInInspector]
        /// <summary>
        /// The Default Asset Path to the Scriptable Events linked to the IOBase
        /// </summary>
        public string DefaultDirectoryPath;
        public static Dictionary<int, List<IOBase>> IOBaseInstances = new();
        /// <summary>
        /// Add to this function to return ScriptableEvents based on the ID of the IOBase. The Schematics Manager will search for the original
        /// IOBase and communicate the new list here.
        /// </summary>
        public static Action<int, List<ScriptableEventBase>> OnIOBaseInstantiated;
        /// <summary>
        /// The index of the ScriptableEvent.IOBase in the global Dictionary of Instances.
        /// </summary>
        [HideInInspector]
        public int ID = -1;

        public IOBase()
        {
            OnIOBaseInstantiated += InitializeEvents;
            IDCounter++;
            ID = IDCounter;
        }

        ~IOBase()
        {
            OnIOBaseInstantiated -= InitializeEvents;
        }

        void InitializeEvents(int id, List<ScriptableEventBase> events)
        {
            if(id == ID)
                IOEvents = events;
        }

        public virtual List<ScriptableEventBase> IOEvents { get; set; }

        public ScriptableEventBase this[int index]
        {
            get
            {
                return IOEvents[index];
            }
            set
            {
                if(IOEvents.Count < index)
                    IOEvents[index] = value;
            }
        }
    }

    public class OutputBase : IOBase
    { }

    /// <summary>
    /// A Collection of ScriptableEvents to Invoke in response to an event of a MonoBehaviour.
    /// This generic version of the Output simply passes an object value, so value types will be boxed, thus incur some GC Alloc.
    /// </summary>
    [Serializable]
    public class Output : OutputBase
    {
        public override List<ScriptableEventBase> IOEvents
        {
            get
            {
                return Events.ToList();
            }
            set
            {
                Events = value.ToArray();
            }
        }
        public ScriptableEventBase[] Events = new ScriptableEventBase[0];
        public int Channel = 0;
        [Tooltip("Before this output will fully invoke, it will await the invocation of each of the Await Events. It beginning listening after any of the given Events in the Output has Invoked.")]
        public AwaitCollection Await;

        public void Invoke(Union value = default)
        {
            AsyncInvoke(value).Forget();
        }
        protected UniTask<bool> AsyncInvoke(Union value = default)
        {
            foreach (var e in Events)
            {
                e?.Invoke(value);
            }
            return UniTask.FromResult(true);
        }

    }

    public class InputBase : IOBase
    { }

    /// <summary>
    /// A Collection of Events that are Subscribed to to perform a function within a MonoBehaviour. 
    /// This is the generic version of Input for Scriptable Events, which uses some reflection and caching to handle argument typing.
    /// </summary>
    [Serializable]
    public class Input : InputBase
    {
        public override List<ScriptableEventBase> IOEvents
        {
            get
            {
                return Subscriptions.ToList();
            }
            set
            {
                Subscriptions = value.ToArray();
            }
        }

        public ScriptableEventBase[] Subscriptions = new ScriptableEventBase[0];
        public int Channel = 0;
        [Tooltip("Events that are Awaiting before the given Event's functionality can perform. Listening starts after any Event in Subscriptions has been Invoked.")]
        public AwaitCollection Await;

        public void Subscribe(UnityEngine.Object instance, Action<object> action)
        {
            foreach (var sub in Subscriptions)
            {
                if (sub == null) continue;

                var type = sub.GetType();

                Type baseType = type;
                while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(ScriptableEvent<>)))
                {
                    baseType = baseType.BaseType;
                }

                if (baseType != null)
                {
                    var eventType = baseType.GetGenericArguments()[0];

                    // Use the cache system to get a fast subscription wrapper
                    if (SubscriptionWrapperCache.TrySubscribe(eventType, sub, instance, action, Await) == false)
                    {
                        Debug.LogError($"Failed to subscribe to ScriptableEvent<{eventType.Name}>.", instance);
                    }
                }
            }
        }

        public void UnSubscribe(UnityEngine.Object instance)
        {
            foreach (var sub in Subscriptions)
            {
                sub?.UnSubscribe(instance);
            }
        }
    }

    public static class SubscriptionWrapperCache
    {
        private static readonly Dictionary<Type, IWrapperInvoker> cache = new();

        public static bool TrySubscribe(Type eventType, ScriptableEventBase evt, UnityEngine.Object instance, Action<object> handler, AwaitCollection await)
        {
            if (!cache.TryGetValue(eventType, out var invoker))
            {
                // Create a generic invoker wrapper for T
                var invokerType = typeof(WrapperInvoker<>).MakeGenericType(eventType);
                invoker = (IWrapperInvoker)Activator.CreateInstance(invokerType);
                cache[eventType] = invoker;
            }

            return invoker.TrySubscribe(evt, instance, handler, await);
        }

        private interface IWrapperInvoker
        {
            bool TrySubscribe(ScriptableEventBase evt, UnityEngine.Object instance, Action<object> handler, AwaitCollection await);
        }

        private class WrapperInvoker<T> : IWrapperInvoker
        {
            public bool TrySubscribe(ScriptableEventBase evt, UnityEngine.Object instance, Action<object> handler, AwaitCollection await)
            {
                if (evt is not ScriptableEvent<T> typedEvent)
                    return false;

                Action<T> wrapper = val =>
                {
                    try { handler?.Invoke(val); }
                    catch (Exception e) { Debug.LogException(e); }
                };

                typedEvent.Subscribe(instance, wrapper, await);
                return true;
            }
        }
    }

    /// <summary>
    /// When an Output is Invoked, it Awaits the given Await Events
    /// When an Input is subscribed, each subscription Awaits the given Await Events
    /// </summary>
    [Serializable]
    public class AwaitCollection
    {
        private bool IsAwaiting = false;
        private bool HasPassed = false;
        public AwaitEvent[] Events = new AwaitEvent[0];

        public async UniTask<bool> Await(ScriptableEventBase scriptableEvent)
        {
            HasPassed = false;

            if (Events.Length == 0) return false;

            foreach(var ev in Events)
            {
                ev.Event.Subscribe(scriptableEvent, (Union val) => {
                    if(IsAwaiting)
                    {
                        ev.Triggered = true;
                    }
                }, null);
            }

            while (!HasPassed)
            {
                await UniTask.WaitForEndOfFrame();

                HasPassed = true;
                foreach (var ev in Events)
                {
                    if(!ev.Triggered)
                    {
                        HasPassed = false;
                        break;
                    }
                }
            }


            foreach (var ev in Events)
            {
                ev.Event.UnSubscribe(scriptableEvent);
            }

            return false;
        }

        [Serializable]
        public class AwaitEvent
        {
            public ScriptableEventBase Event;
            public bool Triggered = false;
        }
    }
}

/// <summary>
/// A Generically Typed Scriptable Object that is used as an Event to connect Inputs and Outputs of Components in Unity.
/// It includes the option to Asynchronously await the invocation of other Events for it itself to invoke, as well as for invoking Events that 
/// are the child of this Event. It also includes functionality for automatic Analytics tracking, controlled communication through Channels, Networked Invocation using Netcode for GameObjects,
/// and Visual Scripting nodes for both UVS and Remedy.Flow. 
/// </summary>
/// <typeparam name="T">The Type of the Value of the Scriptable Event.</typeparam>
public class ScriptableEvent<T> : ScriptableEventBase
{
    public int TypeIndex
    {
        get
        {
            int index = -1;
            for(int i = 0; i < ScriptableEventTypes.Count; i++)
            {
                var args = ScriptableEventTypes[i].BaseType.GetGenericArguments();
                if (args.Length > 0 && args[0] == typeof(T))
                {
                    index = i;
                }
            }
            return index;
        }
    }

    [Tooltip("A delay for the invokation of the Event, in Seconds")]
    public float Delay = 0;
    /// <summary>
    /// All Active Subscription to this Event.
    /// </summary> 
    public Dictionary<int, List<Subscription>> Subscribers = new();

    [Tooltip("If true, the Value set in the Inspector for this Event is passed to Invoke, instead of that argument passed to the Invoke method.")]
    public bool UseInternalValue = false;
    [Tooltip("The Value of the ScriptableEvent. If UseInternalValue is true, this Value is passed when the Event is Invoked, ignoring the value argument.")]
    public T Value;
    //[Button("Test")]
    public void ManualInvoke()
    {
        Invoke(Value);
    }

    /// <summary>
    /// The Current Value of the ScriptableEvent. This is updated every time Invoke is called for the Event.
    /// </summary>
    public T CurrentValue;
    /// <summary>
    /// The Last Value of the ScriptableEvent. This is updated evey time Invoke is called for the Event, but to the previous invocation's value.
    /// </summary>
    [HideInInspector]
    public T LastValue;

    /// <summary>
    /// Converts the current value into an Object so that it can be used by certain systems, prominently Analytics.
    /// </summary>
    public override object ValueAsObject { get => (object)CurrentValue; }

    [SerializeField]
    [Tooltip("The anaylitics information. This information is gathered if Analytics are enabled for the project.")]
    protected AnalyticsInfo Analytics;

    protected void OnEnable()
    {
        if (!Application.isPlaying)
            AllSubscribers.Clear();

        Subscribers.Clear();
    }

    /// <summary>
    /// Converts a ScriptableEventBase[] into the given <paramref name="argumentType"/> and updates the value of the <paramref name="target"/> array to that.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="eventsField"></param>
    /// <param name="inputEvents"></param>
    /// <param name="argumentType"></param>
    public static void SetEventsDynamically(object target, FieldInfo eventsField, List<ScriptableEventBase> inputEvents, Type argumentType)
    {
        Type eventType = ScriptableEventBase.GetEventTypeForArgumentType(argumentType);
        Type listType = typeof(List<>).MakeGenericType(eventType);

        var list = Activator.CreateInstance(listType) as System.Collections.IList;

        foreach (var ev in inputEvents)
        {
            if (ev == null) continue;
            if (eventType.IsAssignableFrom(ev.GetType()))
            {
                list.Add(ev);
            }
            else
            {
                Debug.LogWarning($"Skipping incompatible event type: {ev.GetType()} expected: {eventType}");
            }
        }

        // Convert to array
        MethodInfo toArrayMethod = listType.GetMethod("ToArray");
        var array = toArrayMethod.Invoke(list, null);

        // Assign via reflection
        try
        {
            eventsField.SetValue(target, array);
        }
        catch(Exception e)
        {
            Debug.LogError("Implicit IO Base Updating failed with error: " + e);
        }
    }

    public override void Subscribe(UnityEngine.Object instance, Action<Union> action, AwaitCollection await = null, Type parent = null) 
    {
        Subscribe(instance, (T value) =>
        {
            var unionVal = new Union();
            unionVal.Set<T>(value);
            action?.Invoke(unionVal);
        }, await, parent);
    }

    /// <summary>
    /// Removes all of the MonoBehaviour's Subscriptions to this Event.
    /// </summary>
    /// <param name="monoBehaviour"></param>
    public override void UnSubscribe(UnityEngine.Object monoBehaviour)
    {
        if (!Application.isPlaying) return;

        Span<int> _subsToRemove = stackalloc int[128];
        int removeCount = 0;

        foreach (var kvp in Subscribers)
        {
            var sub = kvp.Key;
            if (sub == monoBehaviour.GetInstanceID())
                _subsToRemove[removeCount++] = sub;
        }    

        for (int i = 0; i < removeCount; i++)
        {
            Subscribers.Remove(_subsToRemove[i]);
        }
    }

    /// <summary>
    /// Subscribe the given method to the UnityEvent of this ScriptableEvent. 
    /// Prefer to invoke this OnEnable of MonoBehaviours, as if the MonoBehaviour is Disabled it's subscription is 
    /// automatically removed.
    /// </summary>
    /// <param name="action"></param>
    public virtual void Subscribe(UnityEngine.Object instance, Action<T> action, AwaitCollection await = null, Type parentType = null)
    {
        if (!Application.isPlaying) return;

        AddGlobalSubscriberID(instance);
        int mbID = instance.GetInstanceID();
        var sub = new Subscription((T value) => { action.Invoke(value); }, parentType, await == null ? new AwaitCollection() : await);

        if (!Subscribers.ContainsKey(mbID))
            Subscribers.Add(mbID, new());
        Subscribers[mbID].Add(sub);
    }

    /// <summary>
    /// Sets the Parent of the given Child Component. This is only valid for Subscribing MB's that require a certain parent
    /// for the Evnet they're subscribed to invoke on them.
    /// </summary>
    /// <param name="thisComponent"></param>
    /// <param name="otherBehaviour"></param>
    public void UpdateParent(UnityEngine.Object child, UnityEngine.Object parent)
    {
        int childID = child.GetInstanceID();
        int parentID = parent.GetInstanceID();

        if (childID == -1 || (parentID == -1 && parent != null) || Subscribers[childID][0].ParentType != parent.GetType())
        {
            Debug.Log("Failure assigning a Parent for the Event.", child);
            return;
        }

        var parentSub = Subscribers[parentID];

        if (parent != null)
            if (!parentSub[0].Children.Contains(childID))
                parentSub[0].Children.Add(childID);
        else
        {
            foreach(var kvp in Subscribers)
            {
                var subList = kvp.Value;
                
                foreach(var sub in subList)
                {
                    sub.Children.Remove(childID);
                }
            }
        }
    }

    /// <summary>
    /// Returns True if the given MB Instance is already subscribed to this Event.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public bool IsSubscribed(UnityEngine.Object instance)
    {
        int mbID = instance.GetInstanceID();
        return Subscribers.ContainsKey(mbID);
    }

    public void Invoke(T value)
    {
        InternalInvoke(value);
    }

    protected virtual void InternalInvoke(T value)
    {
        CurrentValue = value;
        InvokeSubscribersIteratively(value);
        LastValue = value;
    }

    /// <summary>
    /// The initial Invokation for the ScriptableEvent
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected override void InternalInvoke(Union value = default)
    {
        T curValue;

        if (UseInternalValue)
            curValue = Value;
        else
            curValue = value.Get<T>();

        CurrentValue = curValue;

        InvokeSubscribersIteratively(curValue);

        LastValue = curValue;
    }

    private void InvokeSubscribersIteratively(T value)
    {
        Span<int> _workingSubs = stackalloc int[128];
        int subCount = 0;

        foreach (var kvp in Subscribers)
        {
            var key = kvp.Key;
            if (subCount >= _workingSubs.Length) break;
            _workingSubs[subCount++] = key;
        }

        Span<int> processingStack = stackalloc int[128];
        int stackTop = 0;

        for (int i = 0; i < subCount; i++)
        {
            int rootID = _workingSubs[i];
            var rootSub = Subscribers[rootID];

            if (rootSub[0].ParentType != null) continue;

            // Push to stack
            if (stackTop < processingStack.Length)
                processingStack[stackTop++] = rootID;

            while (stackTop > 0)
            {
                int currentID = processingStack[--stackTop];
                var subList = Subscribers[currentID];

                try
                {
                    foreach(var sub in subList)
                    {
                        InvokeSubscription(sub, value);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e, this);
                }

                if (subList[0].Children != null)
                {
                    foreach (var childID in subList[0].Children)
                    {
                        if (stackTop < processingStack.Length)
                            processingStack[stackTop++] = childID;
                    }
                }
            }
        }
    }

    protected virtual void InvokeSubscription(Subscription sub, T value)
    {
        //await sub.Await.Await(this);
        sub.Action?.Invoke(value);
    }

    [Serializable]
    public class Subscription
    {
        [Tooltip("The Instance for this Subscription.")]
        //[ReadOnly]
        [SerializeField]
        public UnityEngine.Object Parent;
        public Action<T> Action;
        /// <summary>
        /// Filled with Scoped Subscriptions, the ID's of Components who have made this Subscriber it's parent.
        /// </summary>
        public List<int> Children = new();
        /// <summary>
        /// The parent Type of recieved from a ScopedSubscription
        /// </summary>
        public Type ParentType;
        /// <summary>
        /// Recieved directly from Input. These will be awaited within the Invoke method for the given subscription.
        /// </summary>
        public AwaitCollection Await;

        public Subscription(Action<T> action, Type parentType, AwaitCollection await)
        {
            Action = action;
            ParentType = parentType;
            Await = await;
        }
    }


    /// <summary>
    /// A Collection of Events that are Subscribed to to perform a function within a MonoBehaviour
    /// </summary>
    [Serializable]
    public new class Input : InputBase
    {
        public int TypeIndex
        {
            get
            {
                int index = -1;
                for (int i = 0; i < ScriptableEventTypes.Count; i++)
                {
                    var args = ScriptableEventTypes[i].BaseType.GetGenericArguments();
                    if (args.Length > 0 && args[0] == typeof(T))
                    {
                        index = i;
                    }
                }
                return index;
            }
        }

        public override List<ScriptableEventBase> IOEvents
        {
            get
            {
                return Subscriptions.ToList<ScriptableEventBase>();
            }
            set
            {
                FieldInfo field = this.GetType().GetField(nameof(Subscriptions));
                SetEventsDynamically(this, field, value, typeof(T));
            }
        }

        public T OriginalValue;
        public T CurrentValue => Subscriptions.Length > 0 ? Subscriptions[0].CurrentValue : OriginalValue;
        
        [Tooltip("The Events that trigger the passed functionality after Subscribing. These are Invoked from the Output defined within another Component.")]
        public ScriptableEvent<T>[] Subscriptions = new ScriptableEvent<T>[0];
        [Tooltip("Only the Outputs that share the same Channel as this Input can fire the functionality subscribed to the Input..")]
        public int Channel = 0;
        [Tooltip("Events that are Awaiting before the given Event's functionality can perform. Listening starts after any Output Event in Subscriptions has been Invoked.")]
        public AwaitCollection Await;

        /// <summary>
        /// Adds the instance to the as a subscriber to each of the given Events within the Subscriptions list.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        public void Subscribe(UnityEngine.Object instance, Action<T> action)
        {
            foreach (var sub in Subscriptions)
            {
                sub.Subscribe(instance, action, Await);
            }
        }
        public void Unsubscribe(UnityEngine.Object instance)
        {
            foreach (var sub in Subscriptions)
            {
                sub.UnSubscribe(instance);
            }
        }
    
        /// <summary>
        /// Copies the properties of a Parent Input to this one, forcing it to use the same Output > Input relationship as it's parent.
        /// </summary>
        /// <param name="parentInput"></param>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        public void ManualSubscribe(Input parentInput, UnityEngine.Object instance, Action<T> action, bool mergeAwait = true)
        {
            Unsubscribe(instance);

            Subscriptions = parentInput.Subscriptions;

            if (mergeAwait && Await != null && parentInput.Await != null)
                Await.Events.AddRange(parentInput.Await.Events);
            else if(parentInput.Await != null)
                Await = parentInput.Await;

            Subscribe(instance, action);
        }
    }
    
    /// <summary>
    /// A Collection of ScriptableEvents to Invoke in response to an event of a MonoBehaviour
    /// </summary>
    [Serializable]
    public new class Output : OutputBase
    {
        public int TypeIndex
        {
            get
            {
                int index = -1;
                for (int i = 0; i < ScriptableEventTypes.Count; i++)
                {
                    var args = ScriptableEventTypes[i].BaseType.GetGenericArguments();
                    if (args.Length > 0 && args[0] == typeof(T))
                    {
                        index = i;
                    }
                }
                return index;
            }
        }

        public override List<ScriptableEventBase> IOEvents
        {
            get
            {
                return Events.ToList<ScriptableEventBase>();
            }
            set
            {
                FieldInfo field = this.GetType().GetField(nameof(Events));
                SetEventsDynamically(this, field, value, typeof(T));
            }
        }

        [Tooltip("When the Output is Invoked, even ScriptableEvent in this Output is Invoked, and subsequentally calls any subscribed functionality from other Component's Input.")]
        public ScriptableEvent<T>[] Events = new ScriptableEvent<T>[0];
        [Tooltip("Only Inputs that share the same Channel as this Output can be Invoked by this Output.")]
        public int Channel = 0;
        [Tooltip("Before this output will fully invoke, it will await the invocation of each of the Await Events. It beginning listening after any of the given Events in the Output has Invoked.")]
        public AwaitCollection Await;

        /// <summary>
        /// Iterates through all the Events in the Output and Invokes them, subsequently declaring functionality for 
        /// Inputs that share one of the same Events.
        /// </summary>
        /// <param name="value"></param>
        public void Invoke(T value = default)
        {
            AsyncInvoke(value).Forget();
        }
        protected UniTask<bool> AsyncInvoke(T value = default)
        {
            foreach (var e in Events)
            {
                e?.Invoke(value);
            }
            return UniTask.FromResult(true);
        }


        /// <summary>
        /// Merges (or optionally overrides) this Output with the given Parent Output. This will cause this Output to call the Events of the Parent Output.
        /// </summary>
        /// <param name="parentOutput"></param>
        /// <param name="overrideOriginal"></param>
        public void MergeOutput(Output parentOutput, bool overrideOriginal = false)
        {
            if(overrideOriginal)
                Events = parentOutput.Events;
            else
                Events.AddRange(parentOutput.Events);
        }
    }

    /// <summary>
    /// Information automatically tracked by this ScriptableEvent, vastly simplifying game state analysis.
    /// </summary>
    [Serializable]
    protected class AnalyticsInfo
    {
        [Tooltip("If True, this Event and any information added to it are sent to the AnalyticsService when it's Invoked")]
        public bool Analyze;
        [Tooltip("The Current Value of each of these Events is logged along with the given ScriptableEvent, but as a parameter of it within Analytics. Therefore allowing quick grouping of relative gameplay state variables.")]
        public ScriptableEventBase[] PayloadItems;

        protected AnalyticsInfo()
        {
            PayloadItems = new ScriptableEventBase[0];
        }

        /// <summary>
        /// Generates an Analytic Payload that includes the names of the PayloadItems as Parameters names, and their CurrentValue as the param value.
        /// </summary>
        /// <returns></returns>
        public void GeneratePayload()
        {
            foreach(var item in PayloadItems)
            {
                // TODO: Hold a local instance of the analyis, then call the SetParam for it to generate the Analysis Payload.
                // var analysisItem;
                // analysisItem.SetParam(item.Name, item.ValueAsObject);
            }
        }
    }

    /*    public class ScriptableAnalysis : Unity.Services.Analytics.Event
        {
            public ScriptableAnalysis(string name) : base(name)
            {
            }

            public void SetParam(string name, object value)
            {
                SetParameter(name, value)'
            }
        }*/


}

// TODO: Make this Netcode ScriptableEvent Invoke a NetworkEvent that peer's copies subscribe to, thus enabling Networked Scriptable Events.
public class ScriptableNetworkEvent<T> : ScriptableEvent<T>
{ }

[CreateAssetMenu(menuName = "Remedy Toolkit/Scriptable Logic/New Event")]
public class ScriptableEvent : ScriptableEvent<Union> // TODO: Replace with Union
{ }