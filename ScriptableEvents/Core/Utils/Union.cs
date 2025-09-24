using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;


namespace Remedy.Schematics.Utils
{
    [Serializable]
    public partial struct Union
    {
        public enum ValueType : byte
        {
            Union,
            Bool, Int, Float,
            Vector2, Vector3, Vector4,
            Color, Color32,
            Quaternion,
            LayerMask,

            String, GameObject, Transform, Component, Material, Texture, AudioClip, Array, Object, Null, PhysicsMaterial,

            List, Scene,
        }

        public static readonly Dictionary<ValueType, Type> TypeLookup = new()
        {
            { ValueType.Union, typeof(Union) },

            { ValueType.Bool, typeof(bool) },
            { ValueType.Int, typeof(int) },
            { ValueType.Float, typeof(float) },

            { ValueType.Vector2, typeof(Vector2) },
            { ValueType.Vector3, typeof(Vector3) },
            { ValueType.Vector4, typeof(Vector4) },

            { ValueType.Color, typeof(Color) },
            { ValueType.Color32, typeof(Color32) },
            { ValueType.Quaternion, typeof(Quaternion) },
            { ValueType.LayerMask, typeof(LayerMask) },

            { ValueType.String, typeof(string) },
            { ValueType.GameObject, typeof(GameObject) },
            { ValueType.Transform, typeof(Transform) },
            { ValueType.Component, typeof(Component) },
            { ValueType.Material, typeof(Material) },
            { ValueType.Texture, typeof(Texture) },
            { ValueType.AudioClip, typeof(AudioClip) },

            { ValueType.Array, typeof(Array) },
            { ValueType.Object, typeof(UnityEngine.Object) },
            { ValueType.Null, null },
            { ValueType.PhysicsMaterial, typeof(PhysicsMaterial) },

            { ValueType.List, typeof(System.Collections.IList) }, // or typeof(List<>) if you’ll make it generic
            { ValueType.Scene, typeof(Scene) },
        };

        public ValueType Type;

        public Vector4 StackValue;    // 16 bytes - Value Types
        public object HeapValue;     // 4-8 bytes - Reference types 

        public int LastUpdateTick;

        private static int GlobalTick = 0;
        public static int NextTick() => Interlocked.Increment(ref GlobalTick);

        private static int _sceneIdCounter = 1;
        private static readonly Dictionary<int, Scene> _sceneLookup = new();

        // Constructors
        public Union(bool value) : this() { Type = ValueType.Bool; StackValue = new Vector4(value ? 1f : 0f, 0, 0, 0); }
        public Union(int value) : this() { Type = ValueType.Int; StackValue = new Vector4(value, 0, 0, 0); }
        public Union(float value) : this() { Type = ValueType.Float; StackValue = new Vector4(value, 0, 0, 0); }
        public Union(Vector2 value) : this() { Type = ValueType.Vector2; StackValue = new Vector4(value.x, value.y, 0, 0); }
        public Union(Vector3 value) : this() { Type = ValueType.Vector3; StackValue = new Vector4(value.x, value.y, value.z, 0); }
        public Union(Vector4 value) : this() { Type = ValueType.Vector4; StackValue = value; }
        public Union(Color value) : this() { Type = ValueType.Color; StackValue = new Vector4(value.r, value.g, value.b, value.a); }
        public Union(Color32 value) : this() { Type = ValueType.Color32; StackValue = new Vector4(value.r / 255f, value.g / 255f, value.b / 255f, value.a / 255f); }
        public Union(Quaternion value) : this() { Type = ValueType.Quaternion; StackValue = new Vector4(value.x, value.y, value.z, value.w); }
        public Union(LayerMask value) : this() { Type = ValueType.LayerMask; StackValue = new Vector4(value.value, 0, 0, 0); }
        public Union(string value) : this() { Type = ValueType.String; HeapValue = value; }
        public Union(GameObject value) : this() { Type = ValueType.GameObject; HeapValue = value; }
        public Union(Component value) : this() { Type = ValueType.Component; HeapValue = value; }
        public Union(PhysicsMaterial value) : this() { Type = ValueType.PhysicsMaterial; HeapValue = value; }
        public Union(List<Union> value) : this() { Type = ValueType.List; HeapValue = value; }
        public Union(Array value) : this() { Type = ValueType.List; HeapValue = value; }
        public Union(Union value) : this() 
        { 
            Type = ValueType.Union; 
            HeapValue = value;
            StackValue = value;
        }

