using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MainCore;
using MainCore.ECS_ver;
using MainCore.Utilities;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public SpriteRenderer sr;
    private Color color;
    [SerializeField] private int cnt;
    public Coroutine AnimationCoroutine;
    private Sprite[] hitFx;
    private float hitFxFactor;

    public Coroutine RecycleCoroutine = null;

    float scale = GlobalSetting.GlobalNoteScale / 0.16f;

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
        //sr.sortingLayerName = "AboveNotes";
        //sr.sortingOrder = 1;
        hitFx = skinInfo.hitFx;
        hitFxFactor = !GlobalSetting.CurrentSkinInfo.isExternal && GlobalSetting.CurrentSkinInfo.skin == Skin.OldOfficial
            ? 120f
            : hitFx.Length / GlobalSetting.CurrentSkinInfo.hitFxDuration;
        RecycleCoroutine = StartCoroutine(RecycleObj());
        sr.color = color;
    }

    public void PlayEffect()
    {
        StopEffect();
        AnimationCoroutine = StartCoroutine(PlayEffectE());
    }

    public void PlayParticle()
    {
        EffectSystemManager.Instance.CreateParticle(cnt, color, transform.position, scale);
    }

    private IEnumerator RecycleObj()
    {
        yield return new WaitForSeconds(GlobalSetting.CurrentSkinInfo.hitFxDuration);
        HitEffectManager.GetInstance().RecycleObj(this);
    }

    public void ForceRecycle()
    {
        StopCoroutine(RecycleCoroutine);
        HitEffectManager.GetInstance().RecycleObj(this);
    }
    private IEnumerator PlayEffectE()
    {
        if (hitFx == null) yield break;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        float timer;
        while ((timer = stopwatch.ElapsedMilliseconds / 1000f * hitFxFactor) < hitFx.Length)
        {
            sr.sprite = hitFx[(int) timer];
            yield return null;
        }
        sr.sprite = hitFx[^1];
    }
    
    public void StopEffect()
    {
        if (AnimationCoroutine != null) StopCoroutine(AnimationCoroutine);
    }
}