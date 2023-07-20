using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Mathf;

namespace LeTai.TrueShadow
{
    [RequireComponent(typeof(TrueShadow))]
    public class InteractiveShadow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler,
        IDeselectHandler, IPointerDownHandler, IPointerUpHandler
    {
        static readonly Color FADED_COLOR = new Color(.5f, .5f, .5f, .5f);
        public float smoothTime = .05f;

        [Tooltip("Deselect on pointer up")] public bool autoDeselect;

        [Header("Size")] public float selectedSize = 28;

        public float hoverSize = 28;
        public float clickedSize = 24;

        [Header("Distance")] public float selectedDistance = 12;

        public float hoverDistance = 12;
        public float clickedDistance = 8;

        [Header("Color")] public Color selectedColor = new Color(0, 0, 0, .25f);

        public Color hoverColor = new Color(0, 0, 0, .20f);
        public Color clickedColor = new Color(0, 0, 0, .25f);
        bool isClicked;
        bool isHovered;

        bool isSelected;
        Color normalColor;
        float normalDistance;

        float normalSize;
        Selectable selectable;

        TrueShadow shadow;
        Color targetColor;
        float targetDistance;

        float targetSize;

#if UNITY_EDITOR
        void Reset()
        {
            shadow = FindTrueShadow();
            if (shadow)
            {
                normalSize = shadow.Size;
                normalDistance = shadow.OffsetDistance;
                normalColor = shadow.Color;

                // Clicked UI remain selected, which is unwanted. Selected state is probably most useful on console
                // and keyboard nav, the later is rather hard to detect
                bool selectedIsNormal = Input.mousePresent || Input.touchSupported;
                autoDeselect = selectedIsNormal;

                hoverSize = Round(Min(normalSize * 1.75f, normalSize + 20f));
                selectedSize = selectedIsNormal ? normalSize : hoverSize;
                clickedSize = Round(Min(normalSize * 1.25f, normalSize + 15f));

                hoverDistance = Round(Min(normalDistance * 1.5f, normalDistance + 20f));
                selectedDistance = selectedIsNormal ? normalDistance : hoverDistance;
                clickedDistance = Round(Min(normalDistance * 1.25f, normalDistance + 15f));


                hoverColor = Color.Lerp(normalColor, FADED_COLOR, .15f);
                selectedColor = selectedIsNormal ? normalColor : hoverColor;
                clickedColor = Color.Lerp(normalColor, FADED_COLOR, .25f);
            }
        }
#endif


        void Update()
        {
            if (!Approximately(targetSize, shadow.Size))
            {
                shadow.Size = SmoothDamp(shadow.Size, targetSize,
                    ref currentSizeVelocity, smoothTime);
            }

            if (!Approximately(targetDistance, shadow.OffsetDistance))
            {
                shadow.OffsetDistance = SmoothDamp(shadow.OffsetDistance, targetDistance,
                    ref currentDistanceVelocity, smoothTime);
            }

            var shadowColor = shadow.Color;
            if (!Approximately(targetColor.a, shadowColor.a))
            {
                var r = SmoothDamp(shadowColor.r, targetColor.r,
                    ref currentColorRVelocity, smoothTime);
                var g = SmoothDamp(shadowColor.g, targetColor.g,
                    ref currentColorGVelocity, smoothTime);
                var b = SmoothDamp(shadowColor.b, targetColor.b,
                    ref currentColorBVelocity, smoothTime);
                var a = SmoothDamp(shadowColor.a, targetColor.a,
                    ref currentColorAVelocity, smoothTime);

                shadow.Color = new Color(r, g, b, a);
            }
        }

        void OnEnable()
        {
            shadow = FindTrueShadow();
            selectable = GetComponent<Selectable>();

            targetSize = normalSize = shadow.Size;
            targetDistance = normalDistance = shadow.OffsetDistance;
            targetColor = normalColor = shadow.Color;

            shadow.Size = targetSize = normalSize;
            shadow.OffsetDistance = targetDistance = normalDistance;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;
            OnStateChange();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isClicked = true;
            OnStateChange();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            OnStateChange();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            OnStateChange();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (autoDeselect && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);

            isClicked = false;
            OnStateChange();
        }

        public void OnSelect(BaseEventData eventData)
        {
            isSelected = true;
            OnStateChange();
        }

        TrueShadow FindTrueShadow()
        {
            var shadows = GetComponents<TrueShadow>();
            if (shadows.Length == 0) return null;

            var ishadows = GetComponents<InteractiveShadow>();

            int index = 0;
            for (; index < ishadows.Length; index++)
                if (ishadows[index] == this)
                    break;

            return shadows[index];
        }

        void OnStateChange()
        {
            if (isClicked)
            {
                targetSize = clickedSize;
                targetDistance = clickedDistance;
                targetColor = clickedColor;
            }
            else if (isSelected)
            {
                targetSize = selectedSize;
                targetDistance = selectedDistance;
                targetColor = selectedColor;
            }
            else if (isHovered)
            {
                targetSize = hoverSize;
                targetDistance = hoverDistance;
                targetColor = hoverColor;
            }
            else
            {
                targetSize = normalSize;
                targetDistance = normalDistance;
                targetColor = normalColor;
            }


            if (selectable && selectable.interactable && selectable.isActiveAndEnabled)
            {
                if (selectable.transition == Selectable.Transition.ColorTint)
                {
                    StopAllCoroutines();
                    StartCoroutine(DirtyForSeconds(selectable.colors.fadeDuration));
                }
            }
        }

        IEnumerator DirtyForSeconds(float duration)
        {
            var start = Time.time;
            while (start + duration >= Time.time)
            {
                shadow.SetTextureDirty();
                yield return null;
            }
        }


        #region AnimationState

        float currentSizeVelocity;
        float currentDistanceVelocity;
        float currentColorRVelocity;
        float currentColorGVelocity;
        float currentColorBVelocity;
        float currentColorAVelocity;

        #endregion
    }
}