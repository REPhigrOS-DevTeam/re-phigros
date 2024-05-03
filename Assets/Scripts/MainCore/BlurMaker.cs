using LeTai.Asset.TranslucentImage;
using UnityEngine;

[RequireComponent(typeof(TranslucentImageSource))]
public class BlurMaker : MonoBehaviour
{
    [SerializeField] private float strength;
    private void Awake()
    {
        var source = GetComponent<TranslucentImageSource>();
        ScalableBlurConfig config = ScriptableObject.CreateInstance<ScalableBlurConfig>();
        config.Strength = strength;
        source.BlurConfig = config;
    }
}
