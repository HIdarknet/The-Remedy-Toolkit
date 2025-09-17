using BlueGraph;
using Remedy.Schematics;
using System;
using System.Collections.Generic;
using UnityEngine;
using Remedy.Framework;
using Remedy.Schematics.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IIFlowNode { }

[Serializable]
public class FlowInvokeBase : SchematicActionNode, IIFlowNode
{
    private static List<Type> _nodeTypes;
    public static List<Type> NodeTypes => _nodeTypes ??= typeof(FlowInvokeBase).GetInheritedTypes();
    public virtual ScriptableEventBase EventBase { get => null; set => _ = value; }

#if UNITY_EDITOR
    [SerializeField]
    public GlobalObjectId EventID;
#endif
}

[Serializable]
public class FlowOnInvokeBase : SchematicEventNode, IIFlowNode
{
    private static List<Type> _nodeTypes;
    public static List<Type> NodeTypes => _nodeTypes ??= typeof(FlowOnInvokeBase).GetInheritedTypes();
    public virtual ScriptableEventBase EventBase { get => null; set => _ = value; }

    public (UnityEngine.Object instance, Action<Union> action) Subscription = new();

#if UNITY_EDITOR
    [SerializeField]
    public GlobalObjectId EventID;
#endif
}

[Serializable]
[Node(Name = "Invoke " + nameof(TEvent), Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventFlow<TEvent, TValue> : FlowInvokeBase where TEvent : ScriptableEvent<TValue>
{
    public override ScriptableEventBase EventBase
    {
        get
        {
            return Event;
        }
        set
        {
            if (value == null) return;
            if (typeof(TEvent).IsAssignableFrom(value.GetType()))
                Event = (TEvent)value;
        }
    }

    [Editable]
    public TEvent Event;
    [Input]
    public TValue Input;


    protected override void OnTrigger(bool awaiting = false)
    {
        var input = GetInputValue<TValue>(nameof(Input), default);
        Event?.Invoke(input);
    }
}


[Serializable]
[Node(Name = "On Invoke " + nameof(TEvent), Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventInvokedFlow<TEvent, TValue> : FlowOnInvokeBase where TEvent : ScriptableEvent<TValue>
{
    public override ScriptableEventBase EventBase
    {
        get
        {
            return Event;
        }
        set
        {
            if(typeof(TEvent).IsAssignableFrom(value.GetType()))
                Event = (TEvent)value;
        }
    }

    [Editable]
    public TEvent Event;
    [Output]
    public TValue Output;

    public override object OnRequestValue(Port port)
    {
        return Output;
    }


    protected override void OnCacheUpdate()
    {
        EventBase?.Subscribe(Subscription.instance, Subscription.action, null);
    }
}

// bool
[Node(Name = "Invoke ScriptableEvent<bool>", Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventBoolFlow : InvokeScriptableEventFlow<ScriptableEventBoolean, bool>
{
}

[Node(Name = "On Invoked ScriptableEvent<bool>", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventBoolInvokedFlow : OnScriptableEventInvokedFlow<ScriptableEventBoolean, bool>
{
}

// float
[Node(Name = "Invoke ScriptableEvent<float>", Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventFloatFlow : InvokeScriptableEventFlow<ScriptableEventFloat, float>
{
}

[Node(Name = "On Invoked ScriptableEvent<float>", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventFloatInvokedFlow : OnScriptableEventInvokedFlow<ScriptableEventFloat, float>
{
}

// GameObject
[Node(Name = "Invoke ScriptableEvent<GameObject>", Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventGameObjectFlow : InvokeScriptableEventFlow<ScriptableEventGameObject, GameObject>
{
}

[Node(Name = "On Invoked ScriptableEvent<GameObject>", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventGameObjectInvokedFlow : OnScriptableEventInvokedFlow<ScriptableEventGameObject, GameObject>
{
}

// Int
[Node(Name = "Invoke ScriptableEvent<int>", Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventIntFlow : InvokeScriptableEventFlow<ScriptableEventInt, int>
{
}

[Node(Name = "On Invoked ScriptableEvent<int>", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventIntInvokedFlow : OnScriptableEventInvokedFlow<ScriptableEventInt, int>
{
}

// RaycastHit
[Node(Name = "Invoke ScriptableEvent<RaycastHit>", Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventRaycastHitFlow : InvokeScriptableEventFlow<ScriptableEventRaycastHit, RaycastHit>
{
}

[Node(Name = "On Invoked ScriptableEvent<RaycastHit>", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventIRaycastHitInvokedFlow : OnScriptableEventInvokedFlow<ScriptableEventRaycastHit, RaycastHit>
{
}

// Vector2
[Node(Name = "Invoke ScriptableEvent<Vector2>", Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventVector2Flow : InvokeScriptableEventFlow<ScriptableEventVector2, Vector2>
{
}

[Node(Name = "On Invoked ScriptableEvent<Vector2>", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventVector2InvokedFlow : OnScriptableEventInvokedFlow<ScriptableEventVector2, Vector2>
{
}

// Vector3
[Node(Name = "Invoke ScriptableEvent<Vector3>", Path = "ScriptableEvents/Actions"), Tags("Object")]
public class InvokeScriptableEventVector3Flow : InvokeScriptableEventFlow<ScriptableEventVector3, Vector3>
{
}

[Node(Name = "On Invoked ScriptableEvent<Vector3>", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventVector3InvokedFlow : OnScriptableEventInvokedFlow<ScriptableEventVector3, Vector3>
{
}