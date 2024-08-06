using DG.Tweening;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.SceneTransitAnimation
{
    public class EntryAnimation : Common.SceneTransitAnimation
    {
        [SerializeField] private Image banner;
        [SerializeField] private Text text;

        public override void Init()
        {
            
        }
        
        public override int Enter()
        {
            return 0;
        }

        public override int Quit()
        {
            var time = .4f;
            text.DOFade(0, .3f).SetEase(Ease.OutSine);
            text.rectTransform.DOScale(5f, .3f).SetEase(Ease.InQuart);
            banner.GetComponent<PeriodicMover>().enabled = false;
            banner.rectTransform.DOAnchorPos(new Vector2(-400, 360), time).SetEase(Ease.OutCubic);
            banner.rectTransform.DOSizeDelta(new Vector2(736, 208), time).SetEase(Ease.OutCubic);
            return (int) (time * 1000);
        }
    }
}
