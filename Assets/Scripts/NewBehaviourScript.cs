using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NewBehaviourScript : MonoBehaviour
{
    private SpriteRenderer sprite;
    // Start is called before the first frame update
    void Start()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("SR bounds: " + sprite.bounds);
            Debug.Log(sprite.size);
            Debug.Log("S bounds: " + sprite.sprite.bounds);
            Debug.Log(sprite.sprite.rect);
        }
    }
}
