using Cysharp.Threading.Tasks;
using DG.Tweening;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class DatuPreviewFadeInOut : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image background;
    [SerializeField] private float originalBgAlpha, originalCharacterAlpha;
    private bool isFading;

    private void Awake()
    {
        spriteRenderer.SetAlpha(0f);
        background.SetAlpha(0f);
        background.raycastTarget = false;
        isFading = false;
    }

    public async void FadeIn(float time, float delta = 0f, Ease ease = Ease.OutSine)
    {
        if (isFading) return;
        isFading = true;
        background.raycastTarget = true;
        background.DOFade(originalBgAlpha, time).SetEase(ease);
        if (delta > 0f) await new WaitForSeconds(delta);
        spriteRenderer.DOFade(originalCharacterAlpha, time + delta).SetEase(ease);
        await new WaitForSeconds(time);
        isFading = false;
    }
    
    public async void FadeOut(float time, float delta = 0f, Ease ease = Ease.OutSine)
    {
        if (isFading) return;
        isFading = true;
        spriteRenderer.DOFade(0f, time + delta).SetEase(ease);
        if (delta > 0f) await new WaitForSeconds(delta);
        background.DOFade(0f, time).SetEase(ease);
        await new WaitForSeconds(time);
        background.raycastTarget = false;
        isFading = false;
    }
}
