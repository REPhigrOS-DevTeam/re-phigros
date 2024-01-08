using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadTest : MonoBehaviour
{
    [SerializeField] private GameObject badTap;
    [SerializeField] private Sprite badTapSprite;
    [SerializeField] private bool paintBad = true;
    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) GenerateBad();
    }

    private void GenerateBad()
    {
        GameObject badTapInstance = Instantiate(badTap, transform.position, Quaternion.identity);
        badTapInstance.transform.localScale = transform.lossyScale;
        badTapInstance.GetComponent<SpriteRenderer>().sprite = badTapSprite;
        Animation badTapAnimation = badTapInstance.GetComponent<Animation>();
        badTapAnimation.clip = badTapAnimation.GetClip(paintBad ? "BadTap" : "BadTapWhite");
        badTapAnimation.Play();
    }
}
