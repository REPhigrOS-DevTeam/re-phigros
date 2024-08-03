using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace MainCore.UI.Utils
{
    public class PullableScrollRect : ScrollRect
    {
         public class RefreshControlEvent : UnityEvent {}

        public float PullDistanceRequiredRefresh { get; set; } = 150f;
        public readonly RefreshControlEvent OnRefresh = new RefreshControlEvent();
        
        private float _initialPosition;
        private bool _isDragging = false;
        private bool _reachedRequiredDistance = false;
        private bool _waitingForReset = false;

        protected override void Start()
        {
            base.Start();
            _initialPosition = content.anchoredPosition.y;
        }

        private void Update()
        {
            if (_waitingForReset)
            {
                if (Mathf.Approximately(content.anchoredPosition.y, _initialPosition))
                {
                    _waitingForReset = false;
                }

                return;
            }
            
            if (_isDragging)
            {
                if (_initialPosition - content.anchoredPosition.y >= PullDistanceRequiredRefresh)
                {
                    _reachedRequiredDistance = true;
                }
                return;
            }

            if (!_reachedRequiredDistance)
            {
                return;
            }
            OnRefresh?.Invoke();
            _reachedRequiredDistance = false;
        }

        public override void OnBeginDrag(PointerEventData p)
        {
            base.OnBeginDrag(p);
            _isDragging = true;
        }
        
        public override void OnEndDrag(PointerEventData p)
        {
            base.OnEndDrag(p);
            _isDragging = false;
        }
    }
}
