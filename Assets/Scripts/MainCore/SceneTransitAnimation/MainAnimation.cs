using DG.Tweening;
using MainCore.Utilities;
using UnityEngine;

namespace MainCore.SceneTransitAnimation
{
    public class MainAnimation : Common.SceneTransitAnimation
    {
        [SerializeField] private RectTransform mainTransform;
        [SerializeField] private RectTransform bannerTransform;
        [SerializeField] private SpriteRenderer charImage;

        public override void Init()
        {
            mainTransform.GetComponent<CanvasGroup>().alpha = 0;
            mainTransform.anchoredPosition = new Vector2(-3000, 0);
            charImage.SetAlpha(0);
        }
        
        public override int Enter()
        {
            charImage.DOFade(1, .6f).SetEase(Ease.InQuad);
            mainTransform.GetComponent<CanvasGroup>().DOFade(1, .6f).SetEase(Ease.InQuad);
            mainTransform.DOAnchorPosX(0, .6f).SetEase(Ease.InQuad);
            return 600;
        }

        public override int Quit()
        {
            charImage.DOFade(0, .6f).SetEase(Ease.OutQuad);
            mainTransform.GetComponent<CanvasGroup>().DOFade(0, .6f).SetEase(Ease.OutQuad);
            mainTransform.DOAnchorPosX(3000, .6f).SetEase(Ease.OutQuad);
            bannerTransform.DOAnchorPosX(2000, .6f).SetEase(Ease.OutQuad);
            return 600;
        }
    }
}