        public Union(Scene scene) : this()
        {
            Type = ValueType.Scene;

            int id = _sceneIdCounter++;
            _sceneLookup[id] = scene;

            StackValue = new Vector4(id, 0, 0, 0);
        }

        public static implicit operator Union(bool value) => new(value);
        public static implicit operator Union(int value) => new(value);
        public static implicit operator Union(float value) => new(value);
        public static implicit operator Union(Vector2 value) => new(value);
        public static implicit operator Union(Vector3 value) => new(value);
        public static implicit operator Union(Vector4 value) => new(value);
        public static implicit operator Union(Color value) => new(value);
        public static implicit operator Union(Color32 value) => new(value);
        public static implicit operator Union(Quaternion value) => new(value);
        public static implicit operator Union(string value) => new(value);
        public static implicit operator Union(LayerMask value) => new(value);
        public static implicit operator Union(GameObject value) => new(value);
        public static implicit operator Union(Component value) => new(value);
        public static implicit operator Union(PhysicsMaterial value) => new(value);
        public static implicit operator Union(List<Union> value) => new(value);
        public static implicit operator Union(Array value) => new(value);
        public static implicit operator Union(Scene scene) => new(scene);

        public static implicit operator bool(Union value) => value.Get<bool>();
        public static implicit operator int(Union value) => value.Get<int>();
        public static implicit operator float(Union value) => value.Get<float>();
        public static implicit operator Vector2(Union value) => value.Get<Vector2>();
        public static implicit operator Vector3(Union value) => value.Get<Vector3>();
        public static implicit operator Vector4(Union value) => value.Get<Vector4>();
        public static implicit operator Color(Union value) => value.Get<Color>();
        public static implicit operator Color32(Union value) => value.Get<Color32>();
        public static implicit operator Quaternion(Union value) => value.Get<Quaternion>();
        public static implicit operator string(Union value) => value.Get<string>();
        public static implicit operator GameObject(Union value) => value.Get<GameObject>();
        public static implicit operator Component(Union value) => value.Get<Component>();
        public static implicit operator PhysicsMaterial(Union value) => value.Get<PhysicsMaterial>();
        public static implicit operator Array(Union value) => value.HeapValue as Array;
        public static implicit operator List<Union>(Union value) => value.HeapValue as List<Union>;
        public static implicit operator Scene(Union value) => value.GetScene();

        public override string ToString()
        {
            return Type switch
            {
                ValueType.Null => "null",
                ValueType.Bool => (StackValue.x != 0f).ToString(),
                ValueType.Int => ((int)StackValue.x).ToString(),
                ValueType.Float => StackValue.x.ToString(),
                ValueType.Vector2 => $"({StackValue.x}, {StackValue.y})",
                ValueType.Vector3 => $"({StackValue.x}, {StackValue.y}, {StackValue.z})",
                ValueType.Vector4 => $"({StackValue.x}, {StackValue.y}, {StackValue.z}, {StackValue.w})",
                ValueType.Color => $"RGBA({StackValue.x:F2}, {StackValue.y:F2}, {StackValue.z:F2}, {StackValue.w:F2})",
                ValueType.LayerMask => $"LayerMask({(int)StackValue.x})",
                _ => HeapValue?.ToString() ?? Type.ToString()
            };
        }

