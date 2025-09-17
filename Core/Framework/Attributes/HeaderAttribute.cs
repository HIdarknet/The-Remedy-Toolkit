using System;
using System.Collections;
using UnityEngine;

namespace Remedy.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class HeaderAttribute : PropertyAttribute
    {
        //
        // Summary:
        //     The header text.
        public readonly string header;

        //
        // Summary:
        //     Add a header above some fields in the Inspector.
        //
        // Parameters:
        //   header:
        //     The header text.
        public HeaderAttribute(string header)
        {
            this.header = header;
        }
    }
}