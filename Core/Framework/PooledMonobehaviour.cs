//using SaintsField;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PooledMonobehaviour<T> : MonoBehaviour where T : PooledMonobehaviour<T> 
{
    //[ReadOnly]
    public int ID;
    //[ReadOnly]
    public string GUID = "";
    public static Dictionary<string, T> Lookup = MonoBehaviourPool.Lookup.Where(item => item.Value.GetType() == typeof(T))
                                                                                .ToDictionary(item => item.Key, item => (T)item.Value);

    private void Start()
    {
        OnValidate();
    }
    private void OnValidate()
    {
        if (GUID == "")
        {
            GUID = System.Guid.NewGuid().ToString();
            ID = Lookup.Count();
        }

        Lookup.Add(GUID, (T)this);
    }

    private void OnDestroy()
    {
        Lookup.Remove(GUID);
    }
}

public struct NetworkPair<T1, T2> : IEquatable<NetworkPair<T1, T2>>
    where T1 : unmanaged, IEquatable<T1>
    where T2 : unmanaged, IEquatable<T2>
{
    public T1 Key;
    public T2 Value;

    public NetworkPair(T1 key, T2 value)
    {
        Key = key;
        Value = value;
    }

    public bool Equals(NetworkPair<T1, T2> other)
    {
        return Key.Equals(other.Key) && Value.Equals(other.Value);
    }

    public override bool Equals(object obj)
    {
        return obj is NetworkPair<T1, T2> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Value);
    }

    public override string ToString()
    {
        return $"[{Key}, {Value}]";
    }
}
