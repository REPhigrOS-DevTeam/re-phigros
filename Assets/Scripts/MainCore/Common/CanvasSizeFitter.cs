using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    [RequireComponent(typeof(CanvasScaler)), ExecuteInEditMode]
    public class CanvasSizeFitter : MonoBehaviour
    {
        private CanvasScaler canvasScaler;
        [SerializeField] private bool invert = false;
        public Vector2 referenceResolution => (!canvasScaler ? gameObject.GetComponent<CanvasScaler>() : canvasScaler).referenceResolution;
        [HideInInspector] public bool result;

        // Start is called before the first frame update
        void Awake()
        {
            canvasScaler = gameObject.GetComponent<CanvasScaler>();
            if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize || canvasScaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                result = false;
                return;
            }
            
            result = (Screen.width * 1f / Screen.height > canvasScaler.referenceResolution.GetRatio()) ^ invert;
        }

        private void Start()
        {
            canvasScaler.matchWidthOrHeight = result ? 1 : 0;
        }

#if UNITY_EDITOR
        // Update is called once per frame
        void Update()
        {
            if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize || canvasScaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight) return; 
            
            result = (Screen.width * 1f / Screen.height > canvasScaler.referenceResolution.GetRatio()) ^ invert;
            canvasScaler.matchWidthOrHeight = result ? 1 : 0;
        }
#endif
    }
}