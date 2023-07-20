using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainCore.UI
{
    [RequireComponent(typeof(Image))]
    public class DoubleTapButton : MonoBehaviour, IPointerDownHandler
    {
        private bool clicked = false;
        private Image image;

        public UnityEvent OnDoubleTap { get; } = new UnityEvent();

        void Start()
        {
            image = GetComponent<Image>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StartCoroutine(CheckClick());
        }

        IEnumerator CheckClick()
        {
            if (!clicked)
            {
                clicked = true;
                image.DOFade(.4f, .5f);
                yield return new WaitForSeconds(1f);
                if (clicked)
                {
                    image.DOFade(1f, .5f);
                    clicked = false;
                }
            }
            else
            {
                clicked = false;
                image.DOComplete();
                image.DOFade(1f, 0f);
                OnDoubleTap.Invoke();
            }
        }
    }
}