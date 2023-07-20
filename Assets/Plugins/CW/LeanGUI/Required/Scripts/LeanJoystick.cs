using CW.Common;
using Lean.Transition;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Lean.Gui
{
    /// <summary>This component turns the current UI element into a joystick.</summary>
    [RequireComponent(typeof(RectTransform))]
    [HelpURL(LeanGui.HelpUrlPrefix + "LeanJoystick")]
    [AddComponentMenu(LeanGui.ComponentMenuPrefix + "Joystick")]
    public class LeanJoystick : LeanSelectable, IPointerDownHandler, IPointerUpHandler
    {
        public enum ShapeType
        {
            Box,
            Circle,
            CircleEdge
        }

        [SerializeField] private ShapeType shape;
        [SerializeField] private Vector2 size = new Vector2(25.0f, 25.0f);
        [SerializeField] private float radius = 25.0f;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float damping = 5.0f;
        [SerializeField] private bool snapWhileHeld = true;
        [SerializeField] private bool relativeToOrigin;
        [SerializeField] private bool centerOnRelease = true;
        [SerializeField] private RectTransform relativeRect;
        [SerializeField] private Vector2 scaledValue;
        [SerializeField] private LeanPlayer downTransitions;
        [SerializeField] private LeanPlayer upTransitions;
        [SerializeField] private UnityEvent onDown;
        [SerializeField] private Vector2Event onSet;
        [SerializeField] private UnityEvent onUp;

        [System.NonSerialized] private RectTransform cachedRectTransform;

        [System.NonSerialized] private bool cachedRectTransformSet;

        [System.NonSerialized] private Vector2 lastValue;

        [System.NonSerialized] private bool lastValueSet;

        [System.NonSerialized] private Vector2 nextValue;

        [System.NonSerialized] private bool nextValueSet;

        [System.NonSerialized] private Vector2 offset;

        [System.NonSerialized] private PointerEventData pointer;

        /// <summary>This allows you to control the shape of the joystick movement.
        /// Box = -Size to +Size on x and y axes.
        /// Circle = Within Radius on x and y axes.</summary>
        public ShapeType Shape
        {
            set { shape = value; }
            get { return shape; }
        }

        /// <summary>This allows you to control the size of the joystick handle movement across the x and y axes.</summary>
        public Vector2 Size
        {
            set { size = value; }
            get { return size; }
        }

        /// <summary>The allows you to control the maximum distance the joystick handle can move across the x and y axes.</summary>
        public float Radius
        {
            set { radius = value; }
            get { return radius; }
        }

        /// <summary>If you want to see where the joystick handle is, then make a child UI element, and set its RectTransform here.</summary>
        public RectTransform Handle
        {
            set { handle = value; }
            get { return handle; }
        }

        /// <summary>This allows you to control how quickly the joystick handle position updates
        /// -1 = instant.
        /// NOTE: This is for visual purposes only, the actual joystick <b>ScaledValue</b> will instantly update.</summary>
        public float Damping
        {
            set { damping = value; }
            get { return damping; }
        }

        /// <summary>If you only want the smooth <b>Dampening</b> to apply when the joystick is returning to the center, then you can enable this.</summary>
        public bool SnapWhileHeld
        {
            set { snapWhileHeld = value; }
            get { return snapWhileHeld; }
        }

        /// <summary>By default, the joystick will be placed relative to the center of this UI element.
        /// If you enable this, then the joystick will be placed relative to the place you first touch this UI element.</summary>
        public bool RelativeToOrigin
        {
            set { relativeToOrigin = value; }
            get { return relativeToOrigin; }
        }

        /// <summary>When the mouse/finger releases from the joystick, should the joystick value reset to the center, or stay where it is?</summary>
        public bool CenterOnRelease
        {
            set { centerOnRelease = value; }
            get { return centerOnRelease; }
        }

        /// <summary>If you want to show the boundary of the joystick relative to the origin, then you can make a new child GameObject graphic, and set its RectTransform here.</summary>
        public RectTransform RelativeRect
        {
            set { relativeRect = value; }
            get { return relativeRect; }
        }

        /// <summary>The -1..1 x/y position of the joystick relative to the Size or Radius.
        /// NOTE: When using a circle joystick, these values are normalized, and thus will never reach 1,1 on both axes. This prevents faster diagonal movement.</summary>
        public Vector2 ScaledValue
        {
            get { return scaledValue; }
        }

        /// <summary>This allows you to perform a transition when a finger begins touching the joystick.
        /// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
        /// For example, the <b>Graphic.color Transition (LeanGraphicColor)</b> component can be used to change the color.
        /// NOTE: Any transitions you perform here must be reverted in the <b>Up Transitions</b> setting using a matching transition component.</summary>
        public LeanPlayer DownTransitions
        {
            get
            {
                if (downTransitions == null) downTransitions = new LeanPlayer();
                return downTransitions;
            }
        }

        /// <summary>This allows you to perform a transition when a finger stops touching the joystick.
        /// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
        /// For example, the <b>Graphic.color Transition (LeanGraphicColor)</b> component can be used to change the color.</summary>
        public LeanPlayer UpTransitions
        {
            get
            {
                if (upTransitions == null) upTransitions = new LeanPlayer();
                return upTransitions;
            }
        }

        /// <summary>This allows you to perform an action when a finger begins touching the joystick.</summary>
        public UnityEvent OnDown
        {
            get
            {
                if (onDown == null) onDown = new UnityEvent();
                return onDown;
            }
        }

        /// <summary>This event is invoked each frame with the ScaledValue.</summary>
        public Vector2Event OnSet
        {
            get
            {
                if (onSet == null) onSet = new Vector2Event();
                return onSet;
            }
        }

        /// <summary>This allows you to perform an action when a finger stops touching the joystick.</summary>
        public UnityEvent OnUp
        {
            get
            {
                if (onUp == null) onUp = new UnityEvent();
                return onUp;
            }
        }

        public RectTransform CachedRectTransform
        {
            get
            {
                if (cachedRectTransformSet == false)
                {
                    cachedRectTransform = GetComponent<RectTransform>();
                    cachedRectTransformSet = true;
                }

                return cachedRectTransform;
            }
        }

        protected virtual void Update()
        {
            var value = Vector2.zero;

            if (centerOnRelease == false && lastValueSet == true)
            {
                value = lastValue;
            }

            if (nextValueSet == true)
            {
                value = nextValue;
            }

            if (pointer != null)
            {
                if (IsInteractable() == true)
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(CachedRectTransform, pointer.position,
                        pointer.pressEventCamera, out value) == true)
                    {
                        value -= offset;
                    }
                }
                else
                {
                    NullPointerNow();
                }
            }

            // Clamp value to current shape
            if (shape == ShapeType.Box)
            {
                value.x = Mathf.Clamp(value.x, -size.x, size.x);
                value.y = Mathf.Clamp(value.y, -size.y, size.y);
            }
            else if (shape == ShapeType.Circle)
            {
                if (value.sqrMagnitude > radius * radius)
                {
                    value = value.normalized * radius;
                }
            }
            else if (shape == ShapeType.CircleEdge)
            {
                value = value.normalized * radius;
            }

            // Update scaledValue
            if (shape == ShapeType.Box)
            {
                scaledValue.x = size.x > 0.0f ? value.x / size.x : 0.0f;
                scaledValue.y = size.y > 0.0f ? value.y / size.y : 0.0f;
            }
            else if (shape == ShapeType.Circle)
            {
                scaledValue = radius > 0.0f ? value / radius : Vector2.zero;
            }
            else if (shape == ShapeType.CircleEdge)
            {
                scaledValue = value.normalized;
            }

            // Update handle position
            if (handle != null)
            {
                var anchoredPosition = handle.anchoredPosition;
                var factor = CwHelper.DampenFactor(damping, Time.deltaTime);

                if (snapWhileHeld == true)
                {
                    if (pointer != null || nextValueSet == true)
                    {
                        factor = 1.0f;
                    }
                }

                anchoredPosition = Vector2.Lerp(anchoredPosition, value + offset, factor);

                handle.anchoredPosition = anchoredPosition;
            }

            lastValue = value;
            lastValueSet = true;
            nextValue = value;
            nextValueSet = false;

            // Update relative position
            if (relativeToOrigin == true && relativeRect != null)
            {
                relativeRect.anchoredPosition = offset;
            }

            // Fire event
            if (onSet != null)
            {
                onSet.Invoke(scaledValue);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (pointer == null && IsInteractable() == true)
            {
                pointer = eventData;

                var origin = pointer.position;

                if (relativeToOrigin == false)
                {
                    var worldPoint = transform.TransformPoint(CachedRectTransform.rect.center);

                    origin = RectTransformUtility.WorldToScreenPoint(pointer.pressEventCamera, worldPoint);
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(CachedRectTransform, origin,
                    pointer.pressEventCamera, out offset);

                if (downTransitions != null)
                {
                    downTransitions.Begin();
                }

                if (onDown != null) onDown.Invoke();
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (pointer == eventData)
            {
                NullPointerNow();
            }
        }

        public void OverrideNextValue(Vector2 value)
        {
            nextValue = value;
            nextValueSet = true;
        }

        public void IncrementNextValue(Vector2 delta)
        {
            nextValue += delta;
            nextValueSet = true;
        }

        private void NullPointerNow()
        {
            pointer = null;

            if (upTransitions != null)
            {
                upTransitions.Begin();
            }

            if (onUp != null) onUp.Invoke();
        }

        [System.Serializable]
        public class Vector2Event : UnityEvent<Vector2>
        {
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
    using TARGET = LeanJoystick;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(LeanJoystick))]
    public class LeanJoystick_Editor : LeanSelectable_Editor
    {
        protected override void DrawSelectableSettings()
        {
            TARGET tgt;
            TARGET[] tgts;
            GetTargets(out tgt, out tgts);

            base.DrawSelectableSettings();

            Separator();

            Draw("shape",
                "This allows you to control the shape of the joystick movement.\n\nBox = -Size to +Size on x and y axes.\n\nCircle = Within Radius on x and y axes.");
            if (Any(tgts, t => t.Shape == LeanJoystick.ShapeType.Box))
            {
                Draw("size",
                    "This allows you to control the size of the joystick handle movement across the x and y axes.");
            }

            if (Any(tgts, t => t.Shape == LeanJoystick.ShapeType.Circle))
            {
                Draw("radius",
                    "The allows you to control the maximum distance the joystick handle can move across the x and y axes.");
            }

            if (Any(tgts, t => t.Shape == LeanJoystick.ShapeType.CircleEdge))
            {
                Draw("radius",
                    "The allows you to control the distance the joystick handle can move across the x and y axes.");
            }

            Draw("scaledValue",
                "The -1..1 x/y position of the joystick relative to the Size or Radius.\n\nNOTE: When using a circle joystick, these values are normalized, and thus will never reach 1,1 on both axes. This prevents faster diagonal movement.");

            Separator();

            Draw("relativeToOrigin",
                "By default, the joystick will be placed relative to the center of this UI element.\n\nIf you enable this, then the joystick will be placed relative to the place you first touch this UI element.");
            Draw("centerOnRelease",
                "When the mouse/finger releases from the joystick, should the joystick value reset to the center, or stay where it is?");

            if (Any(tgts, t => t.RelativeToOrigin == true))
            {
                BeginIndent();
                Draw("relativeRect",
                    "If you want to show the boundary of the joystick relative to the origin, then you can make a new child GameObject graphic, and set its RectTransform here.");
                EndIndent();
            }

            Separator();

            Draw("handle",
                "If you want to see where the joystick handle is, then make a child UI element, and set its RectTransform here.");

            if (Any(tgts, t => t.Handle != null))
            {
                BeginIndent();
                Draw("damping",
                    "This allows you to control how quickly the joystick handle position updates\n\n-1 = instant.\n\nNOTE: This is for visual purposes only, the actual joystick <b>ScaledValue</b> will instantly update.");
                Draw("snapWhileHeld",
                    "If you only want the smooth Dampening to apply when the joystick is returning to the center, then you can enable this.");
                EndIndent();
            }
        }

        protected override void DrawSelectableTransitions(bool showUnusedEvents)
        {
            TARGET tgt;
            TARGET[] tgts;
            GetTargets(out tgt, out tgts);

            if (showUnusedEvents == true || Any(tgts, t => t.DownTransitions.IsUsed == true))
            {
                Draw("downTransitions",
                    "This allows you to perform a transition when a finger begins touching the joystick.\n\nYou can create a new transition GameObject by right clicking the transition name, and selecting Create.\n\nFor example, the Graphic.color Transition (LeanGraphicColor) component can be used to change the color.\n\nNOTE: Any transitions you perform here must be reverted in the Up Transitions setting using a matching transition component.");
            }

            if (showUnusedEvents == true || Any(tgts, t => t.UpTransitions.IsUsed == true))
            {
                Draw("upTransitions",
                    "This allows you to perform a transition when a finger stops touching the joystick.\n\nYou can create a new transition GameObject by right clicking the transition name, and selecting Create.\n\nFor example, the Graphic.color Transition (LeanGraphicColor) component can be used to change the color.");
            }

            base.DrawSelectableTransitions(showUnusedEvents);
        }

        protected override void DrawSelectableEvents(bool showUnusedEvents)
        {
            TARGET tgt;
            TARGET[] tgts;
            GetTargets(out tgt, out tgts);

            if (showUnusedEvents == true || Any(tgts, t => t.OnDown.GetPersistentEventCount() > 0))
            {
                Draw("onDown");
            }

            if (showUnusedEvents == true || Any(tgts, t => t.OnSet.GetPersistentEventCount() > 0))
            {
                Draw("onSet");
            }

            if (showUnusedEvents == true || Any(tgts, t => t.OnUp.GetPersistentEventCount() > 0))
            {
                Draw("onUp");
            }

            base.DrawSelectableEvents(showUnusedEvents);
        }
    }
}
#endif