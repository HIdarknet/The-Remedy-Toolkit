using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Remedy.CacheBoxing
{
    public class BoxedValueTypeBase
    { }

    public class BoxedValueType<T> : BoxedValueTypeBase
    {
        protected T _value = default;
        public virtual T Value { get; set; }
    }
    
    public class BoxedObject : BoxedValueType<object>
    {
        public override object Value { get => _value; set => _value = value; }
    }

    // Int
    public class BoxedInt : BoxedValueType<int>
    {
        public override int Value { get => _value; set => _value = value; }

        public static implicit operator int(BoxedInt boxed) => boxed.Value;
        public static implicit operator BoxedInt(int value) => new BoxedInt { Value = value };
    }

    // Float
    public class BoxedFloat : BoxedValueType<float>
    {
        public override float Value { get => _value; set => _value = value; }

        public static explicit operator float(BoxedFloat boxed) => boxed.Value;
        public static explicit operator BoxedFloat(float value) => new BoxedFloat { Value = value };
    }

    // Boolean
    [Serializable]
    public class BoxedBool : BoxedValueType<Boolean>
    {
        public override Boolean Value { get => _value; set => _value = value; }

        public static implicit operator Boolean(BoxedBool boxed) => boxed.Value;
        public static implicit operator BoxedBool(Boolean value) => new BoxedBool { Value = value };
    }

    // Byte
    public class BoxedByte : BoxedValueType<byte>
    {
        public override byte Value { get => _value; set => _value = value; }

        public static explicit operator byte(BoxedByte boxed) => boxed.Value;
        public static explicit operator BoxedByte(byte value) => new BoxedByte { Value = value };
    }

    // Char
    public class BoxedChar : BoxedValueType<char>
    {
        public override char Value { get => _value; set => _value = value; }

        public static explicit operator char(BoxedChar boxed) => boxed.Value;
        public static explicit operator BoxedChar(char value) => new BoxedChar { Value = value };
    }

    // Decimal
    public class BoxedDecimal : BoxedValueType<decimal>
    {
        public override decimal Value { get => _value; set => _value = value; }

        public static explicit operator decimal(BoxedDecimal boxed) => boxed.Value;
        public static explicit operator BoxedDecimal(decimal value) => new BoxedDecimal { Value = value };
    }

    // Double
    public class BoxedDouble : BoxedValueType<double>
    {
        public override double Value { get => _value; set => _value = value; }

        public static explicit operator double(BoxedDouble boxed) => boxed.Value;
        public static explicit operator BoxedDouble(double value) => new BoxedDouble { Value = value };
    }

    // Int16 (short)
    public class BoxedInt16 : BoxedValueType<short>
    {
        public override short Value { get => _value; set => _value = value; }

        public static explicit operator short(BoxedInt16 boxed) => boxed.Value;
        public static explicit operator BoxedInt16(short value) => new BoxedInt16 { Value = value };
    }

    // Int32 (int)
    public class BoxedInt32 : BoxedValueType<int>
    {
        public override int Value { get => _value; set => _value = value; }

        public static explicit operator int(BoxedInt32 boxed) => boxed.Value;
        public static explicit operator BoxedInt32(int value) => new BoxedInt32 { Value = value };
    }

    // Int64 (long)
    public class BoxedInt64 : BoxedValueType<long>
    {
        public override long Value { get => _value; set => _value = value; }

        public static explicit operator long(BoxedInt64 boxed) => boxed.Value;
        public static explicit operator BoxedInt64(long value) => new BoxedInt64 { Value = value };
    }

    // SByte
    public class BoxedSByte : BoxedValueType<sbyte>
    {
        public override sbyte Value { get => _value; set => _value = value; }

        public static explicit operator sbyte(BoxedSByte boxed) => boxed.Value;
        public static explicit operator BoxedSByte(sbyte value) => new BoxedSByte { Value = value };
    }

    // UInt16
    public class BoxedUInt16 : BoxedValueType<ushort>
    {
        public override ushort Value { get => _value; set => _value = value; }

        public static explicit operator ushort(BoxedUInt16 boxed) => boxed.Value;
        public static explicit operator BoxedUInt16(ushort value) => new BoxedUInt16 { Value = value };
    }

    // UInt32
    public class BoxedUInt32 : BoxedValueType<uint>
    {
        public override uint Value { get => _value; set => _value = value; }

        public static explicit operator uint(BoxedUInt32 boxed) => boxed.Value;
        public static explicit operator BoxedUInt32(uint value) => new BoxedUInt32 { Value = value };
    }

    // UInt64
    public class BoxedUInt64 : BoxedValueType<ulong>
    {
        public override ulong Value { get => _value; set => _value = value; }

        public static explicit operator ulong(BoxedUInt64 boxed) => boxed.Value;
        public static explicit operator BoxedUInt64(ulong value) => new BoxedUInt64 { Value = value };
    }

    // Vector2
    [Serializable]
    public class BoxedVector2 : BoxedValueType<Vector2>
    {
        public override Vector2 Value { get => _value; set => _value = value; }

        public static implicit operator Vector2(BoxedVector2 boxed) => boxed.Value;
        public static explicit operator BoxedVector2(Vector2 value) => new BoxedVector2 { Value = value };


    }

    // Vector3
    public class BoxedVector3 : BoxedValueType<Vector3>
    {
        public override Vector3 Value { get => _value; set => _value = value; }

        public static implicit operator BoxedVector3(Vector3 value) => new BoxedVector3 { Value = value };
        public static explicit operator Vector3(BoxedVector3 boxed) => boxed.Value;
    }

    // Vector4
    public class BoxedVector4 : BoxedValueType<Vector4>
    {
        public override Vector4 Value { get => _value; set => _value = value; }

        public static implicit operator BoxedVector4(Vector4 value) => new BoxedVector4 { Value = value };
        public static explicit operator Vector4(BoxedVector4 boxed) => boxed.Value;
    }

    // Quaternion
    public class BoxedQuaternion : BoxedValueType<Quaternion>
    {
        public override Quaternion Value { get => _value; set => _value = value; }

        public static implicit operator BoxedQuaternion(Quaternion value) => new BoxedQuaternion { Value = value };
        public static explicit operator Quaternion(BoxedQuaternion boxed) => boxed.Value;
    }

    // Color
    public class BoxedColor : BoxedValueType<Color>
    {
        public override Color Value { get => _value; set => _value = value; }

        public static implicit operator BoxedColor(Color value) => new BoxedColor { Value = value };
        public static explicit operator Color(BoxedColor boxed) => boxed.Value;
    }

    // Rect
    public class BoxedRect : BoxedValueType<Rect>
    {
        public override Rect Value { get => _value; set => _value = value; }

        public static implicit operator BoxedRect(Rect value) => new BoxedRect { Value = value };
        public static explicit operator Rect(BoxedRect boxed) => boxed.Value;
    }

    // Bounds
    public class BoxedBounds : BoxedValueType<Bounds>
    {
        public override Bounds Value { get => _value; set => _value = value; }

        public static implicit operator BoxedBounds(Bounds value) => new BoxedBounds { Value = value };
        public static explicit operator Bounds(BoxedBounds boxed) => boxed.Value;
    }

    // RaycastHit
    public class BoxedRaycastHit : BoxedValueType<RaycastHit>
    {
        public override RaycastHit Value { get => _value; set => _value = value; }

        public static implicit operator BoxedRaycastHit(RaycastHit value) => new BoxedRaycastHit { Value = value };
        public static explicit operator RaycastHit(BoxedRaycastHit boxed) => boxed.Value;
    }
}