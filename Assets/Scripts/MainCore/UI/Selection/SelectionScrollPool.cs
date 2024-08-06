using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MainCore.Common;
using MainCore.UI.Utils;
using UnityEngine;

namespace MainCore.UI.Selection
{
    public class SelectionScrollPool : MonoSingleton<SelectionScrollPool>
    {
        [SerializeField] private PullableScrollRect scrollRect;
        [SerializeField] private RectTransform contentTransform;
        [SerializeField] private float cellSize;
        [SerializeField] private int firstItemIndex;
        [SerializeField] private float animationDuration;
        [Range(1, 20)] [SerializeField] private int xCount;

        private int _currentStartLine = 0;
        private int MaximumLineNo => (int) Math.Ceiling(contentTransform.sizeDelta.y / cellSize);

        private void Start()
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            //OnScrollValueChanged(Vector2.zero);
        }

        public async void Warmup()
        {
            await UniTask.Delay(200);
            var maxIndex = Math.Min(firstItemIndex + 6 * xCount, contentTransform.childCount);
            for (var i = firstItemIndex; i < maxIndex; i++)
            {
                contentTransform.GetChild(i).GetComponent<CanvasGroup>().DOFade(1, animationDuration);
                contentTransform.GetChild(i).GetComponent<SelectionInfoBinder>().NotifyUpdate(false);
            }
        }
        
        private void OnScrollValueChanged(Vector2 pos)
        {
            var startLine = Math.Clamp((int) Math.Ceiling(contentTransform.anchoredPosition.y / cellSize) - 2, 0, MaximumLineNo);
            if (startLine == _currentStartLine) return;
            var startIndex = firstItemIndex + startLine * xCount;
            
            var prevStartLine = _currentStartLine;
            var prevStartIndex = firstItemIndex + prevStartLine * xCount;
            var prevEndLine = Math.Clamp(prevStartLine + 5, 0, MaximumLineNo);
            var prevEndIndex = Math.Min(firstItemIndex + prevEndLine * xCount + xCount, contentTransform.childCount);
            _currentStartLine = startLine;
            var endLine = Math.Clamp(startLine + 5, 0, MaximumLineNo);
            var endIndex = Math.Min(firstItemIndex + endLine * xCount + xCount, contentTransform.childCount);

            if (prevStartLine < startLine)
            {
                for (var i = prevStartIndex; i < startIndex; i++)
                {
                    contentTransform.GetChild(i).GetComponent<CanvasGroup>().DOFade(0, animationDuration);
                    contentTransform.GetChild(i).GetComponent<SelectionInfoBinder>().Unload();
                }
                for (var i = prevEndIndex; i < endIndex; i++)
                {
                    contentTransform.GetChild(i).GetComponent<CanvasGroup>().DOFade(1, animationDuration);
                    contentTransform.GetChild(i).GetComponent<SelectionInfoBinder>().NotifyUpdate(false);
                }
            }
            else
            {
                for (var i = startIndex; i < prevStartIndex; i++)
                {
                    contentTransform.GetChild(i).GetComponent<CanvasGroup>().DOFade(1, animationDuration);
                    contentTransform.GetChild(i).GetComponent<SelectionInfoBinder>().NotifyUpdate(false);
                }
                for (var i = endIndex; i < prevEndIndex; i++)
                {
                    contentTransform.GetChild(i).GetComponent<CanvasGroup>().DOFade(0, animationDuration);
                    contentTransform.GetChild(i).GetComponent<SelectionInfoBinder>().Unload();
                }
            }
        }
    }
}