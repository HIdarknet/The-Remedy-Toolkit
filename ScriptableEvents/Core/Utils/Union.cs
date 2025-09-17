using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Remedy.Schematics.Utils
{
    [Serializable]
    public partial struct Union
    {
        public enum ValueType : byte
        {
            Bool, Int, Float,
            Vector2, Vector3, Vector4,
            Color, Color32,
            Quaternion,
            LayerMask,

            String, GameObject, Transform, Component, Material, Texture, AudioClip, Array, Object, Null, PhysicsMaterial,

            List, Scene,
        }

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
        public static implicit operator Union(Quaternion value) => new(value);
        public static implicit operator Union(string value) => new(value);
        public static implicit operator Union(LayerMask value) => new(value);
        public static implicit operator Union(GameObject value) => value;
        public static implicit operator Union(Component value) => value;
        public static implicit operator Union(PhysicsMaterial value) => value;
        public static implicit operator Union(List<Union> value) => new(value);
        public static implicit operator Union(Scene scene) => new(scene);

        public static implicit operator bool(Union value) => value.Get<bool>();
        public static implicit operator int(Union value) => value.Get<int>();
        public static implicit operator float(Union value) => value.Get<float>();
        public static implicit operator Vector2(Union value) => value.Get<Vector2>();
        public static implicit operator Vector3(Union value) => value.Get<Vector3>();
        public static implicit operator Vector4(Union value) => value.Get<Vector4>();
        public static implicit operator Color(Union value) => value.Get<Color>();
        public static implicit operator Quaternion(Union value) => value.Get<Quaternion>();
        public static implicit operator string(Union value) => value.Get<string>();
        public static implicit operator GameObject(Union value) => value.Get<GameObject>();
        public static implicit operator Component(Union value) => value.Get<Component>();
        public static implicit operator PhysicsMaterial(Union value) => value.Get<PhysicsMaterial>();
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