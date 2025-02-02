using System;
using System.Collections.Generic;
using System.IO;
using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.Settings;
using MainCore.UI.Utils;
using MaTech.Audio;
using UnityEngine.Windows;
using File = System.IO.File;

namespace MainCore
{
    public class HitSoundManager : MonoSingleton<HitSoundManager>
    {
        private static Dictionary<int, AudioSource[]> _unityAudios;
        private static Dictionary<int, AudioSample[]> _maAudios;
        private static Dictionary<int, int> _audioIndexes;

        private static float _hitSoundVolume = 1f;

        [SerializeField] private static AudioClip[] hitSounds = new AudioClip[5];
        private static string[] hitSoundPaths = new string[5];
        [SerializeField] private int[] hitSoundsLength;
        [SerializeField] private SkinInfo debugInfo;

        private bool _initialized = false;

        protected override void OnAwake()
        {
            InitMaAudio().Forget();
        }

        public static void UpdateVolume()
        {
            _hitSoundVolume = GlobalSetting.HitVolume;
        }

        private AsyncMethodSequencer _refreshHitSoundSequencer;
        private AsyncMethodSequencer _refreshMaAudioSequencer;

        private async UniTaskVoid InitMaAudio()
        {
            _maAudios = new Dictionary<int, AudioSample[]>();
            _audioIndexes = new Dictionary<int, int>();

            MaAudio.LoadForUnity();
            _initialized = true;
            // await RefreshMaAudio();
        }

        public async void RefreshHitSounds()
        {
            _refreshHitSoundSequencer ??= new AsyncMethodSequencer(RefreshHitSoundsInternal);
            await _refreshHitSoundSequencer.Invoke();
        }

        public async UniTask RefreshHitSoundsInternal()
        {
            await UniTask.WaitUntil(() => SkinManager.Instance.Initialized && _initialized);
            Debug.Log("[HitSoundManager] Refreshing hit sounds...");
            Resources.UnloadUnusedAssets();
            hasFallback = false;
            SkinInfo skinInfo = GlobalSetting.CurrentSkinInfo;
            debugInfo = skinInfo;
            hitSoundPaths = new string[5];
            hitSounds[0] = null;
            hitSounds[1] = skinInfo.clickAc;
            hitSounds[2] = skinInfo.dragAc;
            hitSounds[3] = hitSounds[1];
            hitSounds[4] = skinInfo.flickAc;
            if (skinInfo.isExternal)
            {
                hitSoundPaths = new string[5];
                hitSoundPaths[0] = null;
                hitSoundPaths[1] = skinInfo.clickAcPath;
                hitSoundPaths[2] = skinInfo.dragAcPath;
                hitSoundPaths[3] = skinInfo.clickAcPath;
                hitSoundPaths[4] = skinInfo.flickAcPath;
            }

            await UniTask.WaitUntil(() => _refreshHitSoundSequencer != null);
            await RefreshMaAudio();
            Debug.Log("[HitSoundManager] Done.");
        }

        private async UniTask RefreshMaAudio()
        {
            _refreshMaAudioSequencer ??= new AsyncMethodSequencer(RefreshMaAudioInternal);
            await _refreshMaAudioSequencer.Invoke();
        }

        private async UniTask RefreshMaAudioInternal()
        {
            _maAudios.Clear();
            _audioIndexes.Clear();
            for (int i = 0; i < hitSounds.Length; i++)
            {
                AudioSample[] audioSamples = new AudioSample[hitSoundsLength[i]];
                _maAudios.Add(i, audioSamples);
                _audioIndexes.Add(i, 0);
                if (!hitSounds[i])
                    continue;
                for (var j = 0; j < hitSoundsLength[i]; j++)
                {
                    if (audioSamples[j] != null) _maAudios[i][j].Unload();
                    if (i == 0)
                    {
                        audioSamples[j] = null;
                        continue;
                    }

                    audioSamples[j] = await AudioSample.LoadFromAudioClip(hitSounds[i]);
                    if (audioSamples[j] == null)
                    {
                        if (File.Exists(hitSoundPaths[i])) // 尝试直接从url读取
                        {
                            audioSamples[j] =
                                await AudioSample.LoadFromExternalUrl(ConvertFilePathToFileUrl(hitSoundPaths[i]));
                            if (audioSamples[j] != null) goto next;
                        }

                        if (hasFallback) throw new Exception("Can't load hit sounds!");
                        FallbackLoad();
                        RefreshMaAudio().Forget(); // await会阻塞
                        return;
                    }

                    next:
                    _maAudios[i][j].Volume = _hitSoundVolume;
                }
            }
        }

        private static string ConvertFilePathToFileUrl(string filePath)
        {
            // 将路径分割为各个部分
            string[] parts = Path.GetFullPath(filePath).Replace("\\", "/").Split('/');

            // 对每个部分进行 URL 编码
            // 第一个是空的或驱动器名，不用编码
            for (int i = 1; i < parts.Length; i++)
            {
                parts[i] = Uri.EscapeDataString(parts[i]);
            }

            // 重新组合路径
            string encodedPath = string.Join(Path.DirectorySeparatorChar, parts);

            Debug.Log("file://" + encodedPath);
            
            // 添加 file:// 协议
            return "file://" + encodedPath;
        }

        private bool hasFallback = false;

        private void FallbackLoad()
        {
            hasFallback = true;
            Debug.Log(
                $"[HitSoundManager] Fallback: {GlobalSetting.CurrentSkinInfo.isExternal} {GlobalSetting.CurrentSkinInfo.skinName}");
            Debug.Log("[HitSoundManager] Can't load hit sounds! Fallback to default hit sound...");
            InGameUIManager.ShowModalWindowWithClose("错误", "无法加载当前皮肤音效，将使用默认音效", () => { }, "确定");
            var fallbackSkinInfo = SkinManager.Instance.GetInternalSkinInfo(Skin.Official);
            hitSounds[0] = null;
            hitSounds[1] = fallbackSkinInfo.clickAc;
            hitSounds[2] = fallbackSkinInfo.dragAc;
            hitSounds[3] = fallbackSkinInfo.clickAc;
            hitSounds[4] = fallbackSkinInfo.flickAc;
            hitSoundPaths = new string[5];
            hitSoundPaths[0] = null;
            hitSoundPaths[1] = fallbackSkinInfo.clickAcPath;
            hitSoundPaths[2] = fallbackSkinInfo.dragAcPath;
            hitSoundPaths[3] = fallbackSkinInfo.clickAcPath;
            hitSoundPaths[4] = fallbackSkinInfo.flickAcPath;
        }

        public void Play(int soundIndex, float rewriteVolume = -1)
        {
            var orgVlm = _hitSoundVolume;
            if (rewriteVolume >= 0) _hitSoundVolume = rewriteVolume;

            PlayByMaAudio(soundIndex);

            _hitSoundVolume = orgVlm;
        }

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
    }
}