using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.SceneTransitAnimation
{
    public class AdditiveSceneDefaultAnimation : Common.SceneTransitAnimation
    {
        [SerializeField] private RectTransform mainTransform;
        [SerializeField] private Image blurImg;
        [SerializeField] private bool shouldAnimateBlur = false;
        private const float InitialY = -100f;
        private static readonly int Radius = Shader.PropertyToID("_Size");
        private Material _blurMat;

        public override void Init()
        {
            throw new InvalidOperationException("Additive scenes should not execute Init animation.");
        }

        //丢弃返回值
        public override int Enter()
        {
            mainTransform.anchoredPosition = new Vector2(0, InitialY);
            mainTransform.GetComponent<CanvasGroup>().alpha = 0;
            mainTransform.GetComponent<CanvasGroup>().DOFade(1, .2f).SetEase(Ease.InQuad);
            mainTransform.DOAnchorPosY(0, .2f).SetEase(Ease.InQuad);
            if (!shouldAnimateBlur) return 300;
            
            _blurMat = Instantiate(new Material(Shader.Find("Custom/BackBlur")));
            blurImg.material = _blurMat;
            _blurMat.SetFloat(Radius, 0);
            _blurMat.DOFloat(1f, Radius, .3f);
            return 300;
        }

        public override int Quit()
        {
            mainTransform.DOAnchorPosY(InitialY, .2f).SetEase(Ease.OutQuad);
            mainTransform.GetComponent<CanvasGroup>().DOFade(0, .2f).SetEase(Ease.OutQuad);
            if (!shouldAnimateBlur) return 300;
            _blurMat.DOFloat(0f, Radius, .2f);
            return 300;
        }
    }
}