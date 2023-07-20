using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Text)), ExecuteInEditMode]
public class TextSizeFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Text text;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        text = gameObject.GetComponent<Text>();
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, text.preferredHeight);
    }
}
