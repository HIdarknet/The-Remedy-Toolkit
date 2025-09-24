using BlueGraph;
using Remedy.Schematics;
using System;
using UnityEngine;
using Remedy.Schematics.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IIFlowNode { }

/*
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
}*/

[Serializable]
[Node(Name = "Invoke Event", Path = "ScriptableEvents/Events"), Tags("Object")]
public class InvokeScriptableEvent : SchematicActionNode
{
    [Editable]
    public ScriptableEventBase Event;
    [Input]
    public Union Input;

    protected override void OnTrigger(bool awaiting = false)
    {
        var input = GetInputValue<Union>(nameof(Input), default);
        Event?.Invoke(input);
    }
}

[Serializable]
[Node(Name = "On Invoke Event", Path = "ScriptableEvents/Events"), Tags("Object")]
public class OnScriptableEventInvoked : SchematicEventNode
{

    [Editable]
    public ScriptableEventBase Event;
    [Output]
    public Union Output;

    public override object OnRequestValue(Port port)
    {
        return Output;
    }
}
 
[Node(Name = "Set Variable", Path = "Schematic/Variables"), Tags("Object")]
public class SetScriptableVariable : SchematicActionNode
{
    [Editable]
    public ScriptableVariable Target;
    [Input]
    public Union Value;

    protected override void OnTrigger(bool awaiting = false)
    {
        var newVal = GetInputValue<Union>(nameof(Value), default);
        Target.Value = newVal;
    } 
}