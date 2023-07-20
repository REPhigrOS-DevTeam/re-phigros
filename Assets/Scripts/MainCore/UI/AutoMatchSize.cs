using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    [ExecuteInEditMode, RequireComponent(typeof(CanvasScaler)), DisallowMultipleComponent]
    public class AutoMatchSize : MonoBehaviour
    {
        [SerializeField] private float max, min;

        private CanvasScaler _scaler;

        private void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
            Match();
        }

#if UNITY_EDITOR
        private void Update() => Match();
#endif

        private void Match()
        {
            _scaler.matchWidthOrHeight = (float) Screen.width / Screen.height > _scaler.referenceResolution.x / _scaler.referenceResolution.y ? max : min;
        }
    }
}