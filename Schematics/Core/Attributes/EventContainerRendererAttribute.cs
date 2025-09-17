using System;

[AttributeUsage(AttributeTargets.Field)]
public class EventContainerRendererAttribute : CustomFieldRendererAttribute
{
    public Type DefaultType;
    public string DefaultName;
    public bool Modifiable;
    public bool CanChangeType;
    public ScriptableEventBase.IOBase IOBase;

    public EventContainerRendererAttribute(Type defaultType, string defaultName, bool modifiable = false, bool canChangeType = true)
    {
        DefaultType = defaultType;
        DefaultName = defaultName;
        Modifiable = modifiable;
        CanChangeType = canChangeType;
    }
}