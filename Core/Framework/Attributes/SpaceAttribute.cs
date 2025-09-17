using System;
using System.Collections;
using UnityEngine;

namespace Remedy.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class SpaceAttribute : PropertyAttribute
    {
        //
        // Summary:
        //     The spacing in pixels.
        public readonly float height;

        public SpaceAttribute()
        {
            height = 8f;
        }

        //
        // Summary:
        //     Use this DecoratorDrawer to add some spacing in the Inspector.
        //
        // Parameters:
        //   height:
        //     The spacing in pixels.
        public SpaceAttribute(float height)
        {
            this.height = height;
        }
    }
}