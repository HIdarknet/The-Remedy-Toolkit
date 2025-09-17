using Remedy.Schematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

internal class SchematicEditorEventManager
{
    private List<ScriptableEventReference> _workingSet;
    public List<ScriptableEventReference> WorkingSet => _workingSet ??= new(); 

    private SchematicGraph _graph;

    internal SchematicEditorEventManager(SchematicGraph graph)
    {
        _graph = graph;
    }

    internal ScriptableEventReference GetOrCreateEventReference(UnityEngine.Object obj, string propertyPath, MemberWrapper field, Type defaultType, string defaultName)
    {
        var objID = GlobalObjectId.GetGlobalObjectIdSlow(obj);

        var existing = FindEventReference(obj, propertyPath, field.Name);
        if (existing != null)
            return existing;
        else
        {
            var newEventRef = new ScriptableEventReference(objID, propertyPath, field, defaultType, defaultName, UpdateNodesForEvent);
            WorkingSet.Add(newEventRef);
            return newEventRef;
        }
    }

    private ScriptableEventReference FindEventReference(UnityEngine.Object obj, string propertyPath, string fieldName)
    {
        var objID = GlobalObjectId.GetGlobalObjectIdSlow(obj).targetObjectId;

        foreach (var ser in WorkingSet)
        {
            if (ser.Property.ObjID.targetObjectId == objID && ser.Property.Path == propertyPath && ser.FieldName == fieldName)
                return ser;
        }

        return null;
    }

    /// <summary>
    /// Iterates through the Nodes in the Node to ScriptableEventReference caches to Update their Event References.
    /// </summary>
    private void UpdateNodesForEvent(GlobalObjectId oldID, GlobalObjectId newID)
    {
        var onInvokeNodes = _graph.Nodes.OfType<FlowOnInvokeBase>().Where(node => node.EventID.targetObjectId == oldID.targetObjectId);
        var invokeNodes = _graph.Nodes.OfType<FlowInvokeBase>().Where(node => node.EventID.targetObjectId == oldID.targetObjectId);

        foreach(var node in onInvokeNodes)
        {
            node.EventID = newID;
        }
        foreach (var node in invokeNodes)
        {
            node.EventID = newID;
        }
    }

}