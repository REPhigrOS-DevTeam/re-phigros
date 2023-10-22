using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    [RequireComponent(typeof(CanvasScaler)), ExecuteInEditMode]
    public class CanvasSizeFitter : MonoBehaviour
    {
        private CanvasScaler canvasScaler;

        // Start is called before the first frame update
        void Start()
        {
            canvasScaler = gameObject.GetComponent<CanvasScaler>();
            canvasScaler.matchWidthOrHeight =
                Screen.width * 1f / Screen.height > canvasScaler.referenceResolution.GetRatio() ? 1 : 0;
        }

#if UNITY_EDITOR
        // Update is called once per frame
        void Update()
        {
            canvasScaler.matchWidthOrHeight =
                Screen.width * 1f / Screen.height > canvasScaler.referenceResolution.GetRatio() ? 1 : 0;
        }
#endif
    }
}