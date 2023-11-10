using System.Diagnostics;
using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PeriodicMover : MonoBehaviour
{
    private Vector3 originPosition;
    private Vector3 originScale;
    [SerializeField] private float amplitude = 1f, period = 1f;
    [SerializeField] private CanvasSizeFitter canvasSizeFitter;
    
    private Stopwatch stopwatch = new Stopwatch();
    private float scale;

    // Start is called before the first frame update
    private void Awake()
    {
        originPosition = transform.position;
        originScale = transform.localScale;
        if (canvasSizeFitter && !canvasSizeFitter.result)
        {
            scale = Screen.width * 1f / Screen.height / canvasSizeFitter.referenceResolution.GetRatio();
        }
        else
        {
            scale = 1f;
        }
    }

    private void Start()
    {
        stopwatch.Start();
        Update();
    }

    // Update is called once per frame
    private void Update()
    {
        #if UNITY_EDITOR
        if (canvasSizeFitter && !canvasSizeFitter.result)
        {
            scale = Screen.width * 1f / Screen.height / canvasSizeFitter.referenceResolution.GetRatio();
        }
        else
        {
            scale = 1f;
        }
        #endif
        transform.position = (originPosition + new Vector3(0f,
            amplitude * Mathf.Sin(2 * Mathf.PI * stopwatch.ElapsedMilliseconds / 1000f / period), 0f)) * scale;
        transform.localScale = originScale * scale;
    }

    private void OnDisable()
    {
        transform.position = originPosition * scale;
        transform.localScale = originScale * scale;
    }
}