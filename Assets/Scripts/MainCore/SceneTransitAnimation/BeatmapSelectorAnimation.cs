using System.Collections.Generic;
using DG.Tweening;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.SceneTransitAnimation
{
    public class BeatmapSelectorAnimation : Common.SceneTransitAnimation
    {
        [SerializeField] private RectTransform leftTransform, rightTransform;
        [SerializeField] private Image backgroundIllustration;
        [SerializeField] private List<Image> shadows;

        private Sequence _seq;

        public override void Init()
        {
            leftTransform.anchoredPosition = new Vector2(-2000f, leftTransform.anchoredPosition.y);
            rightTransform.anchoredPosition = new Vector2(2000f, rightTransform.anchoredPosition.y);
            shadows.ForEach(x => x.SetAlpha(0f));
        }

        public override int Enter()
        {
            _seq = DOTween.Sequence();
            _seq.Append(leftTransform.DOAnchorPosX(80, .6f).SetEase(Ease.InQuad))
                .Join(rightTransform.DOAnchorPosX(-80, .6f).SetEase(Ease.InQuad));
            _seq.Play();
            shadows.ForEach(x => x.DOFade(1, .6f));
            
            return 600;
        }

        public override int Quit()
        {
            //_seq.Complete();
            
            _seq = DOTween.Sequence();
            _seq.AppendInterval(.4f)
                .Append(leftTransform.DOAnchorPosX(-2000, .6f).SetEase(Ease.OutQuad))
                .Join(rightTransform.DOAnchorPosX(2000, .6f).SetEase(Ease.OutQuad))
                .Insert(.3f, backgroundIllustration.DOFade(1, .7f));
            _seq.Play();
            shadows.ForEach(x => x.DOFade(0, .6f));

            return 1000;
        }
        
        
    }
}