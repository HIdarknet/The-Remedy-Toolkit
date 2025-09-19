using CodiceApp.EventTracking.Plastic;
using Remedy.Framework;
using Remedy.Schematics.Utils;
//using SaintsField;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[SchematicGlobalObject("Input")]
public class RemedyInput : SingletonData<RemedyInput>
{
    public bool ShowCursor = false;
    public enum ControlSchemeType
    {
        Keyboard,
        Gamepad
    }
    private ControlSchemeType _currentControlScheme = ControlSchemeType.Keyboard;
    public static ControlSchemeType ControlScheme => Instance._currentControlScheme;

    private UnityEvent<InputAction.CallbackContext> _onInputDown = new();
    /// <summary>
    /// An Event that is called when an Input is pressed. The CallbackContext is passed with the Event.
    /// </summary>
    public static UnityEvent<InputAction.CallbackContext> OnInputDown => Instance._onInputDown;
    private UnityEvent<InputAction.CallbackContext> _onInputHeld = new();
    /// <summary>
    /// An Event that is called when an Input is held. The CallbackContext is passed with the Event.
    /// </summary>
    public static UnityEvent<InputAction.CallbackContext> OnInputHeld => Instance._onInputHeld;
    private UnityEvent<InputAction.CallbackContext> _onInputUp = new();
    /// <summary>
    /// An Event that is called when an Input is released. The CallbackContext is passed with the Event.
    /// </summary>
    public static UnityEvent<InputAction.CallbackContext> OnInputUp => Instance._onInputUp;

    private static SerializableDictionary<ScriptableEventBase, Union> _currentInput = new();

    [IdentityListRenderer(identifierType: EventListIdentifierType.Name, identifierField: "Name", depth: 0, foldoutTitle: "Maps", itemName: "Map")]
    public InputActionMap[] InputMaps = new InputActionMap[0];
    [SerializeField]
    //[Dropdown("GetInputMaps")]
    private InputActionMap _currentActionMap;
    private Dictionary<InputAction, List<InputActionEvent>> _actionsToUpdate = new();


    public void InitializeInput()
    {
        if (InputMaps.Length > 0)
        {
            SetInputMap(InputMaps[0].Name);
        }
    }

    public void SetInputMap(string mapName)
    {
        foreach(var map in InputMaps)
            if (map.Name == mapName)
                _currentActionMap = map;
            else
            {
                Debug.LogError("The Requested Input Map does not exist: " + mapName);
                return;
            }

        foreach(var inputEventCollection in _currentActionMap.Inputs)
        {
            inputEventCollection.Input.Enable();

            inputEventCollection.Input.started += (InputAction.CallbackContext context) =>
            {
                foreach(var ev in inputEventCollection.Output.Events)
                {
                    switch (ev)
                    {
                        case ScriptableEventBoolean asBool:
                            _currentInput[ev] = true;
                            asBool?.Invoke(_currentInput[ev]);
                            break;
                        case ScriptableEventFloat asFloat:
                            _currentInput[ev] = context.ReadValue<float>();
                            asFloat?.Invoke(_currentInput[ev]);
                            break;
                        case ScriptableEventVector2 asVec2:
                            _currentInput[ev] = context.ReadValue<Vector2>();
                            asVec2?.Invoke(_currentInput[ev]);
                            break;
                        default:
                            //ev?.Invoke((Union)context.ReadValueAsObject());
                            break;
                    }
                }

                if (!inputEventCollection.OneShot)
                {
                    if (!_actionsToUpdate.ContainsKey(inputEventCollection.Input))
                        _actionsToUpdate.Add(inputEventCollection.Input, new());
                    _actionsToUpdate[inputEventCollection.Input].Add(inputEventCollection);
                }
            };

            inputEventCollection.Input.performed += (InputAction.CallbackContext context) =>
            {
                foreach (var ev in inputEventCollection.Output.Events)
                {
                    switch (ev)
                    {
                        case ScriptableEventBoolean asBool:
                            _currentInput[ev] = true;
                            asBool?.Invoke(_currentInput[ev]);
                            break;
                        case ScriptableEventFloat asFloat:
                            _currentInput[ev] = context.ReadValue<float>();
                            asFloat?.Invoke(_currentInput[ev]);
                            break;
                        case ScriptableEventVector2 asVec2:
                            _currentInput[ev] = context.ReadValue<Vector2>();
                            asVec2?.Invoke(_currentInput[ev]);
                            break;
                        default:
                            //ev?.Invoke((Union)context.ReadValueAsObject());
                            break;
                    }
                }
            };
            inputEventCollection.Input.canceled += (InputAction.CallbackContext context) =>
            {
                _actionsToUpdate.Remove(inputEventCollection.Input);
            };

            foreach (var ev in inputEventCollection.Output.Events)
            {
                inputEventCollection.Input.canceled += (InputAction.CallbackContext context) =>
                {
                    switch (ev)
                    {
                        case ScriptableEventBoolean asBool:
                            _currentInput[ev] = false;
                            asBool?.Invoke(default);
                            break;
                        case ScriptableEventFloat asFloat:
                            _currentInput[ev] = default;
                            asFloat?.Invoke(default);
                            break;
                        case ScriptableEventVector2 asVec2:
                            _currentInput[ev] = default;
                            asVec2?.Invoke(default);
                            break;
                        default:
                            _currentInput[ev] = default;
                            ev?.Invoke(default);
                            break;
                    }
                };
            }
        }
    }

    public void UpdateInputs()
    {
        if (_currentActionMap == null) return;

        foreach(var inputEvents in _actionsToUpdate.Values)
        {
            foreach (var inputEvent in inputEvents)
            {
                foreach(var ev in inputEvent.Output.Events)
                {
                    ev?.Invoke(_currentInput[ev]);
                }
            }
        }

        if (ShowCursor)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    [Serializable]
    public class InputActionMap 
    {
        public string Name;
        [IdentityListRenderer(identifierType: EventListIdentifierType.Name, identifierField: "Name", depth: 1, foldoutTitle: "Inputs", itemName: "Input")]
        public InputActionEvent[] Inputs = new InputActionEvent[0];

        public InputActionMap() { }

        public InputActionMap(string name)
        {
            Name = name;
        }

        public InputActionEvent this[int i]
        {
            get { return Inputs[i]; }
        }

        public void AddInput(string inputName)
        {
            Inputs = Inputs.Append(new(inputName)).ToArray();
        }

        public void RemoveInput(InputActionEvent inputEvent)
        {
            Inputs = Inputs.Where(item => item != inputEvent).ToArray();
        }
    }

    [Serializable]
    public class InputActionEvent
    {
        public string Name;
        public string EventName => $"Input {Name}";  
        [IMGUIContainerRenderer]
        [Tooltip("The Input Action, with it's binds.")]
        public InputAction Input= new();

        [EventContainerRenderer(typeof(ScriptableEventBase), "EventName")]
        public ScriptableEventBase.Output Output = new ScriptableEventBase.Output();

        [Tooltip("If true, the Output Event is not called again on update, although it will be called if the value for the Input has been updated (InputActionPhase.Performed).")]
        public bool OneShot = false;

        public InputActionEvent() 
        {
            //if (Input == null) Input = new();
        }
        public InputActionEvent(string name)
        {
            Name = name;
        }
    }
}