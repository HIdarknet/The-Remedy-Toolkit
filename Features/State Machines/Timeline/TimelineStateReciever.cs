using System.Collections.Generic;
using System.Linq;
using Remedy.StateMachines;
using Remedy.StateMachines.ForGameObjects;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Remedy.StateMachines.Timeline
{
    class TimelineStateReciever : MonoBehaviour, INotificationReceiver
    {
        private PlayableDirector _playableDirector;
        public PlayableDirector PlayableDirector => _playableDirector ??= GetComponent<PlayableDirector>();
        private MarkerTrack _markerTrack;
        private List<StateBookmark> _markers;
        private ScriptMachine _stateMachineGraph;
        private string _currentState = "";
        public string CurrentState => _currentState;


        void Awake()
        {
            var timelineAsset = PlayableDirector.playableAsset as TimelineAsset;
            _markerTrack = timelineAsset.markerTrack;
            _markers = _markerTrack.GetMarkers().OfType<StateBookmark>().ToList();
            _stateMachineGraph = GetComponent<ScriptMachine>();
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is StateBookmark marker)
            {
                gameObject.SetState(marker.State);
            }
        }

        public void GoToState(string stateName)
        {
            var marker = _markers.FirstOrDefault(marker => marker.State == stateName);
            if (marker != null)
            {
                PlayableDirector.time = marker.time + 0.1f;
                if(!marker.PauseHere)
                    PlayableDirector.Play();
            }
            _currentState = stateName;
        }
    }
}
