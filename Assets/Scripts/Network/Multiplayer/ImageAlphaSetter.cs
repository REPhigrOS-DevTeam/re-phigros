using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

[RequireComponent(typeof(Image))]
public class ImageAlphaSetter : MonoBehaviour
{
    private Image image;
    private void Awake()
    {
        image = gameObject.GetComponent<Image>();
    }

    public void Set(float value)
    {
        value = Mathf.Clamp01(value);
        image.SetAlpha(value);
    }
}
