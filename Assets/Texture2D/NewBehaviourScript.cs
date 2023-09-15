using System.Collections;
using System.Collections.Generic;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Sprite sprite = gameObject.GetComponent<Image>().sprite;
        Debug.Log(Util.GetAvgColor(sprite));
        Debug.Log(transform.parent.parent.gameObject.GetComponent<Image>().color = Util.GetPossibleBGColor(sprite));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
