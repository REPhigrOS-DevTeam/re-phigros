using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using E7.Native;
using MainCore.Common;
using UnityEngine;

namespace MainCore
{
    public class HitSoundManager : MonoSingleton<HitSoundManager>
    {
        private static Dictionary<int, NativeAudioPointer> _nativeAudios;
        private static Dictionary<int, AudioSource[]> _unityAudios;
        private static Dictionary<int, int> _audioIndexes;
        private static NativeSource.PlayOptions _nativeAudioOptions;
        private static float _hitSoundVolume = 1f;

        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private int[] hitSoundsLength;

        private List<int> nativeIndexes = new();

        protected override void OnAwake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            InitNativeAudio().Forget();
#else
            InitUnityAudio();
#endif
            DontDestroyOnLoad(gameObject);
        }

        //Handle NativeAudio's sounds later to achieve a sync.
        void LateUpdate()
        {
            if (nativeIndexes.Count == 0) return;

            int tapCnt = 0, dragCnt = 0, flickCnt = 0;

            foreach (var i in nativeIndexes)
            {
                if (i is 1 or 3) tapCnt++;
                else if (i is 2) dragCnt++;
                else flickCnt++;
            }

            tapCnt = tapCnt > 3 ? 3 : tapCnt;
            dragCnt = dragCnt > 3 ? 3 : dragCnt;
            flickCnt = flickCnt > 2 ? 2 : flickCnt;

            if (dragCnt + flickCnt == 0)
            {
                while (tapCnt-- > 0)
                {
                    var pointer = _nativeAudios[1];
                    var source = NativeAudio.GetNativeSourceAuto();
                    source.Play(pointer, _nativeAudioOptions);
                }
            }
            else if (dragCnt == 0)
            {
                int cnt = 0;
                while (tapCnt-- > 0)
                {
                    cnt++;
                    var pointer = _nativeAudios[1];
                    var source = NativeAudio.GetNativeSourceAuto();
                    source.Play(pointer, _nativeAudioOptions);
                }

                while (flickCnt-- > 0 && cnt < 3)
                {
                    cnt++;
                    var pointer = _nativeAudios[4];
                    var source = NativeAudio.GetNativeSourceAuto();
                    source.Play(pointer, _nativeAudioOptions);
                }
            }
            else if (flickCnt == 0)
            {
                int cnt = 0;
                while (tapCnt-- > 0)
                {
                    cnt++;
                    var pointer = _nativeAudios[1];
                    var source = NativeAudio.GetNativeSourceAuto();
                    source.Play(pointer, _nativeAudioOptions);
                }

                while (dragCnt-- > 0 && cnt < 3)
                {
                    cnt++;
                    var pointer = _nativeAudios[2];
                    var source = NativeAudio.GetNativeSourceAuto();
                    source.Play(pointer, _nativeAudioOptions);
                }
            }
            else
            {
                var pointer = _nativeAudios[1];
                var source = NativeAudio.GetNativeSourceAuto();
                source.Play(pointer, _nativeAudioOptions);
                pointer = _nativeAudios[2];
                source = NativeAudio.GetNativeSourceAuto();
                source.Play(pointer, _nativeAudioOptions);
                pointer = _nativeAudios[4];
                source = NativeAudio.GetNativeSourceAuto();
                source.Play(pointer, _nativeAudioOptions);
            }

            nativeIndexes.Clear();
        }

        public static void Init()
        {
            _hitSoundVolume = GlobalSetting.hitVolume;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (NativeAudio.OnSupportedPlatform)
            {
                for (var i = 0; i < NativeAudio.GetNativeSourceCount(); i++)
                {
                    NativeAudio.GetNativeSource(i).SetVolume(_hitSoundVolume);
                }
            }
#endif

            _nativeAudioOptions.volume = _hitSoundVolume;
        }

        private async UniTaskVoid InitNativeAudio()
        {
            if (!NativeAudio.OnSupportedPlatform) return;

            var opt = NativeAudio.InitializationOptions.defaultOptions;
            NativeAudio.Initialize(opt);
            _nativeAudios = new Dictionary<int, NativeAudioPointer>();
            _audioIndexes = new Dictionary<int, int>();
            _nativeAudioOptions = new NativeSource.PlayOptions();

            for (var i = 0; i < hitSounds.Length; i++)
            {
                hitSounds[i].LoadAudioData();
                while (hitSounds[i].loadState != AudioDataLoadState.Loaded)
                {
                    await Task.Delay(20);
                }

                _nativeAudios.Add(i, NativeAudio.Load(hitSounds[i]));
            }
        }

        private void InitUnityAudio()
        {
            _unityAudios = new Dictionary<int, AudioSource[]>();
            _audioIndexes = new Dictionary<int, int>();

            for (var i = 0; i < hitSounds.Length; i++)
            {
                _unityAudios.Add(i, new AudioSource[hitSoundsLength[i]]);
                _audioIndexes.Add(i, 0);
                for (var j = 0; j < hitSoundsLength[i]; j++)
                {
                    var obj = new GameObject("Unity Audio - HitSound");
                    obj.transform.SetParent(transform);
                    obj.transform.position = new Vector3(0, 0, -10);
                    var comp = obj.AddComponent<AudioSource>();
                    comp.loop = false;
                    comp.playOnAwake = false;
                    comp.clip = hitSounds[i];
                    _unityAudios[i][j] = comp;
                }
            }
        }

        public void Play(int soundIndex, float rewriteVolume = -1)
        {
            var orgVlm = _hitSoundVolume;
            if (rewriteVolume >= 0 ) _hitSoundVolume = rewriteVolume;

#if UNITY_ANDROID && !UNITY_EDITOR
            PlayByNativeAudio(soundIndex);
#else
            PlayByUnityAudio(soundIndex);
#endif

            _hitSoundVolume = orgVlm;
        }

        private void PlayByNativeAudio(int soundIndex)
        {
            if (_hitSoundVolume <= 0.01f) return;

            nativeIndexes.Add(soundIndex);
        }

        private void PlayByUnityAudio(int soundIndex)
        {
            if (_hitSoundVolume <= 0.01f) return;

            var index = _audioIndexes[soundIndex] + 1;
            if (index >= hitSoundsLength[soundIndex]) index = 0;
            var source = _unityAudios[soundIndex][index];
            _audioIndexes[soundIndex] = index;

            source.volume = _hitSoundVolume;
            source.PlayScheduled(AudioSettings.dspTime);
        }
    }
}