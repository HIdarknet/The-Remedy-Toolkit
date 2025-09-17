using System;

[AttributeUsage(AttributeTargets.Field)]
public class IdentityListRendererAttribute : CustomFieldRendererAttribute
{
    public EventListIdentifierType IdentifierType;
    public string Identifier;
    public string[] ExtraFields;
    public string Options;
    public int Depth;
    public string AddItem;
    public string RemoveItem;
    public string FoldoutTitle;
    public string ItemName;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifierType">Determines the kind of UIElement to use for the creation of new Items.</param>
    /// <param name="identifierField">The Name of the Field within each List Item that acts as the identifier.</param>
    /// <param name="depth">Increment for each placement of this attribute reletive to the nest of the class the field is included in to make it visually distinct from the other nests in the Hierarchy</param>
    /// <param name="extraFields">Names of Fields in the class of items within the list that should also be drawn in the IODock.</param>
    /// <param name="options">A reference to a Field, Property, or Method that returns the Options available for a Dropdown.</param>
    public IdentityListRendererAttribute(EventListIdentifierType identifierType, string identifierField, int depth, string addItemCallback = null, string removeItemCallback = null, string[] extraFields = null, string options = null, string foldoutTitle = "", string itemName = "")
    {
        IdentifierType = identifierType;
        Identifier = identifierField;
        ExtraFields = extraFields;
        Options = options;
        Depth = depth;
        AddItem = addItemCallback;
        RemoveItem = removeItemCallback;
        FoldoutTitle = foldoutTitle;
        ItemName = itemName;
    }
}

public enum EventListIdentifierType
{
    Name,
    Dropdown
}
