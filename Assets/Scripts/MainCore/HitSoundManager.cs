#if UNITY_EDITOR || !UNITY_ANDROID
#define DISABLE_NATIVE_AUDIO
#endif
// #define USE_MA_AUDIO
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;
#if !DISABLE_NATIVE_AUDIO || USE_MA_AUDIO
using Cysharp.Threading.Tasks;
#endif
#if USE_MA_AUDIO
using MaTech.Audio;
#endif
#if !DISABLE_NATIVE_AUDIO
using E7.Native;
#endif

namespace MainCore
{
    public class HitSoundManager : MonoSingleton<HitSoundManager>
    {
        private static Dictionary<int, AudioSource[]> _unityAudios;
#if !DISABLE_NATIVE_AUDIO
        private static Dictionary<int, NativeAudioPointer> _nativeAudios;
#endif
#if USE_MA_AUDIO
        private static Dictionary<int, AudioSample[]> _maAudios;
#endif
        private static Dictionary<int, int> _audioIndexes;

        private static float _hitSoundVolume = 1f;

        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private int[] hitSoundsLength;

#if !DISABLE_NATIVE_AUDIO
        private static NativeSource.PlayOptions _nativeAudioOptions;
        private List<int> nativeIndexes = new();
#endif

        protected override void OnAwake()
        {
#if USE_MA_AUDIO
            InitMaAudio().Forget();
#elif !DISABLE_NATIVE_AUDIO
            InitNativeAudio().Forget();
#else
            InitUnityAudio();
#endif
        }

        //Handle NativeAudio's sounds later to achieve a sync.
        void LateUpdate()
        {
#if !DISABLE_NATIVE_AUDIO
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

#endif
        }

        public static void UpdateVolume()
        {
            _hitSoundVolume = GlobalSetting.HitVolume;
            
#if !DISABLE_NATIVE_AUDIO
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

#if !DISABLE_NATIVE_AUDIO
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
#endif

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

        public async void RefreshHitSounds()
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
#elif !DISABLE_NATIVE_AUDIO
            await RefreshNativeAudio();
#else
            RefreshUnityAudio();
#endif
        }

#if !DISABLE_NATIVE_AUDIO
        private async UniTask RefreshNativeAudio()
        {
            _nativeAudios.Clear();
            for (var i = 0; i < hitSounds.Length; i++)
            {
                if (!hitSounds[i]) continue;
                hitSounds[i].LoadAudioData();
                var i1 = i;
                await UniTask.WaitUntil(() => hitSounds[i1].loadState is AudioDataLoadState.Loaded or AudioDataLoadState.Failed);
                if (hitSounds[i].loadState == AudioDataLoadState.Failed) Debug.Log("???");
                _nativeAudios.Add(i,
                    hitSounds[i].loadState == AudioDataLoadState.Failed ? null : NativeAudio.Load(hitSounds[i]));
            }
        }
#endif


        private void RefreshUnityAudio()
        {
            foreach (AudioSource[] audioSources in _unityAudios.Values)
            {
                foreach (AudioSource audioSource in audioSources)
                {
                    Destroy(audioSource.gameObject);
                }
            }
            _unityAudios.Clear();
            _audioIndexes.Clear();

            for (var i = 0; i < hitSounds.Length; i++)
            {
                _unityAudios.Add(i, new AudioSource[hitSoundsLength[i]]);
                _audioIndexes.Add(i, 0);
                for (var j = 0; j < hitSoundsLength[i]; j++)
                {
                    var obj = new GameObject($"Unity Audio - HitSound {i}-{j}");
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
                    if (!hitSounds[i] && (!GlobalSetting.CurrentSkinInfo || !GlobalSetting.CurrentSkinInfo.isExternal)) continue;
                    if (_maAudios[i][j] != null) _maAudios[i][j].Unload();
                    if (i == 0)
                    {
                        _maAudios[i][j] = null;
                    }
                    else
                    {
                        _maAudios[i][j] = GlobalSetting.CurrentSkinInfo.isExternal
                            ? await AudioSample.LoadFromExternalUrl(UrlEncodePath(SkinManager.Instance.skinPath + "/" + GlobalSetting.CurrentSkinInfo.id + "/" + i switch
                            {
                                1 => "click.ogg",
                                2 => "drag.ogg",
                                3 => "click.ogg",
                                4 => "flick.ogg",
                                _ => throw new ArgumentOutOfRangeException()
                            }))
                            : await AudioSample.LoadFromAudioClip(hitSounds[i]);
                    }

                    if (_maAudios[i][j] == null) continue;
                    _maAudios[i][j].Volume = _hitSoundVolume;
                }
            }
        }
#endif

        private string UrlEncodePath(string path)
        {
            string str = "file://" + string.Join("/", path.Replace("\\", "/").Split('/').Select(UrlEncode));
            Debug.Log(str);
            return str;
        }

        private string UrlEncode(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(str);
            UTF8Encoding utf8Encoding = new UTF8Encoding(false);
            while (textElementEnumerator.MoveNext())
            {
                string qwq = textElementEnumerator.GetTextElement();
                if (qwq.Length == 1)
                {
                    char c = qwq[0];
                    if (c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.' or '~')
                    {
                        stringBuilder.Append(c);
                        continue;
                    }
                }

                stringBuilder.Append(string.Join("", utf8Encoding.GetBytes(qwq).Select(b => "%" + Convert.ToString(b, 16))));
            }

            return stringBuilder.ToString();
        }

        public void Play(int soundIndex, float rewriteVolume = -1)
        {
            var orgVlm = _hitSoundVolume;
            if (rewriteVolume >= 0) _hitSoundVolume = rewriteVolume;

#if USE_MA_AUDIO
            PlayByMaAudio(soundIndex);
#elif !DISABLE_NATIVE_AUDIO
            PlayByNativeAudio(soundIndex);
#else
            PlayByUnityAudio(soundIndex);
#endif

            _hitSoundVolume = orgVlm;
        }

#if !DISABLE_NATIVE_AUDIO
        private void PlayByNativeAudio(int soundIndex)
        {
            if (_hitSoundVolume <= 0.01f) return;

            nativeIndexes.Add(soundIndex);
        }
#endif

        private void PlayByUnityAudio(int soundIndex)
        {
            if (_hitSoundVolume <= 0.01f) return;

            var index = _audioIndexes[soundIndex] + 1;
            if (index >= hitSoundsLength[soundIndex]) index = 0;
            var source = _unityAudios[soundIndex][index];
            _audioIndexes[soundIndex] = index;

            // Debug.Log(_hitSoundVolume);
            // Debug.Log(source.clip.samples);
            float[] f = new float[source.clip.channels * source.clip.samples];
            source.clip.GetData(f, 0);
            // Debug.Log($"[{string.Join(", ", f.Take(Mathf.Min(f.Length, 20)))}]");
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