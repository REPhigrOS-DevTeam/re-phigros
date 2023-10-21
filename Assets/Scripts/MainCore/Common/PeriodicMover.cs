using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MainCore.Utilities;
using UnityEngine;

public class PeriodicMover : MonoBehaviour
{
    private Vector3 originPosition;
    private float scale;
    [SerializeField] private float amplitude = 1f, period = 1f;
    [SerializeField] private Vector2 targetResolution;
    private Stopwatch stopwatch = new Stopwatch();
    // Start is called before the first frame update
    private void Awake()
    {
        originPosition = transform.position;
        scale = Screen.width * 1f / Screen.height > targetResolution.GetRatio() ? targetResolution.y / Screen.height : targetResolution.x / Screen.width;
    }

    private void Start()
    {
        transform.localScale = new Vector3(scale, scale, 1);
        stopwatch.Start();
        Update();
    }

    // Update is called once per frame
    private void Update()
    {
#if UNITY_EDITOR
        scale = Screen.width * 1f / Screen.height > targetResolution.GetRatio() ? targetResolution.y / Screen.height : targetResolution.x / Screen.width;
        transform.localScale = new Vector3(scale, scale, 1);    
#endif
        transform.position = (originPosition + new Vector3(0f, amplitude * Mathf.Sin(2 * Mathf.PI * stopwatch.ElapsedMilliseconds / 1000f / period), 0f)) * scale;
    }

    private void OnDisable()
    {
        transform.position = originPosition * scale;
    }
}
