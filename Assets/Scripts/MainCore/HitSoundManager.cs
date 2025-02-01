using System.Collections.Generic;
using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.Settings;
using MainCore.UI.Utils;
using MaTech.Audio;

namespace MainCore
{
    public class HitSoundManager : MonoSingleton<HitSoundManager>
    {
        private static Dictionary<int, AudioSource[]> _unityAudios;
        private static Dictionary<int, AudioSample[]> _maAudios;
        private static Dictionary<int, int> _audioIndexes;

        private static float _hitSoundVolume = 1f;

        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private int[] hitSoundsLength;

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
            await RefreshMaAudio();
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
            SkinInfo skinInfo = GlobalSetting.CurrentSkinInfo;
            hitSounds[0] = null;
            hitSounds[1] = skinInfo.clickAc ??
                           await Util.ReadMusicAsAudioClipAsync(SkinManager.Instance.SkinPath +
                                                                $"/{GlobalSetting.CurrentSkinInfo.id}/click.ogg");
            hitSounds[2] = skinInfo.dragAc ??
                           await Util.ReadMusicAsAudioClipAsync(SkinManager.Instance.SkinPath +
                                                                $"/{GlobalSetting.CurrentSkinInfo.id}/drag.ogg");
            hitSounds[3] = skinInfo.clickAc ?? hitSounds[1];
            hitSounds[4] = skinInfo.flickAc ??
                           await Util.ReadMusicAsAudioClipAsync(SkinManager.Instance.SkinPath +
                                                                $"/{GlobalSetting.CurrentSkinInfo.id}/flick.ogg");

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
                _maAudios.Add(i, new AudioSample[hitSoundsLength[i]]);
                _audioIndexes.Add(i, 0);
                for (var j = 0; j < hitSoundsLength[i]; j++)
                {
                    if (!hitSounds[i] && (GlobalSetting.CurrentSkinInfo == null ||
                                          !GlobalSetting.CurrentSkinInfo.isExternal))
                        continue;
                    if (_maAudios[i][j] != null) _maAudios[i][j].Unload();
                    if (i == 0)
                    {
                        _maAudios[i][j] = null;
                        continue;
                    }

                    _maAudios[i][j] = await AudioSample.LoadFromAudioClip(hitSounds[i]);
                    if (_maAudios[i][j] == null)
                    {
                        FallbackLoad();
                        await RefreshMaAudio();
                        return;
                    }

                    _maAudios[i][j].Volume = _hitSoundVolume;
                }
            }
        }

        private void FallbackLoad()
        {
            Debug.Log(
                $"[HitSoundManager] Fallback: {GlobalSetting.CurrentSkinInfo.isExternal} {GlobalSetting.CurrentSkinInfo.skinName}");
            Debug.Log("[HitSoundManager] Can't load hit sounds! Fallback to default hit sound...");
            InGameUIManager.ShowModalWindowWithClose("错误", "无法加载当前皮肤音效，将使用默认音效", () => { }, "确定");
            var fallbackSkinInfo = HitEffectManager.GetInstance().GetInternalSkinInfo(Skin.Official);
            hitSounds[0] = null;
            hitSounds[1] = fallbackSkinInfo.clickAc;
            hitSounds[2] = fallbackSkinInfo.dragAc;
            hitSounds[3] = fallbackSkinInfo.clickAc;
            hitSounds[4] = fallbackSkinInfo.flickAc;
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