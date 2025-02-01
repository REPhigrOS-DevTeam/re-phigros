using System;
using System.Collections;
using System.Diagnostics;
using MainCore.Data;
using MainCore.ECS;
using UnityEngine;

namespace MainCore
{
    public class EffectManager : MonoBehaviour
    {
        public SpriteRenderer sr;
        private Color color;
        [SerializeField] private int cnt;
        public Coroutine AnimationCoroutine;
        private Sprite[] hitFx;
        private float hitFxFactor;
        private bool hideParticles = true;

        public Coroutine RecycleCoroutine = null;

        float scale = GlobalSetting.GlobalNoteScale / 0.16f;

        private Stopwatch hitEffectTime = new Stopwatch();

        // Start is called before the first frame update 
        private void Awake()
        {
            transform.localScale = new Vector3(0.001f, 0.001f, 1f);
        }

        public void Enable(SkinInfo skinInfo, HitFxJudgeType JudgeType)
        {
            color = skinInfo.hitFxTinted
                ? JudgeType switch
                {
                    HitFxJudgeType.Perfect => skinInfo.perfectColor,
                    HitFxJudgeType.Good => skinInfo.goodColor,
                    _ => throw new ArgumentOutOfRangeException(nameof(JudgeType), JudgeType, null)
                }
                : Color.white;
            scale = GlobalSetting.GlobalNoteScale / 0.16f * skinInfo.hitFxScale;
            transform.localScale = new Vector3(scale, scale, scale);
            gameObject.layer = 7; // ParticleLayer
            //sr.sortingLayerName = "AboveNotes";
            //sr.sortingOrder = 1;
            hitFx = skinInfo.hitFx;
            hitFxFactor = !skinInfo.isExternal && skinInfo.skin == Skin.OldOfficial
                ? 120f
                : hitFx.Length / skinInfo.hitFxDuration;
            hideParticles = skinInfo.hideParticles;
            RecycleCoroutine = StartCoroutine(RecycleObj());
            sr.color = color;
        }

        private void Start()
        {
            Update();
        }

        private void Update()
        {
            int timer = (int) (hitEffectTime.ElapsedMilliseconds / 1000f * hitFxFactor);
            sr.sprite = hitFx[Mathf.Min(timer, hitFx.Length - 1)];
        }

        public void PlayEffect()
        {
            StopEffect();
            hitEffectTime.Restart();
            if (hideParticles) return;
            EffectSystemManager.Instance.CreateParticle(cnt, color, transform.position, scale);
        }

        private IEnumerator RecycleObj()
        {
            yield return new WaitForSeconds(GlobalSetting.CurrentSkinInfo.hitFxDuration);
            StopEffect();
            HitEffectManager.GetInstance().RecycleObj(this);
        }

        public void ForceRecycle()
        {
            StopCoroutine(RecycleCoroutine);
            StopEffect();
            HitEffectManager.GetInstance().RecycleObj(this);
        }

        public void StopEffect()
        {
            hitEffectTime.Reset();
        }
    }
}