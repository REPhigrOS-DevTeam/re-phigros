using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class DelayCorrect : MonoBehaviour
    {
        [SerializeField] private Image tap;
        [SerializeField] private Slider sizeGetter;
        [SerializeField] private Slider hitSfxVolumeGetter;
        [SerializeField] private Slider offsetGetter;
        [SerializeField] private AudioClip tapSound;

        private AudioSource audio;
        private float beatTime = .96f * 2f;
        private float lastFrame = 0;

        private bool played = false;
        private float posY = 0;
        private float speed = 1f;
        private float val = 0;

        async void Start()
        {
            EffectManager effectManager = HitEffectManager.GetInstance().GetObj(HitFxJudgeType.Perfect);
            effectManager.transform.position = Camera.main.transform.position - new Vector3(0, 0, 1);
            effectManager.PlayParticle();
            audio = GetComponent<AudioSource>();
            await Task.Delay(3000);
            speed = 1400 / beatTime;
            audio.PlayScheduled(AudioSettings.dspTime);
            await Task.Delay((int) (beatTime * 500));
            HeartBeat();
        }


        void Update()
        {
            tap.rectTransform.localScale = Vector2.one * sizeGetter.value;

            var percentage = ((val - offsetGetter.value / 1000f) % beatTime);

            posY = 1000 - speed * percentage;

            tap.rectTransform.anchoredPosition = new Vector2(-250, posY);

            if (lastFrame > percentage && !played)
            {
                //AudioSource.PlayClipAtPoint(tapSound, Camera.main.transform.position);
                HitSoundManager.Instance.Play(1, hitSfxVolumeGetter.value);
                tap.rectTransform.anchoredPosition = new Vector2(-250, -400);
                EffectManager hitFxObj;
                //hitFxObj = ObjectPool.GetInstance().GetObj($"HitFX/clickRaw_{HitEffectManager.HitFxType}_{HitFxJudgeType.Perfect}");
                hitFxObj = HitEffectManager.GetInstance().GetObj(HitFxJudgeType.Perfect);
                hitFxObj.transform.position = tap.transform.position;
                hitFxObj.PlayParticle();
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
    }
}