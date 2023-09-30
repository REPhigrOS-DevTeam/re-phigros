using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MainCore;
using MainCore.ECS_ver;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public SpriteRenderer sr;
    [SerializeField] private Color color;
    [SerializeField] private int cnt;
    public Animator animator;

    public Coroutine RecycleCoroutine = null;

    float scale = GlobalSetting.globalNoteScale / 0.16f;

    // Start is called before the first frame update 
    private void Awake()
    {
        scale = GlobalSetting.globalNoteScale / GetFactor();
    }

    void Start()
    {
        transform.localScale = new Vector3(scale, scale, scale);
        //sr.sortingLayerName = "AboveNotes";
        //sr.sortingOrder = 1;
    }

    public void Enable()
    {
        RecycleCoroutine = StartCoroutine(RecycleObj());
    }

    public void PlayParticle()
    {
        EffectSystemManager.Instance.CreateParticle(cnt, color, transform.position, scale);
    }

    private IEnumerator RecycleObj()
    {
        yield return new WaitForSeconds(0.49f);
        animator.Play("NoteEffect", 0, 0f);
        HitEffectManager.GetInstance().RecycleObj(this);
    }

    public void ForceRecycle()
    {
        StopCoroutine(RecycleCoroutine);
        animator.Play("NoteEffect", 0, 0f);
        HitEffectManager.GetInstance().RecycleObj(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetFactor()
    {
        return GlobalSetting.Skin switch {
            Skin.Official => 0.215f,
            Skin.Phira => 0.32f,
            Skin.OldOfficial => 0.16f,
            Skin.Sacabam => 0.25f,
            Skin.StarPinkXz => 0.16f,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}