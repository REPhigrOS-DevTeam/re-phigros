using DG.Tweening;
using UnityEngine;

namespace MainCore.SceneTransitAnimation
{
    public class LoadingAnimation : Common.SceneTransitAnimation
    {
        [SerializeField] private RectTransform leftTransform, rightTransform;
        [SerializeField] private RectTransform loadingIndicatorTransform;

        private Sequence _seq;

        public override void Init()
        {
            leftTransform.anchoredPosition = new Vector2(-2000f, 0);
            rightTransform.anchoredPosition = new Vector2(2000f, 0);
            //centerTransform.GetComponent<CanvasGroup>().alpha = 0;
            loadingIndicatorTransform.GetComponent<CanvasGroup>().alpha = 0;
            loadingIndicatorTransform.anchoredPosition = new Vector2(-380, -200);
        }

        public override int Enter()
        {
            _seq = DOTween.Sequence();
            _seq.Append(leftTransform.DOAnchorPosX(0, .6f).SetEase(Ease.InQuad))
                .Join(rightTransform.DOAnchorPosX(0, .6f).SetEase(Ease.InQuad))
                .Insert(.3f, loadingIndicatorTransform.GetComponent<CanvasGroup>().DOFade(1, .8f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo))
                //.Join(centerTransform.GetComponent<CanvasGroup>().DOFade(1, .5f).SetEase(Ease.OutQuad))
                .Join(loadingIndicatorTransform.DOAnchorPosY(130, .5f));
            _seq.Play();
            
            return 1000;
        }

        public override int Quit()
        {
            //_seq.Complete();
            
            _seq = DOTween.Sequence();
            _seq.Append(leftTransform.DOAnchorPosX(-2000, .6f).SetEase(Ease.OutQuad))
                .Join(rightTransform.DOAnchorPosX(2000, .6f).SetEase(Ease.OutQuad))
                //.Join(centerTransform.GetComponent<CanvasGroup>().DOFade(0, .5f).SetEase(Ease.OutQuad))
                .Insert(.3f, loadingIndicatorTransform.DOAnchorPosY(-200, .5f));
            _seq.Play();

            return 1000;
        }
        
        
    }
}