using Remedy.Framework;
//using SaintsField;
using UnityEngine;
using Remedy.UI;
using Remedy.Inventories;

namespace Remedy.UI
{
    [ExecuteAlways]
    public class InventorySelectionWheel : UIComponent
    {
        //[InfoBox("Ensure to set up the Material Communicator and wire it into this component using Scriptable Events!")]
        public ScriptableEventBoolean.Input OpenWheelInput;
        public ScriptableEventVector2.Input SelectionInput;
        [Tooltip("Opens the weapon wheel, then selects the item at the given index. This is ignored if the Inventory doesn't have said item.")]
        public ScriptableEventInt.Input OpenAndSelectInput;

        public ScriptableEventFloat.Output OnContentUpdated;
        public ScriptableEventFloat.Output OnSelectionChanged;
        public ScriptableEventBoolean.Output OnStateChanged;

        [Tooltip("The Inventory we're displaying/choosing Items from.")]
        public Inventory Inventory;

        public float ItemDistance = 1f;
        public float SelectionSpeed = 15f;
        public float OpenCloseSpeed = 15f;
        public float CloseDelay = 1f;

        public SerializableDictionary<int, UISprite> ItemSlotSprites = new();
        public UISprite WeaponWheelSprite;

        private float _previousSelection = 0;
        private float _currentSelection = 0;
        [SerializeField, HideInInspector]
        private bool _isInitialized = false;
        [SerializeField, HideInInspector]
        private bool _hasSetupMaterial = false;
        private bool _setupInventory = false;
        private bool _open = false;
        private bool _wasOpen = false;
        private Vector3 _initialScale;
        private Vector2 _currentInput = Vector2.zero;
        private bool _openAndSelecting = false;
        private bool _hasSelected = false;
        private float _closeWait = 0f;
        private int _openAndSelectingIndex = 0;

        protected override void InitiateAndSubscribeEvents()
        {
            if (Application.isPlaying)
            {
                OpenWheelInput?.Subscribe(this, (bool val) =>
                {
                    _open = val;

                    if (val)
                    {
                        _openAndSelecting = false;
                        _closeWait = 0f;
                    }
                });

                SelectionInput?.Subscribe(this, (Vector2 val) =>
                {
                    if (_open && !_openAndSelecting)
                    {
                        int count = Inventory.Contents.Count;
                        _currentInput += val;
                        if (count == 0 || Mathf.Abs(_currentInput.x) < 1 && Mathf.Abs(_currentInput.y) < 1)
                            return;

                        float angle = Mathf.Atan2(_currentInput.x, _currentInput.y);
                        Debug.Log(angle);

                        // Divide full circle into sectors
                        float sectorSize = Mathf.PI * 2f / count;
                        int index = 0;
                        if (angle > 0)
                            index = Mathf.RoundToInt(angle / sectorSize) % count;
                        else
                            index = count - (Mathf.RoundToInt(Mathf.Abs(angle) / sectorSize));

                        _currentSelection = index;
                    }
                });

                OpenAndSelectInput?.Subscribe(this, (int value) =>
                {
                    _openAndSelecting = true;
                    _hasSelected = false;
                    _openAndSelectingIndex = value;
                });

                Inventory.SelectItem(0);
                _initialScale = transform.localScale;
            }
        }

        private void OnValidate()
        {
            if (WeaponWheelSprite == null)
                WeaponWheelSprite = UISprite.New("Weapon Wheel", transform);
        }

        protected override void UnSubscribeEvents()
        {
            SelectionInput?.Unsubscribe(this);
        }

        private void Update()
        {
            _currentInput = Vector2.Lerp(_currentInput, Vector2.zero, 0.1f);

            if (Inventory == null) return;

            if (Application.isPlaying)
            {
                if (_openAndSelecting)
                {
                    if (!_hasSelected)
                    {
                        _open = true;

                        if (Vector3.Distance(transform.localScale, _initialScale) < 0.1f)
                        {
                            _hasSelected = true;
                            _currentSelection = _openAndSelectingIndex;
                        }
                    }
                    else
                    {
                        _open = false;
                    }
                }

                if (_open)
                {
                    if (!_wasOpen)
                    {
                        _setupInventory = true;
                        OnStateChanged?.Invoke(true);
                    }

                    transform.localScale = Vector3.Lerp(transform.localScale, _initialScale, OpenCloseSpeed * Time.deltaTime);
                    _wasOpen = true;
                    _closeWait = 0f;
                }
                else
                {
                    if (_closeWait > CloseDelay)
                    {
                        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, OpenCloseSpeed * Time.deltaTime);
                        Select((int)_currentSelection);
                    }
                    _closeWait += Time.deltaTime;
                    _wasOpen = false;
                }
            }

            if (!_setupInventory)
            {
                for (int i = 0; i < Inventory.Contents.Count; i++)
                {
                    if (ItemSlotSprites.TryGetValue(i, out UISprite sprite))
                    {
                        sprite.SetSprite(Inventory.Contents[i].Sprite);
                    }
                    else
                    {
                        ItemSlotSprites[i] = UISprite.New("Item_" + Inventory.Contents[i].name, transform, Layer + 1);
                        ItemSlotSprites[i].SetSprite(Inventory.Contents[i].Sprite);
                    }
                }

                for (int i = ItemSlotSprites.Keys.Count; i > 0; i--)
                {
                    if (ItemSlotSprites[i] == null)
                        ItemSlotSprites.Remove(i);
                }

                OnContentUpdated?.Invoke(1f / Inventory.Contents.Count);
                _setupInventory = true;
            }

            float step = 360f / Inventory.Contents.Count;

            for (int i = 0; i < ItemSlotSprites.Keys.Count; i++)
            {
                float halfStep = (step / 2) * Mathf.Deg2Rad;
                float radStep = step * Mathf.Deg2Rad;

                var sprite = ItemSlotSprites[i];
                Vector3 localPosition = new Vector3(Mathf.Sin((i * radStep) - halfStep), -Mathf.Cos((i * radStep) - halfStep), 0) * ItemDistance;
                sprite.transform.localPosition = localPosition;
            }

            // Smoothly Select
            if (_previousSelection != _currentSelection)
            {
                float currentSelectionAngle = _previousSelection * step;
                float goalSelectionAngle = _currentSelection * step;

                _previousSelection = Mathf.LerpAngle(currentSelectionAngle, goalSelectionAngle, SelectionSpeed * Time.deltaTime) / step;
                OnSelectionChanged?.Invoke(_previousSelection);
            }
        }

        public void Select(int item)
        {
            Inventory.SelectItem(item);
            OnStateChanged?.Invoke(false);
        }
    }
}