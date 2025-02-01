using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class DoubleTapButton : MonoBehaviour, IPointerDownHandler
    {
        private bool clicked = false;
        [SerializeField] private Image image;

        public UnityEvent OnDoubleTap { get; } = new UnityEvent();

        public void OnPointerDown(PointerEventData eventData)
        {
            CheckClick().Forget();
        }

        async UniTaskVoid CheckClick()
        {
            if (!clicked)
            {
                clicked = true;
                image.DOFade(.4f, .5f);
                await new WaitForSeconds(1f);
                if (!clicked) return;
                image.DOFade(1f, .5f);
                clicked = false;
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