using Remedy.Framework;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;

[FieldRendererTarget(typeof(IdentityListRendererAttribute))]
public class IdentityListContainerRenderer : FieldRenderer
{
    private IdentityListRendererAttribute _listAttr;

    private enum ParentTypes
    {
        List,
        Array
    }
    private ParentTypes ParentType;


    public IdentityListContainerRenderer(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, MemberWrapper fieldInfo, CustomFieldRendererAttribute attr, string path, List<Action> onModified) : base(window, obj, parent, target, fieldInfo, attr, path, onModified)
    {
        _onModified.Add(() =>
        {
            if (ParentType == ParentTypes.Array)
            {
                _field.SetValue(_parent, _asArray);
            }
            else if (ParentType == ParentTypes.List)
            {
                _field.SetValue(_parent, _asList);
            }  
        });
    }

    private IList _asList => _value as IList;
    private Array _asArray => _value as Array;

    private VisualElement _container;
    private Foldout _containerFoldout;
    private bool _hasAddedModify = false;

    private Color _borderShade = new Color(0, 0, 0, 0.25f);

    protected override void Redraw()
    {
        if(!_hasAddedModify)
        {
            _onModified.Add(() =>
            {
                if (ParentType == ParentTypes.Array)
                {
                    _field.SetValue(_parent, _asArray);
                }
                else if (ParentType == ParentTypes.List)
                {
                    _field.SetValue(_parent, _asList);
                }
            });
            _hasAddedModify |= true;
        }

        _listAttr = (IdentityListRendererAttribute)_attr;

        _container = new VisualElement()
        {
            style =
            {
                backgroundColor = SchematicGraphEditorWindow.ColorLookupHierarchichal[_listAttr.Depth][true],
                borderLeftWidth = 2,
                borderTopWidth = 2,
                borderBottomWidth = 2,

                borderLeftColor = _borderShade,
                borderTopColor = _borderShade,
                borderBottomColor = _borderShade,
            }
        };

        _containerFoldout = new Foldout()
        {
            text = _listAttr.FoldoutTitle
        };

        var label = _containerFoldout.Q<Label>();
        if (label != null)
        {
            label.style.color = new StyleColor(new Color(1f, 1f, 1f, 0.5f));
        }
        _container.Add(_containerFoldout);

        Clear();
        GetListOrArray();
        AddCurrentItemsContainer();
        AddNewItemContainer();

        Add(_container);

    }

    private void GetListOrArray()
    {
        if (_value is Array)
            ParentType = ParentTypes.Array;
        else if (_value is IList)
            ParentType = ParentTypes.List;
    }

    private void AddCurrentItemsContainer()
    {
        var container = new VisualElement();

        IList list;
        if(_asArray != null)
        {
            list = _asArray.Cast<object>().ToList();
            ParentType = ParentTypes.Array;
        }
        else
        {
            list = _asList;
            ParentType = ParentTypes.List;
        }
        
        bool step = false;
        int i = 0;
        foreach(var item in list)
        {
            var itemContainer = new VisualElement();

            step = !step;
            var itemFoldout = new PersistentFoldout("Global", "InputMap_" + _value.ToString() + "_Input_" + item.GetField(_listAttr.Identifier).ToString())
            {
                text = _listAttr.ItemName + ": " + item.GetField(_listAttr.Identifier).ToString(),
                style =
                {
                    backgroundColor = SchematicGraphEditorWindow.ColorLookupHierarchichal[_listAttr.Depth][step],

                    borderLeftWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,

                    borderLeftColor = _borderShade,
                    borderTopColor = _borderShade,
                    borderBottomColor = _borderShade,
                }
            };

            string elementPath;

            if (string.IsNullOrEmpty(_path))
                elementPath = $"{_field.Name}.Array.data[{i}]";
            else
                elementPath = $"{_path}.{_field.Name}.Array.data[{i}]";

            i++;

            var removeItemButton = new Button(() =>
            {
                Remove(item);
                foreach(var action in _onModified)
                {
                    action?.Invoke();
                }
                AttemptPrefabRefresh();
            })
            {
                text = "-",
                style =
                {
                    backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Remove"],
                    position = Position.Absolute,
                    top = -1,
                    right = 0,
                    width = 20,
                    height = 20,
                }
            };
            removeItemButton.EnableInteractionHoverHighlight(SchematicGraphEditorWindow.ColorLookupFunctional["Remove"], SchematicGraphEditorWindow.ColorLookupFunctional["RemoveHighlight"]);

            (bool drawFields, VisualElement fieldsContainer) = IODockRegistry.RenderMultipleFields(_window, _object, item, elementPath, onModified: _onModified);
            if(drawFields) itemFoldout.Add(fieldsContainer);

            EnableItemChange(itemFoldout, item);

            itemContainer.Add(itemFoldout);
            itemContainer.Add(removeItemButton);

            container.Add(itemContainer);
        }

        _containerFoldout.Add(container);
    }