        public static ref TTo UnsafeCastAs<TFrom, TTo>(ref TFrom source)
        {
            return ref UnsafeUtility.As<TFrom, TTo>(ref source);
        }
        public Scene GetScene()
        {
            if (Type != ValueType.Scene) return default;

            int id = (int)StackValue.x;
            return _sceneLookup.TryGetValue(id, out var scene) ? scene : default;
        }
        public static Union operator +(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a + (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a + (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a + (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a + (int)b),
                (ValueType.Vector2, ValueType.Vector2) => new Union((Vector2)a + (Vector2)b),
                (ValueType.Vector3, ValueType.Vector3) => new Union((Vector3)a + (Vector3)b),
                (ValueType.Vector4, ValueType.Vector4) => new Union((Vector4)a + (Vector4)b),
                (ValueType.Color, ValueType.Color) => new Union((Color)a + (Color)b),
                (ValueType.Color32, ValueType.Color32) => new Union(AddColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Vector3) => new Union(AddColorVector3((Color)a, (Vector3)b)),
                (ValueType.Vector3, ValueType.Color) => new Union(AddColorVector3((Color)b, (Vector3)a)),
                (ValueType.Color, ValueType.Vector4) => new Union(AddColorVector4((Color)a, (Vector4)b)),
                (ValueType.Vector4, ValueType.Color) => new Union(AddColorVector4((Color)b, (Vector4)a)),
                (ValueType.String, _) => new Union((string)a + b.ToString()),
                (_, ValueType.String) => new Union(a.ToString() + (string)b),
                (ValueType.Array, ValueType.Array) => new Union(CombineArrays((Array)a, (Array)b)),
                (ValueType.List, ValueType.List) => new Union(CombineLists(a.Get<List<Union>>(), b.Get<List<Union>>())),
                (ValueType.Array, _) => new Union(AddToArray((Array)a, b.Get<object>())),
                (ValueType.List, _) => new Union(AddToList(a.Get<List<Union>>(), b)),
                _ => new(GetNull())
            };
        }

        private static string GetNull()
        {
            return null;
        }

        private static Color32 AddColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)Mathf.Min(255, a.r + b.r),
                (byte)Mathf.Min(255, a.g + b.g),
                (byte)Mathf.Min(255, a.b + b.b),
                (byte)Mathf.Min(255, a.a + b.a)
            );
        }

        private static Color AddColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r + vector.x),
                Mathf.Clamp01(color.g + vector.y),
                Mathf.Clamp01(color.b + vector.z),
                color.a
            );
        }

        private static Color AddColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r + vector.x),
                Mathf.Clamp01(color.g + vector.y),
                Mathf.Clamp01(color.b + vector.z),
                Mathf.Clamp01(color.a + vector.w)
            );
        }

        private static Array CombineArrays(Array a, Array b)
        {
            var elementType = a.GetType().GetElementType() ?? typeof(object);
            var result = Array.CreateInstance(elementType, a.Length + b.Length);
            Array.Copy(a, 0, result, 0, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }

        private static List<Union> CombineLists(List<Union> a, List<Union> b)
        {
            var result = new List<Union>();
            foreach (var item in a) result.Add(item);
            foreach (var item in b) result.Add(item);
            return result;
        }

        private static Array AddToArray(Array array, object item)
        {
            var elementType = array.GetType().GetElementType() ?? typeof(object);
            var result = Array.CreateInstance(elementType, array.Length + 1);
            Array.Copy(array, result, array.Length);
            result.SetValue(item, array.Length);
            return result;
        }

        private static List<Union> AddToList(List<Union> list, Union item)
        {
            var result = new List<Union>();
            foreach (var existingItem in list) result.Add(existingItem);
            result.Add(item);
            return result;
        }

        public static Union operator *(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a * (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a * (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a * (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a * (int)b),
                (ValueType.Vector3, ValueType.Vector3) => new Union(Vector3.Scale((Vector3)a, ((Vector3)b))),
                (ValueType.Vector2, ValueType.Vector2) => new Union(Vector2.Scale((Vector2)a, ((Vector2)b))),
                (ValueType.Vector4, ValueType.Vector4) => new Union(Vector4.Scale((Vector4)a, ((Vector4)b))),
                (ValueType.Vector2, ValueType.Float) => new Union((Vector2)a * ((float)b)),
                (ValueType.Float, ValueType.Vector2) => new Union((Vector2)b * ((float)a)),
                (ValueType.Vector4, ValueType.Float) => new Union((Vector4)a * ((float)b)),
                (ValueType.Float, ValueType.Vector4) => new Union((Vector4)b * ((float)a)),
                (ValueType.Vector3, ValueType.Float) => new Union((Vector3)a * (float)b),
                (ValueType.Float, ValueType.Vector3) => new Union((float)a * (Vector3)b),
                (ValueType.Color, ValueType.Color) => new Union(MultiplyColor((Color)a, (Color)b)),
                (ValueType.Color32, ValueType.Color32) => new Union(MultiplyColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Float) => new Union((Color)a * (float)b),
                (ValueType.Float, ValueType.Color) => new Union((Color)b * (float)a),
                (ValueType.Color, ValueType.Vector3) => new Union(MultiplyColorVector3((Color)a, (Vector3)b)),
                (ValueType.Vector3, ValueType.Color) => new Union(MultiplyColorVector3((Color)b, (Vector3)a)),
                (ValueType.Color, ValueType.Vector4) => new Union(MultiplyColorVector4((Color)a, (Vector4)b)),
                (ValueType.Vector4, ValueType.Color) => new Union(MultiplyColorVector4((Color)b, (Vector4)a)),
                (ValueType.String, ValueType.Int) => new Union(string.Concat(Enumerable.Repeat((string)a, (int)b))),
                _ => new(GetNull())
            };
        }

        private static Color MultiplyColor(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }

        private static Color32 MultiplyColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)((a.r * b.r) / 255),
                (byte)((a.g * b.g) / 255),
                (byte)((a.b * b.b) / 255),
                (byte)((a.a * b.a) / 255)
            );
        }

        private static Color MultiplyColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r * vector.x),
                Mathf.Clamp01(color.g * vector.y),
                Mathf.Clamp01(color.b * vector.z),
                color.a
            );
        }

        private static Color MultiplyColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r * vector.x),
                Mathf.Clamp01(color.g * vector.y),
                Mathf.Clamp01(color.b * vector.z),
                Mathf.Clamp01(color.a * vector.w)
            );
        }

        public static Union operator -(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a - (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a - (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a - (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a - (int)b),
                (ValueType.Vector2, ValueType.Vector2) => new Union((Vector2)a - (Vector2)b),
                (ValueType.Vector3, ValueType.Vector3) => new Union((Vector3)a - (Vector3)b),
                (ValueType.Vector4, ValueType.Vector4) => new Union((Vector4)a - (Vector4)b),
                (ValueType.Color, ValueType.Color) => new Union((Color)a - (Color)b),
                (ValueType.Color32, ValueType.Color32) => new Union(SubtractColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Vector3) => new Union(SubtractColorVector3((Color)a, (Vector3)b)),
                (ValueType.Vector3, ValueType.Color) => new Union(SubtractVector3Color((Vector3)a, (Color)b)),
                (ValueType.Color, ValueType.Vector4) => new Union(SubtractColorVector4((Color)a, (Vector4)b)),
                (ValueType.Vector4, ValueType.Color) => new Union(SubtractVector4Color((Vector4)a, (Color)b)),
                (ValueType.String, _) => new Union(((string)a).Replace(b.ToString(), "")),
                (_, ValueType.String) => new Union(a.ToString().Replace((string)b, "")),
                (ValueType.Array, _) => new Union(RemoveFromArray((Array)a, b.Get<object>())),
                (ValueType.List, _) => new Union(RemoveFromList(a.Get<List<Union>>(), b.Get<object>())),
                _ => new(GetNull())
            };
        }

        private static Color32 SubtractColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)Mathf.Max(0, a.r - b.r),
                (byte)Mathf.Max(0, a.g - b.g),
                (byte)Mathf.Max(0, a.b - b.b),
                (byte)Mathf.Max(0, a.a - b.a)
            );
        }

        private static Color SubtractColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r - vector.x),
                Mathf.Clamp01(color.g - vector.y),
                Mathf.Clamp01(color.b - vector.z),
                color.a
            );
        }

        private static Vector3 SubtractVector3Color(Vector3 vector, Color color)
        {
            return new Vector3(
                vector.x - color.r,
                vector.y - color.g,
                vector.z - color.b
            );
        }

        private static Color SubtractColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                Mathf.Clamp01(color.r - vector.x),
                Mathf.Clamp01(color.g - vector.y),
                Mathf.Clamp01(color.b - vector.z),
                Mathf.Clamp01(color.a - vector.w)
            );
        }

        private static Vector4 SubtractVector4Color(Vector4 vector, Color color)
        {
            return new Vector4(
                vector.x - color.r,
                vector.y - color.g,
                vector.z - color.b,
                vector.w - color.a
            );
        }

        private static Array RemoveFromArray(Array array, object itemToRemove)
        {
            var list = new List<object>();
            bool removed = false;

            for (int i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);
                if (!removed && Equals(item, itemToRemove))
                {
                    removed = true;
                    continue;
                }
                list.Add(item);
            }

            var elementType = array.GetType().GetElementType() ?? typeof(object);
            var result = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                result.SetValue(list[i], i);
            }
            return result;
        }

        private static List<Union> RemoveFromList(List<Union> list, object itemToRemove)
        {
            var result = new List<Union>();
            bool removed = false;

            foreach (var item in list)
            {
                if (!removed && Equals(item, itemToRemove))
                {
                    removed = true;
                    continue;
                }
                result.Add(item);
            }
            return result;
        }

        public static Union operator /(Union a, Union b)
        {
            return (a.Type, b.Type) switch
            {
                (ValueType.Int, ValueType.Int) => new Union((int)a / (int)b),
                (ValueType.Float, ValueType.Float) => new Union((float)a / (float)b),
                (ValueType.Int, ValueType.Float) => new Union((int)a / (float)b),
                (ValueType.Float, ValueType.Int) => new Union((float)a / (int)b),
                (ValueType.Vector2, ValueType.Vector2) => new Union(new Vector2(((Vector2)a).x / ((Vector2)b).x,
                                                                                    ((Vector2)a).y / ((Vector2)b).y)),
                (ValueType.Vector3, ValueType.Vector3) => new Union(new Vector3(((Vector3)a).x / ((Vector3)b).x,
                                                                                    ((Vector3)a).y / ((Vector3)b).y,
                                                                                    ((Vector3)a).z / ((Vector3)b).z)),
                (ValueType.Vector4, ValueType.Vector4) => new Union(new Vector4(((Vector4)a).x / ((Vector4)b).x,
                                                                                    ((Vector4)a).y / ((Vector4)b).y,
                                                                                    ((Vector4)a).z / ((Vector4)b).z,
                                                                                    ((Vector4)a).w / ((Vector4)b).w)),
                (ValueType.Vector2, ValueType.Float) => new Union((Vector2)a / (float)b),
                (ValueType.Float, ValueType.Vector2) => new Union(new Vector2((float)a / ((Vector2)b).x, (float)a / ((Vector2)b).y)),
                (ValueType.Vector3, ValueType.Float) => new Union((Vector3)a / (float)b),
                (ValueType.Float, ValueType.Vector3) => new Union(new Vector3((float)a / ((Vector3)b).x, (float)a / ((Vector3)b).y, (float)a / ((Vector3)b).z)),
                (ValueType.Vector4, ValueType.Float) => new Union((Vector4)a / (float)b),
                (ValueType.Float, ValueType.Vector4) => new Union(new Vector4((float)a / ((Vector4)b).x, (float)a / ((Vector4)b).y, (float)a / ((Vector4)b).z, (float)a / ((Vector4)b).w)),
                (ValueType.Color, ValueType.Color) => new Union(DivideColor((Color)a, (Color)b)),
                (ValueType.Color32, ValueType.Color32) => new Union(DivideColor32((Color32)a, (Color32)b)),
                (ValueType.Color, ValueType.Float) => new Union((Color)a / (float)b),
                (ValueType.Color, ValueType.Vector3) => new Union(DivideColorVector3((Color)a, (Vector3)b)),
                (ValueType.Color, ValueType.Vector4) => new Union(DivideColorVector4((Color)a, (Vector4)b)),
                _ => new(GetNull())
            };
        }

        private static Color DivideColor(Color a, Color b)
        {
            return new Color(
                b.r != 0 ? a.r / b.r : 0,
                b.g != 0 ? a.g / b.g : 0,
                b.b != 0 ? a.b / b.b : 0,
                b.a != 0 ? a.a / b.a : 0
            );
        }

        private static Color32 DivideColor32(Color32 a, Color32 b)
        {
            return new Color32(
                (byte)(b.r != 0 ? (a.r * 255) / b.r : 0),
                (byte)(b.g != 0 ? (a.g * 255) / b.g : 0),
                (byte)(b.b != 0 ? (a.b * 255) / b.b : 0),
                (byte)(b.a != 0 ? (a.a * 255) / b.a : 0)
            );
        }

        private static Color DivideColorVector3(Color color, Vector3 vector)
        {
            return new Color(
                vector.x != 0 ? Mathf.Clamp01(color.r / vector.x) : 0,
                vector.y != 0 ? Mathf.Clamp01(color.g / vector.y) : 0,
                vector.z != 0 ? Mathf.Clamp01(color.b / vector.z) : 0,
                color.a
            );
        }

        private static Color DivideColorVector4(Color color, Vector4 vector)
        {
            return new Color(
                vector.x != 0 ? Mathf.Clamp01(color.r / vector.x) : 0,
                vector.y != 0 ? Mathf.Clamp01(color.g / vector.y) : 0,
                vector.z != 0 ? Mathf.Clamp01(color.b / vector.z) : 0,
                vector.w != 0 ? Mathf.Clamp01(color.a / vector.w) : 0
            );
        }
    }

    public partial struct Union
    {
        public T Get<T>()
        {
            return TypeCache<T>.Getter(this);
        }

        public void Set<T>(T value)
        {
            TypeCache<T>.Setter(ref this, ref value);
            LastUpdateTick = NextTick();
        }

        // Cache specialized per T
        private static class TypeCache<T>
        {
            public static readonly Func<Union, T> Getter;

            public static readonly SetterDelegate Setter;
            public delegate void SetterDelegate(ref Union u, ref T value);

            static TypeCache()
            {
                Getter = _ => default;
                Setter = (ref Union u, ref T v) =>
                {
                    u.Type = ValueType.Null;
                    u.StackValue = default;
                };

                // Getters
                if (typeof(T) == typeof(bool))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Bool => u.StackValue.x != 0f,
                            ValueType.Int => u.StackValue.x != 0f,
                            ValueType.Float => u.StackValue.x != 0f,
                            _ => false
                        };
                        return UnsafeCastAs<bool, T>(ref result);
                    };

                else if (typeof(T) == typeof(int))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Bool => u.StackValue.x != 0f ? 1 : 0,
                            ValueType.Int => (int)u.StackValue.x,
                            ValueType.Float => (int)u.StackValue.x,
                            ValueType.LayerMask => (int)u.StackValue.x,
                            _ => 0
                        };
                        return UnsafeCastAs<int, T>(ref result);
                    };

                else if (typeof(T) == typeof(float))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Bool => u.StackValue.x != 0f ? 1f : 0f,
                            ValueType.Int => u.StackValue.x,
                            ValueType.Float => u.StackValue.x,
                            ValueType.LayerMask => u.StackValue.x,
                            _ => 0f
                        };
                        return UnsafeCastAs<float, T>(ref result);
                    };

                else if (typeof(T) == typeof(Vector2))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Vector2 => new Vector2(u.StackValue.x, u.StackValue.y),
                            ValueType.Vector3 => new Vector2(u.StackValue.x, u.StackValue.y),
                            ValueType.Vector4 => new Vector2(u.StackValue.x, u.StackValue.y),
                            _ => Vector2.zero
                        };
                        return UnsafeCastAs<Vector2, T>(ref result);
                    };

                else if (typeof(T) == typeof(Vector3))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Vector2 => new Vector3(u.StackValue.x, u.StackValue.y, 0),
                            ValueType.Vector3 => new Vector3(u.StackValue.x, u.StackValue.y, u.StackValue.z),
                            ValueType.Vector4 => new Vector3(u.StackValue.x, u.StackValue.y, u.StackValue.z),
                            _ => Vector3.zero
                        };
                        return UnsafeCastAs<Vector3, T>(ref result);
                    };

                else if (typeof(T) == typeof(Vector4))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Vector2 => new Vector4(u.StackValue.x, u.StackValue.y, 0, 0),
                            ValueType.Vector3 => new Vector4(u.StackValue.x, u.StackValue.y, u.StackValue.z, 0),
                            ValueType.Vector4 => u.StackValue,
                            ValueType.Color => u.StackValue,
                            ValueType.Color32 => u.StackValue,
                            ValueType.Quaternion => u.StackValue,
                            _ => Vector4.zero
                        };
                        return UnsafeCastAs<Vector4, T>(ref result);
                    };

                else if (typeof(T) == typeof(Color))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Color => new Color(u.StackValue.x, u.StackValue.y, u.StackValue.z, u.StackValue.w),
                            ValueType.Color32 => new Color(u.StackValue.x, u.StackValue.y, u.StackValue.z, u.StackValue.w),
                            ValueType.Vector4 => new Color(u.StackValue.x, u.StackValue.y, u.StackValue.z, u.StackValue.w),
                            _ => Color.white
                        };
                        return UnsafeCastAs<Color, T>(ref result);
                    };


                else if (typeof(T) == typeof(Color32))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Color => new Color32(
                                (byte)(u.StackValue.x * 255),
                                (byte)(u.StackValue.y * 255),
                                (byte)(u.StackValue.z * 255),
                                (byte)(u.StackValue.w * 255)),
                            ValueType.Color32 => new Color32(
                                (byte)(u.StackValue.x * 255),
                                (byte)(u.StackValue.y * 255),
                                (byte)(u.StackValue.z * 255),
                                (byte)(u.StackValue.w * 255)),
                            _ => new Color32(255, 255, 255, 255)
                        };
                        return UnsafeCastAs<Color32, T>(ref result);
                    };

                else if (typeof(T) == typeof(Quaternion))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.Quaternion => new Quaternion(u.StackValue.x, u.StackValue.y, u.StackValue.z, u.StackValue.w),
                            ValueType.Vector4 => new Quaternion(u.StackValue.x, u.StackValue.y, u.StackValue.z, u.StackValue.w),
                            _ => Quaternion.identity
                        };
                        return UnsafeCastAs<Quaternion, T>(ref result);
                    };

                else if (typeof(T) == typeof(LayerMask))
                    Getter = u =>
                    {
                        var result = u.Type switch
                        {
                            ValueType.LayerMask => (LayerMask)(int)u.StackValue.x,
                            ValueType.Int => (LayerMask)(int)u.StackValue.x,
                            ValueType.Float => (LayerMask)(int)u.StackValue.x,
                            _ => (LayerMask)0
                        };
                        return UnsafeCastAs<LayerMask, T>(ref result);
                    };


                // Setters
                if (typeof(T) == typeof(bool))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref bool val = ref UnsafeCastAs<T, bool>(ref v);
                        u.Type = ValueType.Bool;
                        u.StackValue.x = val ? 1f : 0f;
                        u.StackValue.y = 0;
                        u.StackValue.z = 0;
                        u.StackValue.w = 0;
                    };
                }
                else if (typeof(T) == typeof(int))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref int val = ref UnsafeCastAs<T, int>(ref v);
                        u.Type = ValueType.Int;
                        u.StackValue.x = val;
                        u.StackValue.y = 0;
                        u.StackValue.z = 0;
                        u.StackValue.w = 0;
                    };
                }
                else if (typeof(T) == typeof(float))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref float val = ref UnsafeCastAs<T, float>(ref v);
                        u.Type = ValueType.Float;
                        u.StackValue.x = val;
                        u.StackValue.y = 0;
                        u.StackValue.z = 0;
                        u.StackValue.w = 0;
                    };
                }
                else if (typeof(T) == typeof(Vector2))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref Vector2 val = ref UnsafeCastAs<T, Vector2>(ref v);
                        u.Type = ValueType.Vector2;
                        u.StackValue.x = val.x;
                        u.StackValue.y = val.y;
                        u.StackValue.z = 0;
                        u.StackValue.w = 0;
                    };
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref Vector3 val = ref UnsafeCastAs<T, Vector3>(ref v);
                        u.Type = ValueType.Vector3;
                        u.StackValue.x = val.x;
                        u.StackValue.y = val.y;
                        u.StackValue.z = val.z;
                        u.StackValue.w = 0;
                    };
                }
                else if (typeof(T) == typeof(Vector4))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref Vector4 val = ref UnsafeCastAs<T, Vector4>(ref v);
                        u.Type = ValueType.Vector4;
                        u.StackValue = val;
                    };
                }
                else if (typeof(T) == typeof(Color))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref Color val = ref UnsafeCastAs<T, Color>(ref v);
                        u.Type = ValueType.Color;
                        u.StackValue.x = val.r;
                        u.StackValue.y = val.g;
                        u.StackValue.z = val.b;
                        u.StackValue.w = val.a;
                    };
                }
                else if (typeof(T) == typeof(Color32))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref Color32 val = ref UnsafeCastAs<T, Color32>(ref v);
                        u.Type = ValueType.Color32;
                        u.StackValue.x = val.r / 255f;
                        u.StackValue.y = val.g / 255f;
                        u.StackValue.z = val.b / 255f;
                        u.StackValue.w = val.a / 255f;
                    };
                }
                else if (typeof(T) == typeof(Quaternion))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref Quaternion val = ref UnsafeCastAs<T, Quaternion>(ref v);
                        u.Type = ValueType.Quaternion;
                        u.StackValue.x = val.x;
                        u.StackValue.y = val.y;
                        u.StackValue.z = val.z;
                        u.StackValue.w = val.w;
                    };
                }
                else if (typeof(T) == typeof(LayerMask))
                {
                    Setter = (ref Union u, ref T v) =>
                    {
                        ref LayerMask val = ref UnsafeCastAs<T, LayerMask>(ref v);
                        u.Type = ValueType.LayerMask;
                        u.StackValue.x = val.value;
                        u.StackValue.y = 0;
                        u.StackValue.z = 0;
                        u.StackValue.w = 0;
                    };
                }
                else
                {
                    Setter = (ref Union u, ref T val) =>
                    {
                        switch (val)
                        {
                            case null: u.Type = ValueType.Null; u.HeapValue = null; break;
                            case string v: u.Type = ValueType.String; u.HeapValue = v; break;
                            case GameObject v: u.Type = ValueType.GameObject; u.HeapValue = v; break;
                            case Transform v: u.Type = ValueType.Transform; u.HeapValue = v; break;
                            case Component v: u.Type = ValueType.Component; u.HeapValue = v; break;
                            case Material v: u.Type = ValueType.Material; u.HeapValue = v; break;
                            case Texture2D v: u.Type = ValueType.Texture; u.HeapValue = v; break;
                            case AudioClip v: u.Type = ValueType.AudioClip; u.HeapValue = v; break;
                            case PhysicsMaterial v: u.Type = ValueType.PhysicsMaterial; u.HeapValue = v; break;
                            case Array v: u.Type = ValueType.Array; u.HeapValue = v; break;
                            default: u.Type = ValueType.Object; u.HeapValue = val; break;
                        }
                    };
                }
            }
        }
    }

}