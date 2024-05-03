using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cutting : MonoBehaviour
{

    private Sprite selectPhoto;
    [SerializeField] private Button okBtn;
    [SerializeField] private Material guideMat;
    private float cutRangeWidth;
    private float cutRangeHeight;

    [SerializeField] private Image tempImage;
    [SerializeField] private Image showImg;

    private DragScript ltDrag;
    private DragScript rtDrag;
    private DragScript lbDrag;
    private DragScript rbDrag;
    private DragScript cenDrag;

    RectTransform rbRect;
    RectTransform lbRect;
    RectTransform rtRect;
    RectTransform ltRect;
    RectTransform cen;

    private Action<Sprite> onEnd;


    private void Start()
    {
        okBtn.onClick.AddListener(SetCut);
        ltDrag = transform.Find("CutEditorLT").gameObject.AddComponent<DragScript>();
        rtDrag = transform.Find("CutEditorRT").gameObject.AddComponent<DragScript>();
        lbDrag = transform.Find("CutEditorLB").gameObject.AddComponent<DragScript>();
        rbDrag = transform.Find("CutEditorRB").gameObject.AddComponent<DragScript>();
        cenDrag = transform.Find("CutEditorCen").gameObject.AddComponent<DragScript>();
        ltDrag.Draging += DragLT;
        rtDrag.Draging += DragRT;
        lbDrag.Draging += DragLB;
        rbDrag.Draging += DragRB;
        cenDrag.Draging += DragCen;

        rbRect = transform.Find("CutEditorRB").GetComponent<RectTransform>();
        lbRect = transform.Find("CutEditorLB").GetComponent<RectTransform>();
        rtRect = transform.Find("CutEditorRT").GetComponent<RectTransform>();
        ltRect = transform.Find("CutEditorLT").GetComponent<RectTransform>();
        cen = transform.Find("CutEditorCen").GetComponent<RectTransform>();

        ShowCut(tempImage.sprite, spr =>
        {
            showImg.sprite = spr;
            showImg.SetNativeSize();
        });
    }

    /// <summary>
    /// 裁剪（进入裁剪界面，初始化数据，其实也没啥可初始化的）
    /// </summary>
    private void ShowCut(Sprite sprite, Action<Sprite> onEnd)
    {
        this.onEnd = onEnd;
        tempImage.sprite = selectPhoto = sprite;
        cutRangeWidth = tempImage.preferredWidth;
        cutRangeHeight = tempImage.preferredHeight;
        
        transform.Find("GuideMask").GetComponent<RectTransform>().sizeDelta = new Vector2(cutRangeWidth, cutRangeHeight);
  
        transform.Find("CutEditorLT").GetComponent<Image>().color = Color.red;
        transform.Find("CutEditorRT").GetComponent<Image>().color = Color.red;
        transform.Find("CutEditorLB").GetComponent<Image>().color = Color.red;
        transform.Find("CutEditorRB").GetComponent<Image>().color = Color.red;
        
        SetCutRate();
    }

    //设置比例
    private void SetCutRate()
    {
        float width;
        float height;
        if (cutRangeWidth / cutRangeHeight > 1)
        {
            height = cutRangeHeight;
            width = cutRangeHeight;
        }
        else
        {
            width = cutRangeWidth;
            height = cutRangeWidth;
        }
        ltRect.anchoredPosition = new Vector2(-width / 2, height / 2);
        rtRect.anchoredPosition = new Vector2(width / 2, height / 2);
        lbRect.anchoredPosition = new Vector2(-width / 2, -height / 2);
        rbRect.anchoredPosition = new Vector2(width / 2, -height / 2);
        UpdateCen(width, height, Vector2.zero);
    }


    #region   拖动角标事件
    private void DragRB(PointerEventData data)
    {
        if (Input.touchCount > 1)
            return;

        //Vector2 delta = Input.GetTouch(0).deltaPosition;
        Vector2 delta = data.delta;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            delta.y = delta.x * -1;
        }
        else
        {
            delta.x = delta.y * -1;
        }
        

        Vector2 oriPos = rbRect.anchoredPosition;
        rbRect.transform.position += (Vector3)delta;
        Vector2 endPos = rbRect.anchoredPosition;
        if (endPos.x > cutRangeWidth / 2)
        {
            endPos = oriPos;
        }
        else if (endPos.x < ltRect.anchoredPosition.x + 150)
        {
            endPos = oriPos;
        }
        if (endPos.y > ltRect.anchoredPosition.y - 100)
        {
            endPos = oriPos;
        }
        else if (endPos.y < -cutRangeHeight / 2)
        {
            endPos = oriPos;
        }
        rbRect.anchoredPosition = endPos;

        lbRect.anchoredPosition = new Vector2(lbRect.anchoredPosition.x, endPos.y);
        rtRect.anchoredPosition = new Vector3(endPos.x, rtRect.anchoredPosition.y);

        Vector2 pos = Vector2.Lerp(lbRect.anchoredPosition, rtRect.anchoredPosition, 0.5f);
        float width = rtRect.anchoredPosition.x - lbRect.anchoredPosition.x;
        float height = rtRect.anchoredPosition.y - lbRect.anchoredPosition.y;
        UpdateCen(width, height, pos);
    }

    private void DragLB(PointerEventData data)
    {
        //禁止双手操作
        if (Input.touchCount > 1)
            return;

        //Vector2 delta = Input.GetTouch(0).deltaPosition;
        Vector2 delta = data.delta;
        
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            delta.y = delta.x;
        }
        else
        {
            delta.x = delta.y;
        }
        

        Vector2 oriPos = lbRect.anchoredPosition;
        lbRect.transform.position += (Vector3)delta;
        Vector2 endPos = lbRect.anchoredPosition;
        if (endPos.x < -cutRangeWidth / 2)
        {
            endPos = oriPos;
        }
        else if (endPos.x > rtRect.anchoredPosition.x - 150)
        {
            endPos = oriPos;
        }
        if (endPos.y > rtRect.anchoredPosition.y - 100)
        {
            endPos = oriPos;
        }
        else if (endPos.y < -cutRangeHeight / 2)
        {
            endPos = oriPos;
        }
        lbRect.anchoredPosition = endPos;

        ltRect.anchoredPosition = new Vector3(endPos.x, rtRect.anchoredPosition.y);
        rbRect.anchoredPosition = new Vector2(rtRect.anchoredPosition.x, endPos.y);

        Vector2 pos = Vector2.Lerp(ltRect.anchoredPosition, rbRect.anchoredPosition, 0.5f);
        float width = rbRect.anchoredPosition.x - ltRect.anchoredPosition.x;
        float height = ltRect.anchoredPosition.y - rbRect.anchoredPosition.y;
        UpdateCen(width, height, pos);
    }

    private void DragRT(PointerEventData data)
    {
        if (Input.touchCount > 1)
            return;

        //Vector2 delta = Input.GetTouch(0).deltaPosition;
        Vector2 delta = data.delta;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            delta.y = delta.x;
        }
        else
        {
            delta.x = delta.y;
        }

        Vector2 oriPos = rtRect.anchoredPosition;
        rtRect.transform.position += (Vector3)delta;
        Vector2 endPos = rtRect.anchoredPosition;
        if (endPos.x > cutRangeWidth / 2)
        {
            endPos = oriPos;
        }
        else if (endPos.x < lbRect.anchoredPosition.x + 150)
        {
            endPos = oriPos;
        }
        if (endPos.y < lbRect.anchoredPosition.y + 100)
        {
            endPos = oriPos;
        }
        else if (endPos.y > cutRangeHeight / 2)
        {
            endPos = oriPos;
        }
        rtRect.anchoredPosition = endPos;

        rbRect.anchoredPosition = new Vector3(endPos.x, lbRect.anchoredPosition.y);
        ltRect.anchoredPosition = new Vector2(lbRect.anchoredPosition.x, endPos.y);

        Vector2 pos = Vector2.Lerp(ltRect.anchoredPosition, rbRect.anchoredPosition, 0.5f);
        float width = rbRect.anchoredPosition.x - ltRect.anchoredPosition.x;
        float height = ltRect.anchoredPosition.y - rbRect.anchoredPosition.y;
        UpdateCen(width, height, pos);
    }

    private void DragLT(PointerEventData data)
    {
        if (Input.touchCount > 1)
            return;

        //Vector2 delta = Input.GetTouch(0).deltaPosition;

        //Vector2 delta = Input.mouseScrollDelta;
        //Debug.Log(data.delta);
        Vector2 delta = data.delta;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            delta.y = delta.x * -1;
        }
        else
        {
            delta.x = delta.y * -1;
        }

        Vector2 oriPos = ltRect.anchoredPosition;
        ltRect.transform.position += (Vector3)delta;
        Vector2 endPos = ltRect.anchoredPosition;
        if (endPos.x < -cutRangeWidth / 2)
        {
            endPos = oriPos;
        }
        else if (endPos.x > rbRect.anchoredPosition.x - 150)
        {
            endPos = oriPos;
        }
        if (endPos.y < rbRect.anchoredPosition.y + 100)
        {
            endPos = oriPos;
        }
        else if (endPos.y > cutRangeHeight / 2)
        {
            endPos = oriPos;
        }
        ltRect.anchoredPosition = endPos;

        lbRect.anchoredPosition = new Vector2(endPos.x, rbRect.anchoredPosition.y);
        rtRect.anchoredPosition = new Vector3(rbRect.anchoredPosition.x, endPos.y);

        Vector2 pos = Vector2.Lerp(ltRect.anchoredPosition, rbRect.anchoredPosition, 0.5f);
        float width = rtRect.anchoredPosition.x - lbRect.anchoredPosition.x;
        float height = rtRect.anchoredPosition.y - lbRect.anchoredPosition.y;
        UpdateCen(width, height, pos);
    }

    private void DragCen(PointerEventData data)
    {
        Transform cen = transform.Find("CutEditorCen").transform;
        RectTransform cenRect = transform.Find("CutEditorCen").transform.GetComponent<RectTransform>();

        //Debug.Log(cen.position);

        float x = cen.position.x + data.delta.x;
        float y = cen.position.y + data.delta.y;

        //做限定  防止中心框拖出图片框
        float cenX = Mathf.Clamp(x, tempImage.transform.position.x - cutRangeWidth * 0.5f + cenRect.rect.width * 0.5f, tempImage.transform.position.x + cutRangeWidth * 0.5f - cenRect.rect.width * 0.5f);
        float cenY = Mathf.Clamp(y, tempImage.transform.position.y - cutRangeHeight * 0.5f + cenRect.rect.height * 0.5f, tempImage.transform.position.y + cutRangeHeight * 0.5f - cenRect.rect.height * 0.5f);

        cen.position = new Vector2(cenX, cenY);

        Vector2 cenPos = cen.gameObject.GetComponent<RectTransform>().anchoredPosition;
        float width = rtRect.anchoredPosition.x - lbRect.anchoredPosition.x;
        float height = rtRect.anchoredPosition.y - lbRect.anchoredPosition.y;

        if (cenPos.x < (-cutRangeWidth + width) / 2)
            cenPos.x = (-cutRangeWidth + width) / 2;
        else if (cenPos.x > (cutRangeWidth - width) / 2)
            cenPos.x = (cutRangeWidth - width) / 2;
        if (cenPos.y < (-cutRangeHeight + height) / 2)
            cenPos.y = (-cutRangeHeight + height) / 2;
        else if (cenPos.y > (cutRangeHeight - height) / 2)
            cenPos.y = (cutRangeHeight - height) / 2;

        ltRect.anchoredPosition = new Vector2(cenPos.x - width / 2, cenPos.y + height / 2);
        rtRect.anchoredPosition = new Vector2(cenPos.x + width / 2, cenPos.y + height / 2);
        lbRect.anchoredPosition = new Vector2(cenPos.x - width / 2, cenPos.y - height / 2);
        rbRect.anchoredPosition = new Vector2(cenPos.x + width / 2, cenPos.y - height / 2);

        UpdateGuide(width, height, cenPos);
    }

    #endregion


    //更新中心点坐标
    private void UpdateCen(float width, float height, Vector2 pos)
    {
        var cen = transform.Find("CutEditorCen").GetComponent<RectTransform>();
        cen.sizeDelta = new Vector2(width, height);
        cen.anchoredPosition = pos;

        UpdateGuide(width, height, cen.anchoredPosition);
    }

    //更新遮罩信息
    private void UpdateGuide(float width, float height, Vector2 rectPos)
    {
        guideMat.SetFloat("_Width", width);
        guideMat.SetFloat("_Height", height);
        guideMat.SetVector("_Center", new Vector4(rectPos.x, rectPos.y , 0, 0));
    }

    /// <summary>
    /// 确认裁剪
    /// </summary>
    private void SetCut()
    {
        float rate = 1;
        if (selectPhoto.texture.width / selectPhoto.texture.height >= 1f)
        {
            rate = selectPhoto.texture.height / cutRangeHeight;
        }
        else
        {
            rate = selectPhoto.texture.width / cutRangeWidth;
        }

        RectTransform lt = transform.Find("CutEditorLT").GetComponent<RectTransform>();
        RectTransform rb = transform.Find("CutEditorRB").GetComponent<RectTransform>();
        RectTransform cen =transform.Find("CutEditorCen").GetComponent<RectTransform>();
        float width = (rb.anchoredPosition.x - lt.anchoredPosition.x) * rate;
        float height = (lt.anchoredPosition.y - rb.anchoredPosition.y) * rate;
        Vector2 cenPos = new Vector2(cen.anchoredPosition.x * rate + selectPhoto.texture.width / 2, cen.anchoredPosition.y * rate + selectPhoto.texture.height / 2);
        //InitProject.Instance.StartDoCoroutine(ProjectBehaviour.Instence.CutSprite(selectPhoto, Mathf.CeilToInt(width), Mathf.CeilToInt(height), cenPos, (result) =>
        //{
        //    ScreenAdaptation(result);
        //}));
        CutSprite(selectPhoto, Mathf.CeilToInt(width), Mathf.CeilToInt(height), cenPos, onEnd);

    }


    public void CutSprite(Sprite oriSprite, int width, int height, Vector2 center, Action<Sprite> finishedCB = null)
    {
        Texture2D newTexture = new Texture2D(Mathf.CeilToInt(width), Mathf.CeilToInt(height));


        //遍历写入像素的过程
        int start_X = (int)(center.x - width / 2);
        start_X = start_X > 0 ? start_X : 1;
        int start_Y = (int)(center.y - height / 2);
        start_Y = start_Y > 0 ? start_Y : 1;
        int end_X = (int)(center.x + width / 2);
        end_X = end_X < oriSprite.rect.width ? end_X : (int)oriSprite.rect.width;
        int end_Y = (int)(center.y + height / 2);
        end_Y = end_Y < oriSprite.rect.height ? end_Y : (int)oriSprite.rect.height;
        
        for (int y = start_Y - 1; y < end_Y; y++)
        {
            for (int x = start_X - 1; x < end_X; x++)
            {
                Color color = oriSprite.texture.GetPixel(x, y);
                newTexture.SetPixel(x - start_X + 1, y - start_Y + 1, color);
            }
        }

        newTexture.anisoLevel = 2;
        newTexture.Apply();
        Sprite result = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
        //ZLog.Log("图片中心为：" + result.rect.center);
        finishedCB?.Invoke(result);
        //ZLog.Log("裁剪完毕");
    }
}
