using MainCore;
using UnityEngine;

public class SkinPreview : MonoBehaviour
{
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
        holdBody.transform.localScale =
            new Vector3(0.15f, 2.7f * 2f / GlobalSetting.CurrentSkinInfo.holdLengthFactor, 0.15f); // 2.7f * 2f是头尾之间的差值
        holdBodyMh.transform.localScale =
            new Vector3(0.15f, 2.7f * 2f / GlobalSetting.CurrentSkinInfo.holdMhLengthFactor, 0.15f);
    }
}