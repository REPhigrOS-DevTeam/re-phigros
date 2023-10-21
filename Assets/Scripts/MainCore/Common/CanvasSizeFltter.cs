using System.Collections;
using System.Collections.Generic;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasSizeFltter : MonoBehaviour
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