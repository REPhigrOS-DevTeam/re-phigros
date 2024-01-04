using MainCore;
using UnityEngine;

public class SkinPreview : MonoBehaviour
{
    public static float Size = 0.2f;
    [SerializeField] private SpriteRenderer click,
        clickMh,
        drag,
        dragMh,
        flick,
        flickMh,
        holdHead,
        holdBody,
        holdEnd,
        holdHeadMh,
        holdBodyMh,
        holdEndMh;

    public void UpdateSkin()
    {
        click.sprite = GlobalSetting.CurrentSkinInfo.click;
        clickMh.sprite = GlobalSetting.CurrentSkinInfo.clickMh;
        drag.sprite = GlobalSetting.CurrentSkinInfo.drag;
        dragMh.sprite = GlobalSetting.CurrentSkinInfo.dragMh;
        flick.sprite = GlobalSetting.CurrentSkinInfo.flick;
        flickMh.sprite = GlobalSetting.CurrentSkinInfo.flickMh;
        holdHead.sprite = GlobalSetting.CurrentSkinInfo.holdHead;
        holdBody.sprite = GlobalSetting.CurrentSkinInfo.holdBody;
        holdEnd.sprite = GlobalSetting.CurrentSkinInfo.holdEnd;
        holdHeadMh.sprite = GlobalSetting.CurrentSkinInfo.holdHeadMh;
        holdBodyMh.sprite = GlobalSetting.CurrentSkinInfo.holdBodyMh;
        holdEndMh.sprite = GlobalSetting.CurrentSkinInfo.holdEndMh;
        click.transform.localScale = new Vector3(Size, Size, 1f);
        clickMh.transform.localScale = new Vector3(Size, Size, 1f);
        drag.transform.localScale = new Vector3(Size, Size, 1f);
        dragMh.transform.localScale = new Vector3(Size, Size, 1f);
        flick.transform.localScale = new Vector3(Size, Size, 1f);
        flickMh.transform.localScale = new Vector3(Size, Size, 1f);
        holdHead.transform.localScale = new Vector3(Size, Size, 1f);
        holdBody.transform.localScale =
            new Vector3(Size, 2.7f * 2f / GlobalSetting.CurrentSkinInfo.holdLengthFactor, 1f); // 2.7f * 2f是头尾之间的差值
        holdEnd.transform.localScale = new Vector3(Size, Size, 1f);
        holdHeadMh.transform.localScale = new Vector3(Size, Size, 1f);
        holdBodyMh.transform.localScale =
            new Vector3(Size, 2.7f * 2f / GlobalSetting.CurrentSkinInfo.holdMhLengthFactor, 1f);
        holdEndMh.transform.localScale = new Vector3(Size, Size, 1f);
    }
}