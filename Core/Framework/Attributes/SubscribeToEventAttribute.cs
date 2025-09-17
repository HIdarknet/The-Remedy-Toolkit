using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.Events;

namespace Remedy.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SubscribeToEventAttribute : Attribute
    {
        public string UnityEventName { get; }

        public SubscribeToEventAttribute(string eventName)
        {
            UnityEventName = eventName;
        }
    }
}