using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;

public class GameObjectSizeFitter : MonoBehaviour
{
    private Vector3 originPosition;
    private Vector3 originScale;
    [SerializeField] private CanvasSizeFitter canvasSizeFitter;

    private void Awake()
    {
        originPosition = transform.position;
        originScale = transform.localScale;
    }

    // Start is called before the first frame update
    private void Start()
    {
        float scale;
        if (canvasSizeFitter && !canvasSizeFitter.result)
        {
            scale = Screen.width * 1f / Screen.height / canvasSizeFitter.referenceResolution.GetRatio();
        }
        else
        {
            scale = 1f;
        }

        transform.position = originPosition * scale;
        transform.localScale = originScale * scale;
    }

    // Update is called once per frame
#if UNITY_EDITOR
    private void Update()
    {
        Start();
    }
#endif
}