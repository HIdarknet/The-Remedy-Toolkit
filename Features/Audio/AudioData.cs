using Remedy.Framework;
//using SaintsField;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.Audio
{
    [CreateAssetMenu(menuName = "Remedy Toolkit/Audio/Audio Data")]
    public class AudioData : SingletonData<AudioData>
    {
        [SerializeField]
        private SerializableDictionary<string, List<AudioDefinition>> _audioDefinitions = new();
        public static SerializableDictionary<string, List<AudioDefinition>> AudioDefinitions
        {
            get
            {
                foreach (var name in Instance._audioDefinitions.Keys)
                {
                    foreach (var clip in Instance._audioDefinitions[name])
                    {
                        clip.Name = name;
                    }

                }
                return Instance._audioDefinitions;
            }
        }

/*
        public static DropdownList<string> GetAudioClips()
        {
            DropdownList<string> list = new();

            try
            {
                foreach (var name in AudioData.AudioDefinitions.Keys)
                {
                    list.Add(name, name);
                }
            }
            catch
            {
                list.Add("No Audio Clips", "");
            }

            return list;
        }*/

        [SerializeField]
        private GameObject _musicSourcePrefab;
        public static GameObject MusicSourcePrefab => Instance._musicSourcePrefab;

        [SerializeField]
        public GameObject _sfxSourcePrefab;
        public static GameObject SFXSourcePrefab => Instance._sfxSourcePrefab;
    }
}