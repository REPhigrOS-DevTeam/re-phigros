using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionText : MonoBehaviour
{
    private void Awake()
    {
        Text text = GetComponent<Text>();
#if !RELEASE_VERSION || UNITY_EDITOR
        text.text = $"Development Version - RE:Phigros {Application.version} by kagari939\n";
#else
        text.text = $"RE:Phigros {Application.version} by kagari939\n";
#endif
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
