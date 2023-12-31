using System;
using System.Collections;
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

        private AudioSource sfx;
        private float beatTime = .96f * 2f;
        private float lastFrame = 0;

        private bool played = false;
        private float posY = 0;
        private float speed = 1f;
        private float val = 0;

        async void Start()
        {
            tapTransform = tap.transform;
            // 预加载
            foreach (Skin skin in Enum.GetValues(typeof(Skin)))
            {
                SkinInfo skinInfo = HitEffectManager.GetSkinInfo(skin);
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
            tapTransform.localScale = Vector2.one * sizeGetter.value * new Vector2(98.9f, 100f);

            var percentage = ((val - offsetGetter.value / 1000f) % beatTime);

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
                StartCoroutine(ReleaseCondition());
            }

            lastFrame = percentage;
        }

        void HeartBeat()
        {
            DOTween.Clear();
            DOTween.To(() => val, (x) => val = x, beatTime * 1000, beatTime * 1000).SetEase(Ease.Linear);
        }

        IEnumerator ReleaseCondition()
        {
            yield return new WaitForSeconds(.05f);
            played = false;
        }

        public void OnSkinChanged()
        {
            tap.sprite = Resources.Load<GameObject>($"Notes/{GlobalSetting.Skin.ToString()}/Tap")
                .GetComponent<NoteMovement>().NormalSprites[0];
        }
    }
}