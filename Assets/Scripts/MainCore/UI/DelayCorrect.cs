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
        [SerializeField] private Transform auxiliaryLineTransform;
        private Transform tapTransform;
        [SerializeField] private Slider sizeGetter;
        [SerializeField] private Slider hitSfxVolumeGetter;
        [SerializeField] private Slider offsetGetter;
        private bool running = true;

        private AudioSource sfx;
        private float beatTime = .96f * 2f;
        private float lastFrame = -1;

        private bool played = false;
        private float posY = 0;
        private float speed = 1f;
        private float val = 0;

        private Stopwatch stopwatch = new();
        private float delay;
        private float ElapsedTime => stopwatch.ElapsedMilliseconds / 1000f - delay;

        private float volume = 0.5f;

        async void Start()
        {
            tapTransform = tap.transform;
            // 预加载
            foreach (Skin skin in Enum.GetValues(typeof(Skin)))
            {
                SkinInfo skinInfo = HitEffectManager.GetInstance().GetInternalSkinInfo(skin);
                EffectManager effectManager = HitEffectManager.GetInstance().GetObj(HitFxJudgeType.Perfect, skinInfo);
                effectManager.transform.position = Camera.main.transform.position - new Vector3(0f, 0f, 1f);
                effectManager.PlayEffect();
            }

            sfx = GetComponent<AudioSource>();
            sfx.volume = volume;
            await UniTask.Delay(3000);
            speed = 1400 / beatTime;
            if (sfx) sfx.PlayScheduled(AudioSettings.dspTime);
            delay = beatTime / 2f;
            // auxiliaryLineTransform.localPosition = new Vector2(0f, 1000f);
            HeartBeat();
        }


        void Update()
        {
            if (!running)
            {
                tapTransform.localPosition = new Vector2(0f, 1000f);
                return;
            }

            tapTransform.localScale = Vector2.one * sizeGetter.value * new Vector2(98.9f, 100f);

            float offsetTime = ElapsedTime - offsetGetter.value / 1000f;
            if (offsetTime < 0) offsetTime = 0f;
            var percentage = (offsetTime % beatTime);

            posY = 1000 - speed * percentage;

            tapTransform.localPosition = new Vector2(0f, posY);
            // if (CheckInput())
            // {
            //     auxiliaryLineTransform.localPosition = new Vector2(0f, posY);
            // }

            if (lastFrame > percentage && !played)
            {
                //AudioSource.PlayClipAtPoint(tapSound, Camera.main.transform.position);
                HitSoundManager.Instance.Play(1, hitSfxVolumeGetter.value);
                tapTransform.localPosition = new Vector2(0f, -400f);
                EffectManager hitFxObj = HitEffectManager.GetInstance().GetObj(HitFxJudgeType.Perfect, GlobalSetting.CurrentSkinInfo, true);
                hitFxObj.transform.position = tapTransform.position;
                hitFxObj.transform.rotation = Quaternion.identity;
                hitFxObj.PlayEffect();
                played = true;
                StartCoroutine(ReleaseCondition());
            }

            lastFrame = percentage;
        }

//         private bool CheckInput()
//         {
// #if UNITY_EDITOR || UNITY_STANDALONE
//             if (Input.GetMouseButtonDown(0) &&
//                 1 - Input.mousePosition.x / Screen.width is > 0 and < 500f / 1920f) // 500是背景宽度，1920是参考分辨率宽度
//             {
//                 return true;
//             }
// #endif
//             for (int i = 0; i < Input.touchCount; i++)
//             {
//                 Touch touch = Input.GetTouch(i);
//                 if (touch.phase == TouchPhase.Began && 1 - touch.position.x / Screen.width is > 0 and < 500f / 1920f)
//                 {
//                     return true;
//                 }
//             }
//
//             return false;
//         }

        void HeartBeat()
        {
            if (stopwatch.IsRunning) stopwatch.Reset();
            stopwatch.Start();
        }

        public void SetRunning(bool state)
        {
            running = state;
            if (state)
            {
                stopwatch.Restart();
                if (!sfx) return;
                sfx.volume = volume;
                sfx.PlayScheduled(AudioSettings.dspTime);
            }
            else
            {
                lastFrame = -1;
                stopwatch.Stop();
                if (!sfx) return;
                sfx.volume = 0f;
                sfx.Stop();
                sfx.time = 0f;
            }
        }

        IEnumerator ReleaseCondition()
        {
            yield return new WaitForSeconds(.05f);
            played = false;
        }

        public void OnSkinChanged()
        {
            tap.sprite = GlobalSetting.CurrentSkinInfo.click;
        }
    }
}