    private void Remove(object item)
    {
        if(ParentType == ParentTypes.List)
        {
            _asList.Remove(item);
        }
        else if(ParentType == ParentTypes.Array)
        {
            var elementType = GetElementType(_value);
            var newArray = Array.CreateInstance(elementType, _asArray.Length - 1);
            int index = 0;
            foreach (var arrayItem in _asArray)
            {
                if (!ReferenceEquals(arrayItem, item))
                    newArray.SetValue(arrayItem, index++);
            }

            _field.SetValue(_parent, newArray);
            _value = _field.GetValue(_parent);
        }

        //_listAttr.RemoveItem?.Invoke(item);
        Redraw();
    }

    private void AddNewItemContainer()
    {
        var newItemContainer = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexGrow = 1,
                width = Length.Percent(100),
                //backgroundColor = Color.gray1 * new Color(1,1,1,0.25f),
            }
        };

        if (_field != null)
        {
            VisualElement IdentifierInput = null;

            if (_listAttr.IdentifierType == EventListIdentifierType.Name)
            {
                IdentifierInput = new TextField()
                {
                    value = "New " + _listAttr.ItemName
                };
            }
            else if (_listAttr.IdentifierType == EventListIdentifierType.Dropdown)
            {
                var optionsObj = _parent.GetType().GetProperty(_listAttr.Options)?.GetValue(_parent);

                if (_listAttr.Options.StartsWith("./"))
                {
                    optionsObj = _object.GetType().GetProperty(_listAttr.Options.Replace("./", ""))?.GetValue(_object);
                }

                if (optionsObj == null)
                {
                    optionsObj = _parent.GetType().GetMethod(_listAttr.Options)?.Invoke(_parent, null);
                }

                if (optionsObj != null)
                {
                    var enumerable = optionsObj as IEnumerable;
                    var optionList = enumerable?.Cast<object>().ToList();
                    var defaultValue = optionList.FirstOrDefault();


                    if (optionList != null && optionList.Count > 0)
                    {
                        IdentifierInput = new PopupField<object>(
                            optionList,
                            defaultValue, 
                            t => t?.ToString(),
                            t => t?.ToString()
                        );
                    }
                }
            }

            if (IdentifierInput != null)
            {
                IdentifierInput.style.flexGrow = 1;


                IdentifierInput.style.borderBottomRightRadius = 0;
                IdentifierInput.style.borderBottomLeftRadius = 0;
                IdentifierInput.style.borderTopLeftRadius = 0;
                IdentifierInput.style.borderTopRightRadius = 0;

                IdentifierInput.style.marginLeft = 0;
                IdentifierInput.style.marginRight = 0;

                var addItemButton =
                new Button(() =>
                {
                        //AssetDatabase.StartAssetEditing();

                        object identifierValue = IdentifierInput switch
                        {
                            TextField tf => tf.value,
                            PopupField<object> pf => pf.value,
                            _ => null
                        };

                        if (identifierValue == null)
                            return;

                        var elementType = GetElementType(_value);
                        var newItem = Activator.CreateInstance(elementType);
                        newItem.SetField(_listAttr.Identifier, identifierValue);

                        if (ParentType == ParentTypes.Array)
                        {
                            var newArray = Array.CreateInstance(elementType, _asArray.Length + 1);
                            Array.Copy(_asArray, newArray, _asArray.Length);
                            newArray.SetValue(newItem, _asArray.Length);

                            _field.SetValue(_parent, newArray);
                            _value = _field.GetValue(_parent);
                        }
                        else if (ParentType == ParentTypes.List)
                        {
                            _asList.Add(newItem);
                        }

                        foreach (var action in _onModified)
                        {
                            action?.Invoke();
                        }
                        
                        AttemptPrefabRefresh();

                        Redraw();
                })
                {
                    text = "Add",
                    style =
                    {
                        borderBottomRightRadius = 0,
                        borderBottomLeftRadius = 0,
                        borderTopLeftRadius = 0,
                        borderTopRightRadius = 0,

                        marginLeft = 0,
                        marginRight = 0,
                    }
                };

                newItemContainer.Add(IdentifierInput);
                newItemContainer.Add(addItemButton);

                _containerFoldout.Add(newItemContainer);
            }
        }
    }

    private static Type GetElementType(object parent)
    {
        var parentType = parent.GetType();
        if (parentType.IsArray)
            return parentType.GetElementType();

        var iListType = parentType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));

        return iListType?.GetGenericArguments()[0] ?? typeof(object);
    }


    private void EnableItemChange(Foldout foldout, object item)
    {
        var label = foldout.Q<Label>();

        if (label == null)
            return;

        label.RegisterCallback<MouseDownEvent>(evt =>
        {
            evt.StopImmediatePropagation();
            if (evt.clickCount == 2 && evt.button == 0)
            {
                if (_listAttr.IdentifierType == EventListIdentifierType.Name)
                    BeginNameChange(label, item);

                if (_listAttr.IdentifierType == EventListIdentifierType.Dropdown)
                    BeginDropdownChange(label, item);
            }
        });

        void BeginNameChange(Label label, object item)
        {
            string oldName = label.text.Replace(_listAttr.ItemName + ": " , "");

            var textField = new TextField
            {
                value = oldName
            };
            textField.style.flexGrow = 1;
            textField.style.marginRight = 20;
            textField.SelectAll();

            var parent = label.parent;
            int index = parent.IndexOf(label);
            parent.Insert(index, textField);
            label.RemoveFromHierarchy();

            void EndRename()
            {
                string newName = textField.value.Trim();
                if (!string.IsNullOrEmpty(newName) && newName != oldName)
                {
                    var identiferField = item.GetType().GetField(_listAttr.Identifier);
                    identiferField.SetValue(item, newName);
                }

                var elementType = GetElementType(_value);
                if (ParentType == ParentTypes.Array)
                {
                    int itemIndex = Array.IndexOf(_asArray, item);

                    var newArray = Array.CreateInstance(elementType, _asArray.Length);
                    Array.Copy(_asArray, newArray, _asArray.Length);

                    item.SetField(_listAttr.Identifier, newName);

                    newArray.SetValue(item, itemIndex);

                    _field.SetValue(_parent, newArray);
                    _value = _field.GetValue(_parent);
                }
                else if (ParentType == ParentTypes.List)
                {
                    int itemIndex = Array.IndexOf(_asArray, item);
                    item.SetField(_listAttr.Identifier, newName);
                    _asList[itemIndex] = item;
                }

                label.text = _listAttr.ItemName + ": " + newName;
                parent.Insert(index, label);
                textField.RemoveFromHierarchy();

                AttemptPrefabRefresh();
            }

            textField.RegisterCallback<FocusOutEvent>(_ => EndRename());
            textField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    EndRename();
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    parent.Insert(index, label);
                    textField.RemoveFromHierarchy();
                    evt.StopPropagation();
                }
            });

            textField.Focus();
        }

        void BeginDropdownChange(Label label, object item)
        {
            string oldName = label.text.Replace(_listAttr.ItemName + ": ", "");

            var optionsObj = _parent.GetType().GetProperty(_listAttr.Options)?.GetValue(_parent);

            if(_listAttr.Options.StartsWith("./"))
            {
                optionsObj = _object.GetType().GetProperty(_listAttr.Options.Replace("./", ""))?.GetValue(_object);
            }

            if (optionsObj == null)
            {
                optionsObj = _parent.GetType().GetMethod(_listAttr.Options)?.Invoke(_parent, null);
            }

            if (optionsObj != null)
            {
                var enumerable = optionsObj as IEnumerable;
                var optionList = enumerable?.Cast<object>().ToList();

                var identiferField = item.GetType().GetField(_listAttr.Identifier);
                optionList.Insert(0, identiferField.GetValue(item));

                var defaultValue = optionList.FirstOrDefault();

                if (optionList != null && optionList.Count > 0)
                {
                    var dropdown = new PopupField<object>
                    (
                        optionList,
                        defaultValue,
                        t => t?.ToString(),
                        t => t?.ToString()
                    );

                    dropdown.style.marginRight = 20;

                    dropdown.RegisterValueChangedCallback(evt =>
                    {
                        identiferField.SetValue(item, evt.newValue); 
                        Redraw();
                    });

                    dropdown.style.flexGrow = 1;

                    var parent = label.parent;
                    int index = parent.IndexOf(label);
                    parent.Insert(index, dropdown);
                    label.RemoveFromHierarchy();

                    void EndDropdownChange()
                    {
                        var elementType = GetElementType(_value);
                        if (ParentType == ParentTypes.Array)
                        {
                            int itemIndex = Array.IndexOf(_asArray, item);

                            var newArray = Array.CreateInstance(elementType, _asArray.Length);
                            Array.Copy(_asArray, newArray, _asArray.Length);

                            item.SetField(_listAttr.Identifier, dropdown.value);

                            newArray.SetValue(item, itemIndex);

                            _field.SetValue(_parent, newArray);
                            _value = _field.GetValue(_parent);
                        }
                        else if (ParentType == ParentTypes.List)
                        {
                            int itemIndex = Array.IndexOf(_asArray, item);
                            item.SetField(_listAttr.Identifier, dropdown.value);
                            _asList[itemIndex] = item;
                        }

                        parent.Insert(index, label);
                        dropdown.RemoveFromHierarchy();

                        AttemptPrefabRefresh();

                        Redraw();
                    }

                    dropdown.RegisterCallback<FocusOutEvent>(_ => EndDropdownChange());
                    dropdown.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        {
                            EndDropdownChange();
                            evt.StopPropagation();
                        }
                        else if (evt.keyCode == KeyCode.Escape)
                        {
                            parent.Insert(index, label);
                            dropdown.RemoveFromHierarchy();
                            evt.StopPropagation();
                        }
                    });

                    dropdown.Focus();
                }
            }
        }
    }
    private void AttemptPrefabRefresh()
    {
        if (string.IsNullOrEmpty(_prefabPath)) return;
        PrefabUtility.SaveAsPrefabAsset(_window.Prefab, _prefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PrefabRefreshUtility.ReimportAndResetPrefab(_window.Prefab);
    }

    private GameObject GetPrefabFromTargetId(ulong targetPrefabId)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var goId = GlobalObjectId.GetGlobalObjectIdSlow(prefab);
                if (goId.targetPrefabId == targetPrefabId)
                {
                    return prefab;
                }
            }
        }
        return null;
    }
}
