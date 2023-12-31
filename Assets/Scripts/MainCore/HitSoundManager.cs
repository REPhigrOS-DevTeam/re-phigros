#define DISABLE_NATIVE_AUDIO
// #define USE_MA_AUDIO
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using E7.Native;
using MainCore.Common;
using MainCore.Utilities;
#if USE_MA_AUDIO
using MaTech.Audio;
#endif
using UnityEngine;

namespace MainCore
{
    public class HitSoundManager : MonoSingleton<HitSoundManager>
    {
        private static Dictionary<int, NativeAudioPointer> _nativeAudios;
        private static Dictionary<int, AudioSource[]> _unityAudios;
#if USE_MA_AUDIO
        private static Dictionary<int, AudioSample[]> _maAudios;
#endif
        private static Dictionary<int, int> _audioIndexes;
        private static NativeSource.PlayOptions _nativeAudioOptions;
        private static float _hitSoundVolume = 1f;

        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private int[] hitSoundsLength;

        private List<int> nativeIndexes = new();

        protected override void OnAwake()
        {
#if USE_MA_AUDIO
            InitMaAudio().Forget();
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && !DISABLE_NATIVE_AUDIO
            InitNativeAudio().Forget();
#else
            InitUnityAudio();
#endif
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

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && !DISABLE_NATIVE_AUDIO
            if (NativeAudio.OnSupportedPlatform)
            {
                for (var i = 0; i < NativeAudio.GetNativeSourceCount(); i++)
                {
                    NativeAudio.GetNativeSource(i).SetVolume(_hitSoundVolume);
                }
            }

            _nativeAudioOptions.volume = _hitSoundVolume;
#endif
        }

        public static void UpdateVolume()
        {
            _hitSoundVolume = GlobalSetting.hitVolume;
        }

        private async UniTaskVoid InitNativeAudio()
        {
            if (!NativeAudio.OnSupportedPlatform) return;

            var opt = NativeAudio.InitializationOptions.defaultOptions;
            NativeAudio.Initialize(opt);
            _nativeAudios = new Dictionary<int, NativeAudioPointer>();
            _audioIndexes = new Dictionary<int, int>();
            _nativeAudioOptions = new NativeSource.PlayOptions();

            await RefreshNativeAudio();
        }

        private void InitUnityAudio()
        {
            _unityAudios = new Dictionary<int, AudioSource[]>();
            _audioIndexes = new Dictionary<int, int>();

            RefreshUnityAudio();
        }

#if USE_MA_AUDIO
        private async UniTaskVoid InitMaAudio()
        {
            _maAudios = new Dictionary<int, AudioSample[]>();
            _audioIndexes = new Dictionary<int, int>();

            MaAudio.LoadForUnity();
            await RefreshMaAudio();
        }
#endif

        public void RefreshHitSounds()
        {
            Resources.UnloadUnusedAssets();
            SkinInfo skinInfo = GlobalSetting.CurrentSkinInfo;
            hitSounds[0] = null;
            hitSounds[1] = skinInfo.clickAC;
            hitSounds[2] = skinInfo.dragAC;
            hitSounds[3] = skinInfo.clickAC;
            hitSounds[4] = skinInfo.flickAC;
#if USE_MA_AUDIO
            await RefreshMaAudio();
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && !DISABLE_NATIVE_AUDIO
            await RefreshNativeAudio();
#else
            RefreshUnityAudio();
#endif
        }

        private async UniTask RefreshNativeAudio()
        {
            _nativeAudios.Clear();
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

        private void RefreshUnityAudio()
        {
            _unityAudios.Clear();
            _audioIndexes.Clear();

            for (var i = 0; i < hitSounds.Length; i++)
            {
                _unityAudios.Add(i, new AudioSource[hitSoundsLength[i]]);
                _audioIndexes.Add(i, 0);
                for (var j = 0; j < hitSoundsLength[i]; j++)
                {
                    if (_unityAudios[i][j]) Destroy(_unityAudios[i][j].gameObject);
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

#if USE_MA_AUDIO
        private async UniTask RefreshMaAudio()
        {
            _maAudios.Clear();
            _audioIndexes.Clear();

            for (int i = 0; i < hitSounds.Length; i++)
            {
                _maAudios.Add(i, new AudioSample[hitSoundsLength[i]]);
                _audioIndexes.Add(i, 0);
                for (var j = 0; j < hitSoundsLength[i]; j++)
                {
                    if (!hitSounds[i]) continue;
                    if (_maAudios[i][j] != null) _maAudios[i][j].Unload();
                    _maAudios[i][j] = await AudioSample.LoadFromAudioClip(hitSounds[i]);
                    _maAudios[i][j].Volume = _hitSoundVolume;
                }
            }
        }
#endif

        public void Play(int soundIndex, float rewriteVolume = -1)
        {
            var orgVlm = _hitSoundVolume;
            if (rewriteVolume >= 0) _hitSoundVolume = rewriteVolume;

#if USE_MA_AUDIO
            PlayByMaAudio(soundIndex);
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && !DISABLE_NATIVE_AUDIO
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

#if USE_MA_AUDIO
        private void PlayByMaAudio(int soundIndex)
        {
            if (_hitSoundVolume <= 0.01f) return;

            var index = _audioIndexes[soundIndex] + 1;
            if (index >= hitSoundsLength[soundIndex]) index = 0;
            var source = _maAudios[soundIndex][index];
            _audioIndexes[soundIndex] = index;

            source.Volume = _hitSoundVolume;
            source.Channel = (ushort)(soundIndex * 10 + index); // 自动分配音轨
            source.PlayImmediate();
        }
#endif
    }
}