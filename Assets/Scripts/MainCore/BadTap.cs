using DG.Tweening;
using MainCore;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BadTap : MonoBehaviour
{
    public void Play(bool paintBad, Sprite badSprite)
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = badSprite;
        spriteRenderer.color = paintBad ? new Color(108f / 255f, 67f / 255f, 67f / 255f) : Color.white; // #6C4343
        spriteRenderer.DOFade(0f, 0.5f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
    }
}
