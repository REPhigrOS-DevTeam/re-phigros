using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using DG.Tweening;
using MainCore;
using MainCore.ECS_ver;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public SpriteRenderer sr;
    [SerializeField] private Color color;
    [SerializeField] private int cnt;
    public Coroutine AnimationCoroutine;
    private Skin skin;
    private Sprite[] hitFx;

    public Coroutine RecycleCoroutine = null;

    float scale = GlobalSetting.globalNoteScale / 0.16f;

    // Start is called before the first frame update 
    private void Awake()
    {
        transform.localScale = new Vector3(0.001f, 0.001f, 1f);
    }

    public void Enable(Skin skin)
    {
        scale = GlobalSetting.globalNoteScale / GetFactor(skin);
        transform.localScale = new Vector3(scale, scale, scale);
        //sr.sortingLayerName = "AboveNotes";
        //sr.sortingOrder = 1;
        hitFx = HitEffectManager.GetSkinInfo(skin).hitFx;
        RecycleCoroutine = StartCoroutine(RecycleObj());
    }

    public void PlayParticle()
    {
        EffectSystemManager.Instance.CreateParticle(cnt, color, transform.position, scale);
        StopEffect();
        AnimationCoroutine = StartCoroutine(PlayEffect());
    }

    private IEnumerator RecycleObj()
    {
        yield return new WaitForSeconds(0.49f);
        HitEffectManager.GetInstance().RecycleObj(this);
    }

    public void ForceRecycle()
    {
        StopCoroutine(RecycleCoroutine);
        // AnimationCoroutine = StartCoroutine(PlayEffect());
        HitEffectManager.GetInstance().RecycleObj(this);
    }

    private static readonly WaitForSeconds Frame60 = new WaitForSeconds(1 / 60f);

    private IEnumerator PlayEffect()
    {
        if (hitFx == null) yield break;
        foreach (var sprite in hitFx)
        {
            sr.sprite = sprite;
            yield return Frame60;
        }
    }
    
    public void StopEffect()
    {
        if (AnimationCoroutine != null) StopCoroutine(AnimationCoroutine);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetFactor(Skin skin)
    {
        return skin switch {
            Skin.Official => 0.215f,
            Skin.Phira => 0.32f,
            Skin.OldOfficial => 0.16f,
            Skin.Sacabam => 0.25f,
            Skin.StarPinkXz => 0.16f,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}