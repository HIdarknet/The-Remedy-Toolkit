using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Remedy.StateMachines.Timeline
{
    [CustomStyle("Bookmark")]
    public class StateBookmark : Marker, INotification
    {
        public PropertyName id { get; }
        [SerializeField]
        private string _state = "";
        public string State => _state;
        [SerializeField]
        private bool _pauseHere;
        public bool PauseHere => _pauseHere;
    }
}
