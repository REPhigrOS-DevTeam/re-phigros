using CW.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Lean.Gui
{
    /// <summary>This component will automatically snap <b>RectTransform.anchoredPosition</b> to the specified interval.</summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [HelpURL(LeanGui.HelpUrlPrefix + "LeanSnap")]
    [AddComponentMenu(LeanGui.ComponentMenuPrefix + "Snap")]
    public class LeanSnap : MonoBehaviour
    {
        [SerializeField] private bool horizontal;
        [SerializeField] private float horizontalOffset;
        [SerializeField] private float horizontalIntervalPixel = 10.0f;
        [SerializeField] private float horizontalIntervalRect;
        [SerializeField] private float horizontalIntervalParent;
        [SerializeField] private float horizontalSpeed = -1.0f;
        [SerializeField] private bool vertical;
        [SerializeField] private float verticalOffset;
        [SerializeField] private float verticalIntervalPixel = 10.0f;
        [SerializeField] private float verticalIntervalRect;
        [SerializeField] private float verticalIntervalParent;
        [SerializeField] private float verticalSpeed = -1.0f;
        [SerializeField] private LeanDrag disableWith;
        [SerializeField] private Vector2Int position;
        [SerializeField] private Vector2IntEvent onPositionChanged;

        [System.NonSerialized] private RectTransform cachedRectTransform;

        /// <summary>Snap horizontally?</summary>
        public bool Horizontal
        {
            set { horizontal = value; }
            get { return horizontal; }
        }

        /// <summary>The snap points will be offset by this many pixels.</summary>
        public float HorizontalOffset
        {
            set { horizontalOffset = value; }
            get { return horizontalOffset; }
        }

        /// <summary>The spacing between each snap point in pixels.</summary>
        public float HorizontalIntervalPixel
        {
            set { horizontalIntervalPixel = value; }
            get { return horizontalIntervalPixel; }
        }

        /// <summary>The spacing between each snap point in 0..1 percent of the current RectTransform size.</summary>
        public float HorizontalIntervalRect
        {
            set { horizontalIntervalRect = value; }
            get { return horizontalIntervalRect; }
        }

        /// <summary>The spacing between each snap point in 0..1 percent of the parent.</summary>
        public float HorizontalIntervalParent
        {
            set { horizontalIntervalParent = value; }
            get { return horizontalIntervalParent; }
        }

        /// <summary>The snap speed.
        /// -1 = Instant.
        /// 1 = Slow.
        /// 10 = Fast.</summary>
        public float HorizontalSpeed
        {
            set { horizontalSpeed = value; }
            get { return horizontalSpeed; }
        }

        /// <summary>Snap vertically?</summary>
        public bool Vertical
        {
            set { vertical = value; }
            get { return vertical; }
        }

        /// <summary>The snap points will be offset by this many pixels.</summary>
        public float VerticalOffset
        {
            set { verticalOffset = value; }
            get { return verticalOffset; }
        }

        /// <summary>The spacing between each snap point in pixels.</summary>
        public float VerticalIntervalPixel
        {
            set { verticalIntervalPixel = value; }
            get { return verticalIntervalPixel; }
        }

        /// <summary>The spacing between each snap point in 0..1 percent of the current RectTransform size.</summary>
        public float VerticalIntervalRect
        {
            set { verticalIntervalRect = value; }
            get { return verticalIntervalRect; }
        }

        /// <summary>The spacing between each snap point in 0..1 percent of the parent.</summary>
        public float VerticalIntervalParent
        {
            set { verticalIntervalParent = value; }
            get { return verticalIntervalParent; }
        }

        /// <summary>The snap speed.
        /// -1 = Instant.
        /// 1 = Slow.
        /// 10 = Fast.</summary>
        public float VerticalSpeed
        {
            set { verticalSpeed = value; }
            get { return verticalSpeed; }
        }

        /// <summary>To prevent UI element dragging from conflicting with snapping, you can specify the drag component here.</summary>
        public LeanDrag DisableWith
        {
            set { disableWith = value; }
            get { return disableWith; }
        }

        /// <summary>This tells you the snap position as integers.</summary>
        public Vector2Int Position
        {
            get { return position; }
        }

        /// <summary>This event will be invoked when the snap position changes.</summary>
        public Vector2IntEvent OnPositionChanged
        {
            get
            {
                if (onPositionChanged == null) onPositionChanged = new Vector2IntEvent();
                return onPositionChanged;
            }
        }

        private Vector2 ParentSize
        {
            get
            {
                var parent = cachedRectTransform.parent as RectTransform;

                return parent != null ? parent.rect.size : Vector2.zero;
            }
        }

        protected virtual void LateUpdate()
        {
            if (disableWith != null && disableWith.Dragging == true)
            {
                return;
            }

            var anchoredPosition = cachedRectTransform.anchoredPosition;
            var rect = cachedRectTransform.rect;
            var parentSize = ParentSize;
            var intervalX = horizontalIntervalPixel + horizontalIntervalRect * rect.width +
                            horizontalIntervalParent * parentSize.x;
            var intervalY = verticalIntervalPixel + verticalIntervalRect * rect.width +
                            verticalIntervalParent * parentSize.y;
            var oldPosition = position;

            if (intervalX != 0.0f)
            {
                position.x = Mathf.RoundToInt((anchoredPosition.x - horizontalOffset) / intervalX);
            }

            if (intervalY != 0.0f)
            {
                position.y = Mathf.RoundToInt((anchoredPosition.y - verticalOffset) / intervalY);
            }

            if (horizontal == true)
            {
                var target = position.x * intervalX + horizontalOffset;
                var factor = CwHelper.DampenFactor(horizontalSpeed, Time.deltaTime);

                anchoredPosition.x = Mathf.Lerp(anchoredPosition.x, target, factor);
            }

            if (vertical == true)
            {
                var target = position.y * intervalY + verticalOffset;
                var factor = CwHelper.DampenFactor(verticalSpeed, Time.deltaTime);

                anchoredPosition.y = Mathf.Lerp(anchoredPosition.y, target, factor);
            }

            cachedRectTransform.anchoredPosition = anchoredPosition;

            if (position != oldPosition)
            {
                if (onPositionChanged != null)
                {
                    onPositionChanged.Invoke(position);
                }
            }
        }

        protected virtual void OnEnable()
        {
            cachedRectTransform = GetComponent<RectTransform>();
        }

        [System.Serializable]
        public class Vector2IntEvent : UnityEvent<Vector2Int>
        {
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
    using TARGET = LeanSnap;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TARGET))]
    public class LeanSnap_Editor : CwEditor
    {
        protected override void OnInspector()
        {
            TARGET tgt;
            TARGET[] tgts;
            GetTargets(out tgt, out tgts);

            Draw("horizontal", "Snap horizontally?");

            if (Any(tgts, t => t.Horizontal == true))
            {
                BeginIndent();
                Draw("horizontalOffset", "The snap points will be offset by this many pixels.", "Offset");
                BeginError(Any(tgts,
                    t => t.HorizontalIntervalPixel == 0.0f && t.HorizontalIntervalRect == 0.0f &&
                         t.HorizontalIntervalParent == 0.0f));
                Draw("horizontalIntervalPixel", "The spacing between each snap point in pixels.", "Interval Pixel");
                Draw("horizontalIntervalRect",
                    "The spacing between each snap point in 0..1 percent of the current RectTransform size.",
                    "Interval Rect");
                Draw("horizontalIntervalParent", "The spacing between each snap point in 0..1 percent of the parent.",
                    "Interval Parent");
                EndError();
                Draw("horizontalSpeed", "The snap speed.\n\n-1 = Instant.\n\n1 = Slow.\n\n10 = Fast.", "Speed");
                EndIndent();
            }

            Separator();

            Draw("vertical", "Snap vertically?");

            if (Any(tgts, t => t.Vertical == true))
            {
                BeginIndent();
                Draw("verticalOffset", "The snap points will be offset by this many pixels.", "Offset");
                BeginError(Any(tgts,
                    t => t.VerticalIntervalPixel == 0.0f && t.VerticalIntervalRect == 0.0f &&
                         t.VerticalIntervalParent == 0.0f));
                Draw("verticalIntervalPixel", "The spacing between each snap point in pixels.", "Interval Pixel");
                Draw("verticalIntervalRect",
                    "The spacing between each snap point in 0..1 percent of the current RectTransform size.",
                    "Interval Rect");
                Draw("verticalIntervalParent", "The spacing between each snap point in 0..1 percent of the parent.",
                    "Interval Parent");
                EndError();
                Draw("verticalSpeed", "The snap speed.\n\n-1 = Instant.\n\n1 = Slow.\n\n10 = Fast.", "Speed");
                EndIndent();
            }

            Separator();

            Draw("disableWith",
                "To prevent UI element dragging from conflicting with snapping, you can specify the drag component here.");
            BeginDisabled(true);
            Draw("position", "This tells you the snap position as integers.");
            EndDisabled();
            Draw("onPositionChanged");
        }
    }
}
#endif