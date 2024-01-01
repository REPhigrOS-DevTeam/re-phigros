using DG.Tweening;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.SceneTransitAnimation
{
    public class EntryAnimation : Common.SceneTransitAnimation
    {
        [SerializeField] private SVGImage bannerSvg;
        [SerializeField] private Image banner, bg;
        [SerializeField] private Text text;
        public override int Enter()
        {
            return 0;
        }

        public override int Quit()
        {
            bannerSvg.DOFade(0, 0.3f).SetEase(Ease.OutSine);
            banner.DOFade(0, 0.3f).SetEase(Ease.OutSine);
            bg.DOColor(Color.white, 0.3f).SetEase(Ease.OutSine);
            text.DOFade(0, 0.3f).SetEase(Ease.OutSine);
            return 400;
        }
    }
}
