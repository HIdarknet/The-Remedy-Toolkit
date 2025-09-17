using UnityEngine;

namespace Remedy.Framework
{
    public class ScriptableGridNode : ScriptableObject
    {
        private ScriptableGrid _grid;
        public ScriptableGrid Grid => _grid;

        private object _value;

        public object Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value != null) _empty = false;
                _value = value;
            }
        }

        private int _x;
        public int X => _x;

        private int _y;
        public int Y => _y;

        private bool _empty = true;

        public bool Empty => _empty;

        private bool _cachedPosition = false;

        private Vector2 _position;
        public Vector2 Position
        {
            get
            {
                if (!_cachedPosition)
                {
                    _position = new Vector2(X, Y);
                    _cachedPosition = true;
                }
                return _position;
            }
        }

        public void SetupNode(ScriptableGrid grid, int x, int y)
        {
            _grid = grid;
            _x = x;
            _y = y;
        }

        protected bool CheckIfBlocked()
        {
            return false;
        }
    }

    public class ScriptableGridNode<T> : ScriptableGridNode
    {
    }
}