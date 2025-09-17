using System;
using UnityEngine;

namespace Remedy.Schematics
{
    public abstract class ScriptGraphParameter : ScriptableObject
    {
        public abstract Type Type { get; }
        public abstract object Value { get; set; }
    }

    public abstract class ScriptGraphParameter<T> : ScriptGraphParameter
    {
        [SerializeField]
        private T value;

        public override Type Type => typeof(T);

        public override object Value
        {
            get => value;
            set => this.value = (T)value;
        }

        public T TypedValue
        {
            get => value;
            set => this.value = value;
        }
    }

    public class IntParameter : ScriptGraphParameter<int>
    { }

    public class StringParameter : ScriptGraphParameter<string>
    { }

    public class BoolParameter : ScriptGraphParameter<bool>
    { }

    public class FloatParameter : ScriptGraphParameter<float>
    { }
}
