using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BGScreenAdapter : MonoBehaviour
{
    private void Awake()
    {
        RectTransform image = gameObject.GetComponent<RectTransform>();
        Vector2 rectTransformSizeDelta = image.sizeDelta;
        float scaleW = 0f, scaleH = 0f;
        if (Screen.width > rectTransformSizeDelta.x) scaleW = Screen.width / rectTransformSizeDelta.x;
        if (Screen.height > rectTransformSizeDelta.y) scaleH = Screen.height / rectTransformSizeDelta.y;
        if (scaleW == 0f && scaleH == 0f) return;
        float scale = Mathf.Max(scaleH, scaleW);
        image.localScale = new Vector2(scale, scale);
    }
}
