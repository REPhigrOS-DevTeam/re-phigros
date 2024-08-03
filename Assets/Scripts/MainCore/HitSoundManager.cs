#define DISABLE_NATIVE_AUDIO
#define USE_MA_AUDIO
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using MainCore.Settings;
using MaTech.Audio;

namespace MainCore
{
    public class HitSoundManager : MonoSingleton<HitSoundManager>
    {
        private static Dictionary<int, AudioSource[]> _unityAudios;
#if USE_MA_AUDIO
        private static Dictionary<int, AudioSample[]> _maAudios;
#endif
        private static Dictionary<int, int> _audioIndexes;

        private static float _hitSoundVolume = 1f;

        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private int[] hitSoundsLength;

        protected override void OnAwake()
        {
#if USE_MA_AUDIO
            InitMaAudio().Forget();
#else
            InitUnityAudio();
#endif
        }

        public static void UpdateVolume()
        {
            _hitSoundVolume = GlobalSetting.HitVolume;
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
#else
            RefreshUnityAudio();
#endif
        }


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
                    if (!hitSounds[i] && (!GlobalSetting.CurrentSkinInfo || !GlobalSetting.CurrentSkinInfo.isExternal))
                        continue;
                    if (_maAudios[i][j] != null) _maAudios[i][j].Unload();
                    if (i == 0)
                    {
                        _maAudios[i][j] = null;
                    }
                    else
                    {
                        _maAudios[i][j] = GlobalSetting.CurrentSkinInfo.isExternal
                            ? await AudioSample.LoadFromExternalUrl(EscapeFilePath(SkinManager.Instance.skinPath + "/" +
                                GlobalSetting.CurrentSkinInfo.id + "/" + i switch
                                {
                                    1 => "click.ogg",
                                    2 => "drag.ogg",
                                    3 => "click.ogg",
                                    4 => "flick.ogg",
                                    _ => throw new ArgumentOutOfRangeException(nameof(i), i, "Illegal HitSound Type Id")
                                }))
                            : await AudioSample.LoadFromAudioClip(hitSounds[i]);
                    }

                    if (_maAudios[i][j] == null) continue;
                    _maAudios[i][j].Volume = _hitSoundVolume;
                }
            }
        }
#endif

        private string EscapeFilePath(string filePath)
        {
            filePath = Path.GetFullPath(filePath).Replace("\\", "/");
            string pathRoot = Path.GetPathRoot(filePath).Replace("\\", "/");
            int system;
            if (pathRoot.EndsWith(":/"))
            {
                system = 0; // Windows
            }
            else if (pathRoot == "/")
            {
                system = 1; // 其他系统
            }
            else
            {
                system = -1;
            }
            
            if (system == -1) throw new ArgumentException("Invalid file path: " + filePath, nameof(filePath));

            filePath = filePath.Substring(pathRoot.Length);

            string pathSeparator = system switch
            {
                0 => "\\",
                1 => "/",
                _ => throw new ArgumentOutOfRangeException()
            };
            return $"file://{pathRoot}{string.Join(pathSeparator, filePath.Split("/").ToList().Select(UnityWebRequest.EscapeURL))}";
        }

        public void Play(int soundIndex, float rewriteVolume = -1)
        {
            var orgVlm = _hitSoundVolume;
            if (rewriteVolume >= 0) _hitSoundVolume = rewriteVolume;

#if USE_MA_AUDIO
            PlayByMaAudio(soundIndex);
#else
            PlayByUnityAudio(soundIndex);
#endif

            _hitSoundVolume = orgVlm;
        }

        private void PlayByUnityAudio(int soundIndex)
        {
            if (_hitSoundVolume <= 0.01f) return;

            var index = _audioIndexes[soundIndex] + 1;
            if (index >= hitSoundsLength[soundIndex]) index = 0;
            var source = _unityAudios[soundIndex][index];
            _audioIndexes[soundIndex] = index;

            // Debug.Log(_hitSoundVolume);
            // Debug.Log(source.clip.samples);
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