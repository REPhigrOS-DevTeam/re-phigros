using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class DelayCorrect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tap;
        private Transform tapTransform;
        [SerializeField] private Slider sizeGetter;
        [SerializeField] private Slider hitSfxVolumeGetter;
        [SerializeField] private Slider offsetGetter;
        [SerializeField] private AudioClip tapSound;
        private bool running = true;

        private AudioSource sfx;
        private float beatTime = .96f * 2f;
        private float lastFrame = 0;

        private bool played = false;
        private float posY = 0;
        private float speed = 1f;
        private float val = 0;

        private Stopwatch stopwatch = new();

        private Coroutine releaseCoroutine;

        async void Start()
        {
            tapTransform = tap.transform;
            // 预加载
            foreach (Skin skin in Enum.GetValues(typeof(Skin)))
            {
                SkinInfo skinInfo = HitEffectManager.GetInstance().GetInternalSkinInfo(skin);
                EffectManager effectManager = HitEffectManager.GetInstance().GetObj(HitFxJudgeType.Perfect, skinInfo);
                effectManager.transform.position = Camera.main.transform.position - new Vector3(0, 0, 1);
                effectManager.PlayEffect();
                if (!skinInfo.hideParticles) effectManager.PlayParticle();
            }
            sfx = GetComponent<AudioSource>();
            await UniTask.Delay(3000);
            speed = 1400 / beatTime;
            if (sfx) sfx.PlayScheduled(AudioSettings.dspTime);
            await UniTask.Delay((int) (beatTime * 500));
            HeartBeat();
        }


        void Update()
        {
            if (!running)
            {
                tapTransform.localPosition = new Vector2(0, -400);
                return;
            }
            tapTransform.localScale = Vector2.one * sizeGetter.value * new Vector2(98.9f, 100f);

            var percentage = ((stopwatch.ElapsedMilliseconds / 1000f - offsetGetter.value / 1000f) % beatTime);

            posY = 1000 - speed * percentage;

            tapTransform.localPosition = new Vector2(0, posY);

            if (lastFrame > percentage && !played)
            {
                //AudioSource.PlayClipAtPoint(tapSound, Camera.main.transform.position);
                HitSoundManager.Instance.Play(1, hitSfxVolumeGetter.value);
                tapTransform.localPosition = new Vector2(0, -400);
                EffectManager hitFxObj;
                //hitFxObj = ObjectPool.GetInstance().GetObj($"HitFX/clickRaw_{HitEffectManager.HitFxType}_{HitFxJudgeType.Perfect}");
                hitFxObj = HitEffectManager.GetInstance().GetObj(HitFxJudgeType.Perfect, GlobalSetting.CurrentSkinInfo);
                hitFxObj.transform.position = tapTransform.position;
                hitFxObj.PlayEffect();
                if (!GlobalSetting.CurrentSkinInfo.hideParticles) hitFxObj.PlayParticle();
                played = true;
                releaseCoroutine = StartCoroutine(ReleaseCondition());
            }

            lastFrame = percentage;
        }

        void HeartBeat()
        {
            if (stopwatch.IsRunning) stopwatch.Reset();
            stopwatch.Start();
        }

        public void SetRunning(bool state)
        {
            if (state)
            {
                stopwatch.Start();
                if (sfx) sfx.PlayScheduled(AudioSettings.dspTime);
            }
            else
            {
                stopwatch.Reset();
                if (sfx) sfx.Stop();
            }
        }

        IEnumerator ReleaseCondition()
        {
            yield return new WaitForSeconds(.05f);
            played = false;
            releaseCoroutine = null;
        }

        public void OnSkinChanged()
        {
            tap.sprite = GlobalSetting.CurrentSkinInfo.click;
        }
    }
}