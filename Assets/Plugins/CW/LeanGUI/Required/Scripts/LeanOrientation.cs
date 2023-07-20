using CW.Common;
using Lean.Transition;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Lean.Gui
{
    /// <summary>This component allows you make a UI element based on the current device orientation.</summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [HelpURL(LeanGui.HelpUrlPrefix + "LeanOrientation")]
    [AddComponentMenu(LeanGui.ComponentMenuPrefix + "Orientation")]
    public class LeanOrientation : MonoBehaviour
    {
        [SerializeField] private bool anchoredPosition;
        [SerializeField] private Vector2 anchoredPositionL;
        [SerializeField] private Vector2 anchoredPositionP;
        [SerializeField] private bool sizeDelta;
        [SerializeField] private Vector2 sizeDeltaL;
        [SerializeField] private Vector2 sizeDeltaP;
        [SerializeField] private bool anchorMin;
        [SerializeField] private Vector2 anchorMinL;
        [SerializeField] private Vector2 anchorMinP;
        [SerializeField] private bool anchorMax;
        [SerializeField] private Vector2 anchorMaxL;
        [SerializeField] private Vector2 anchorMaxP;
        [SerializeField] private bool offsetMin;
        [SerializeField] private Vector2 offsetMinL;
        [SerializeField] private Vector2 offsetMinP;
        [SerializeField] private bool offsetMax;
        [SerializeField] private Vector2 offsetMaxL;
        [SerializeField] private Vector2 offsetMaxP;
        [SerializeField] private bool pivot;
        [SerializeField] private Vector2 pivotL;
        [SerializeField] private Vector2 pivotP;
        [SerializeField] private bool localRotation;
        [SerializeField] private Quaternion localRotationL = Quaternion.identity;
        [SerializeField] private Quaternion localRotationP = Quaternion.identity;
        [SerializeField] private bool localScale;
        [SerializeField] private Vector3 localScaleL;
        [SerializeField] private Vector3 localScaleP;
        [SerializeField] private LeanPlayer landscapeTransitions;
        [SerializeField] private LeanPlayer portraitTransitions;
        [SerializeField] private UnityEvent onLandscape;
        [SerializeField] private UnityEvent onPortrait;

        [SerializeField] private bool landscape;

        [System.NonSerialized] private RectTransform cachedRectTransform;

        [System.NonSerialized] private bool cachedRectTransformSet;

        [System.NonSerialized] private bool dirty;

        /// <summary>Should the RectTransform.anchoredPosition setting change based on device orientation?</summary>
        public bool AnchoredPosition
        {
            set
            {
                anchoredPosition = value;
                dirty = true;
            }
            get { return anchoredPosition; }
        }

        /// <summary>The RectTransform.anchoredPosition value when in landscape mode.</summary>
        public Vector2 AnchoredPositionL
        {
            set
            {
                anchoredPositionL = value;
                dirty = true;
            }
            get { return anchoredPositionL; }
        }

        /// <summary>The RectTransform.anchoredPosition value when in portrait mode.</summary>
        public Vector2 AnchoredPositionP
        {
            set
            {
                anchoredPositionP = value;
                dirty = true;
            }
            get { return anchoredPositionP; }
        }

        /// <summary>Should the RectTransform.sizeDelta setting change based on device orientation?</summary>
        public bool SizeDelta
        {
            set
            {
                sizeDelta = value;
                dirty = true;
            }
            get { return sizeDelta; }
        }

        /// <summary>The RectTransform.sizeDelta value when in landscape mode.</summary>
        public Vector2 SizeDeltaL
        {
            set
            {
                sizeDeltaL = value;
                dirty = true;
            }
            get { return sizeDeltaL; }
        }

        /// <summary>The RectTransform.sizeDelta value when in portrait mode.</summary>
        public Vector2 SizeDeltaP
        {
            set
            {
                sizeDeltaP = value;
                dirty = true;
            }
            get { return sizeDeltaP; }
        }

        /// <summary>Should the RectTransform.anchorMin setting change based on device orientation?</summary>
        public bool AnchorMin
        {
            set
            {
                anchorMin = value;
                dirty = true;
            }
            get { return anchorMin; }
        }

        /// <summary>The RectTransform.anchorMin value when in landscape mode.</summary>
        public Vector2 AnchorMinL
        {
            set
            {
                anchorMinL = value;
                dirty = true;
            }
            get { return anchorMinL; }
        }

        /// <summary>The RectTransform.anchorMin value when in portrait mode.</summary>
        public Vector2 AnchorMinP
        {
            set
            {
                anchorMinP = value;
                dirty = true;
            }
            get { return anchorMinP; }
        }

        /// <summary>Should the RectTransform.anchorMax setting change based on device orientation?</summary>
        public bool AnchorMax
        {
            set
            {
                anchorMax = value;
                dirty = true;
            }
            get { return anchorMax; }
        }

        /// <summary>The RectTransform.anchorMax value when in landscape mode.</summary>
        public Vector2 AnchorMaxL
        {
            set
            {
                anchorMaxL = value;
                dirty = true;
            }
            get { return anchorMaxL; }
        }

        /// <summary>The RectTransform.anchorMax value when in portrait mode.</summary>
        public Vector2 AnchorMaxP
        {
            set
            {
                anchorMaxP = value;
                dirty = true;
            }
            get { return anchorMaxP; }
        }

        /// <summary>Should the RectTransform.offsetMin setting change based on device orientation?</summary>
        public bool OffsetMin
        {
            set
            {
                offsetMin = value;
                dirty = true;
            }
            get { return offsetMin; }
        }

        /// <summary>The RectTransform.offsetMin value when in landscape mode.</summary>
        public Vector2 OffsetMinL
        {
            set
            {
                offsetMinL = value;
                dirty = true;
            }
            get { return offsetMinL; }
        }

        /// <summary>The RectTransform.offsetMin value when in portrait mode.</summary>
        public Vector2 OffsetMinP
        {
            set
            {
                offsetMinP = value;
                dirty = true;
            }
            get { return offsetMinP; }
        }

        /// <summary>Should the RectTransform.offsetMax setting change based on device orientation?</summary>
        public bool OffsetMax
        {
            set
            {
                offsetMax = value;
                dirty = true;
            }
            get { return offsetMax; }
        }

        /// <summary>The RectTransform.offsetMax value when in landscape mode.</summary>
        public Vector2 OffsetMaxL
        {
            set
            {
                offsetMaxL = value;
                dirty = true;
            }
            get { return offsetMaxL; }
        }

        /// <summary>The RectTransform.offsetMax value when in portrait mode.</summary>
        public Vector2 OffsetMaxP
        {
            set
            {
                offsetMaxP = value;
                dirty = true;
            }
            get { return offsetMaxP; }
        }

        /// <summary>Should the RectTransform.pivot setting change based on device orientation?</summary>
        public bool Pivot
        {
            set
            {
                pivot = value;
                dirty = true;
            }
            get { return pivot; }
        }

        /// <summary>The RectTransform.pivot value when in landscape mode.</summary>
        public Vector2 PivotL
        {
            set
            {
                pivotL = value;
                dirty = true;
            }
            get { return pivotL; }
        }

        /// <summary>The RectTransform.pivot value when in portrait mode.</summary>
        public Vector2 PivotP
        {
            set
            {
                pivotP = value;
                dirty = true;
            }
            get { return pivotP; }
        }

        /// <summary>Should the RectTransform.localRotation setting change based on device orientation?</summary>
        public bool LocalRotation
        {
            set
            {
                localRotation = value;
                dirty = true;
            }
            get { return localRotation; }
        }

        /// <summary>The RectTransform.localRotation value when in landscape mode.</summary>
        public Quaternion LocalRotationL
        {
            set
            {
                localRotationL = value;
                dirty = true;
            }
            get { return localRotationL; }
        }

        /// <summary>The RectTransform.localRotation value when in portrait mode.</summary>
        public Quaternion LocalRotationP
        {
            set
            {
                localRotationP = value;
                dirty = true;
            }
            get { return localRotationP; }
        }

        /// <summary>Should the RectTransform.localScale setting change based on device orientation?</summary>
        public bool LocalScale
        {
            set
            {
                localScale = value;
                dirty = true;
            }
            get { return localScale; }
        }

        /// <summary>The RectTransform.localScale value when in landscape mode.</summary>
        public Vector3 LocalScaleL
        {
            set
            {
                localScaleL = value;
                dirty = true;
            }
            get { return localScaleL; }
        }

        /// <summary>The RectTransform.localScale value when in portrait mode.</summary>
        public Vector3 LocalScaleP
        {
            set
            {
                localScaleP = value;
                dirty = true;
            }
            get { return localScaleP; }
        }

        /// <summary>This allows you to perform a transition when the orientation changes to landscape.
        /// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
        /// For example, the <b>Graphic.color Transition (LeanGraphicColor)</b> component can be used to change the color.
        /// NOTE: Any transitions you perform here must be reverted in the <b>Portrait Transitions</b> setting using a matching transition component.</summary>
        public LeanPlayer LandscapeTransitions
        {
            get
            {
                if (landscapeTransitions == null) landscapeTransitions = new LeanPlayer();
                return landscapeTransitions;
            }
        }

        /// <summary>This allows you to perform a transition when the orientation changes to portrait.
        /// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
        /// For example, the <b>Graphic.color Transition (LeanGraphicColor)</b> component can be used to change the color.
        /// NOTE: Any transitions you perform here must be reverted in the <b>Landscape Transitions</b> setting using a matching transition component.</summary>
        public LeanPlayer PortraitTransitions
        {
            get
            {
                if (portraitTransitions == null) portraitTransitions = new LeanPlayer();
                return portraitTransitions;
            }
        }

        /// <summary>This event will be invoked when the orientation changes to landscape.</summary>
        public UnityEvent OnLandscape
        {
            get
            {
                if (onLandscape == null) onLandscape = new UnityEvent();
                return onLandscape;
            }
        }

        /// <summary>This event will be invoked when the orientation changes to portrait.</summary>
        public UnityEvent OnPortrait
        {
            get
            {
                if (onPortrait == null) onPortrait = new UnityEvent();
                return onPortrait;
            }
        }

        protected virtual void Update()
        {
            if (dirty == true || landscape != Screen.width > Screen.height)
            {
                UpdateOrientation();
            }
        }

        /// <summary>This will copy the current RectTransform settings to the <b>Landscape</b> settings.</summary>
        [ContextMenu("Copy To Landscape")]
        public void CopyToLandscape()
        {
            UpdateCachedRectTransform();

            anchoredPositionL = cachedRectTransform.anchoredPosition;
            sizeDeltaL = cachedRectTransform.sizeDelta;
            anchorMinL = cachedRectTransform.anchorMin;
            anchorMaxL = cachedRectTransform.anchorMax;
            offsetMinL = cachedRectTransform.offsetMin;
            offsetMaxL = cachedRectTransform.offsetMax;
            pivotL = cachedRectTransform.pivot;
            localRotationL = cachedRectTransform.localRotation;
            localScaleL = cachedRectTransform.localScale;
        }

        /// <summary>This will copy the current RectTransform settings to the <b>Portrait</b> settings.</summary>
        [ContextMenu("Copy To Portrait")]
        public void CopyToPortrait()
        {
            UpdateCachedRectTransform();

            anchoredPositionP = cachedRectTransform.anchoredPosition;
            sizeDeltaP = cachedRectTransform.sizeDelta;
            anchorMinP = cachedRectTransform.anchorMin;
            anchorMaxP = cachedRectTransform.anchorMax;
            offsetMinP = cachedRectTransform.offsetMin;
            offsetMaxP = cachedRectTransform.offsetMax;
            pivotP = cachedRectTransform.pivot;
            localRotationP = cachedRectTransform.localRotation;
            localScaleP = cachedRectTransform.localScale;
        }

        /// <summary>This forces the orientation to update now.
        /// NOTE: This will call OnLandscape/OnPortrait again.</summary>
        [ContextMenu("Update Orientation")]
        public void UpdateOrientation()
        {
            dirty = false;
            landscape = Screen.width > Screen.height;

            UpdateCachedRectTransform();

            if (anchoredPosition == true)
                cachedRectTransform.anchoredPosition = landscape == true ? anchoredPositionL : anchoredPositionP;
            if (sizeDelta == true) cachedRectTransform.sizeDelta = landscape == true ? sizeDeltaL : sizeDeltaP;
            if (anchorMin == true) cachedRectTransform.anchorMin = landscape == true ? anchorMinL : anchorMinP;
            if (anchorMax == true) cachedRectTransform.anchorMax = landscape == true ? anchorMaxL : anchorMaxP;
            if (offsetMin == true) cachedRectTransform.offsetMin = landscape == true ? offsetMinL : offsetMinP;
            if (offsetMax == true) cachedRectTransform.offsetMax = landscape == true ? offsetMaxL : offsetMaxP;
            if (pivot == true) cachedRectTransform.pivot = landscape == true ? pivotL : pivotP;
            if (localRotation == true)
                cachedRectTransform.localRotation = landscape == true ? localRotationL : localRotationP;
            if (localScale == true) cachedRectTransform.localScale = landscape == true ? localScaleL : localScaleP;

            if (landscape == true)
            {
                if (landscapeTransitions != null)
                {
                    landscapeTransitions.Begin();
                }

                if (onLandscape != null)
                {
                    onLandscape.Invoke();
                }
            }
            else
            {
                if (portraitTransitions != null)
                {
                    portraitTransitions.Begin();
                }

                if (onPortrait != null)
                {
                    onPortrait.Invoke();
                }
            }
        }

        private void UpdateCachedRectTransform()
        {
            if (cachedRectTransformSet == false)
            {
                cachedRectTransform = GetComponent<RectTransform>();
                cachedRectTransformSet = true;
            }
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
    using TARGET = LeanOrientation;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TARGET))]
    public class LeanOrientation_Editor : CwEditor
    {
        protected override void OnInspector()
        {
            TARGET tgt;
            TARGET[] tgts;
            GetTargets(out tgt, out tgts);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy To Landscape") == true)
            {
                Each(tgts, t => t.CopyToLandscape(), true);
            }

            if (GUILayout.Button("Copy To Portrait") == true)
            {
                Each(tgts, t => t.CopyToPortrait(), true);
            }

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Update Orientation") == true)
            {
                Each(tgts, t => t.UpdateOrientation(), true);
            }

            Separator();

            Draw("anchoredPosition",
                "Should the RectTransform.AnchoredPosition setting change based on device orientation?");
            if (Any(tgts, t => t.AnchoredPosition == true))
            {
                BeginIndent();
                Draw("anchoredPositionL", "The RectTransform.anchoredPosition value when in landscape mode.",
                    "Landscape");
                Draw("anchoredPositionP", "The RectTransform.anchoredPosition value when in portrait mode.",
                    "Portrait");
                EndIndent();
            }

            Draw("sizeDelta", "Should the RectTransform.SizeDelta setting change based on device orientation?");
            if (Any(tgts, t => t.SizeDelta == true))
            {
                BeginIndent();
                Draw("sizeDeltaL", "The RectTransform.sizeDelta value when in landscape mode.", "Landscape");
                Draw("sizeDeltaP", "The RectTransform.sizeDelta value when in portrait mode.", "Portrait");
                EndIndent();
            }

            Draw("anchorMin", "Should the RectTransform.anchorMin setting change based on device orientation?");
            if (Any(tgts, t => t.AnchorMin == true))
            {
                BeginIndent();
                Draw("anchorMinL", "The RectTransform.anchorMin value when in landscape mode.", "Landscape");
                Draw("anchorMinP", "The RectTransform.anchorMin value when in portrait mode.", "Portrait");
                EndIndent();
            }

            Draw("anchorMax", "Should the RectTransform.anchorMax setting change based on device orientation?");
            if (Any(tgts, t => t.AnchorMax == true))
            {
                BeginIndent();
                Draw("anchorMaxL", "The RectTransform.anchorMax value when in landscape mode.", "Landscape");
                Draw("anchorMaxP", "The RectTransform.anchorMax value when in portrait mode.", "Portrait");
                EndIndent();
            }

            Draw("offsetMin", "Should the RectTransform.offsetMin setting change based on device orientation?");
            if (Any(tgts, t => t.OffsetMin == true))
            {
                BeginIndent();
                Draw("offsetMinL", "The RectTransform.offsetMin value when in landscape mode.", "Landscape");
                Draw("offsetMinP", "The RectTransform.offsetMin value when in portrait mode.", "Portrait");
                EndIndent();
            }

            Draw("offsetMax", "Should the RectTransform.offsetMax setting change based on device orientation?");
            if (Any(tgts, t => t.OffsetMax == true))
            {
                BeginIndent();
                Draw("offsetMaxL", "The RectTransform.offsetMax value when in landscape mode.", "Landscape");
                Draw("offsetMaxP", "The RectTransform.offsetMax value when in portrait mode.", "Portrait");
                EndIndent();
            }

            Draw("pivot", "Should the RectTransform.pivot setting change based on device orientation?");
            if (Any(tgts, t => t.Pivot == true))
            {
                BeginIndent();
                Draw("pivotL", "The RectTransform.pivot value when in landscape mode.", "Landscape");
                Draw("pivotP", "The RectTransform.pivot value when in portrait mode.", "Portrait");
                EndIndent();
            }

            Draw("localRotation", "Should the RectTransform.localRotation setting change based on device orientation?");
            if (Any(tgts, t => t.LocalRotation == true))
            {
                BeginIndent();
                DrawEulerAngles("localRotationL", "The RectTransform.localRotation value when in landscape mode.",
                    "Landscape");
                DrawEulerAngles("localRotationP", "The RectTransform.localRotation value when in portrait mode.",
                    "Portrait");
                EndIndent();
            }

            Draw("localScale", "Should the RectTransform.localScale setting change based on device orientation?");
            if (Any(tgts, t => t.LocalScale == true))
            {
                BeginIndent();
                Draw("localScaleL", "The RectTransform.localScale value when in landscape mode.", "Landscape");
                Draw("localScaleP", "The RectTransform.localScale value when in portrait mode.", "Portrait");
                EndIndent();
            }

            Separator();

            Draw("landscapeTransitions",
                "This allows you to perform a transition when the orientation changes to landscape. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the Graphic.color Transition (LeanGraphicColor) component can be used to change the color.\n\nNOTE: Any transitions you perform here must be reverted in the Portrait Transitions setting using a matching transition component.");
            Draw("portraitTransitions",
                "This allows you to perform a transition when the orientation changes to portrait. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the Graphic.color Transition (LeanGraphicColor) component can be used to change the color.\n\nNOTE: Any transitions you perform here must be reverted in the Landscape Transitions setting using a matching transition component.");

            Separator();

            Draw("onLandscape");
            Draw("onPortrait");
        }
    }
}
#endif