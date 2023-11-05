using System;
using System.Collections;
using DG.Tweening;
using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class InGameModalWindow : MonoSingleton<InGameModalWindow>
{
    [SerializeField] private Image backPanel;
    [SerializeField] private Image modalWindow;

    [Space(10)] [SerializeField] private Text headerText;

    [SerializeField] private Text bodyText;
    [SerializeField] private Text confirmText;
    [SerializeField] private Text cancelText;
    [SerializeField] private Text alternateText;

    [Space(10)] [SerializeField] private Button confirmButton;

    [SerializeField] private Button cancelButton;
    [SerializeField] private Button alternateButton;
    private Action alternate = null;
    private Action cancel = null;

    private Action confirm;

    public bool IsActive { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private void Start()
    {
        backPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 打开一个模态窗口
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="content">内容</param>
    /// <param name="confirmAction">确认后的行动</param>
    /// /// <param name="confirmtext">确认按钮的text</param>
    /// <param name="cancelAction">取消后的行动</param>
    /// <param name="canceltext">取消按钮的text</param>
    public void Show(string title, string content, Action confirmAction, string confirmtext = "Confirm",
        Action cancelAction = null, string canceltext = "Cancel", Action alternateAction = null,
        string alternatetext = "Alternate")
    {
        IsActive = true;
        backPanel.gameObject.SetActive(true);
        backPanel.color = new Color(0, 0, 0, 0);
        modalWindow.rectTransform.anchoredPosition = new Vector2(0, -800f);
        modalWindow.rectTransform.localScale = new Vector3(0, 0);

        headerText.text = title;
        bodyText.text = content;

        confirm = confirmAction;
        confirmButton.onClick.AddListener(confirmClicked);
        confirmText.text = confirmtext;

        cancel = cancelAction;
        if (cancelAction != null)
        {
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.AddListener(cancelClicked);
            cancelText.text = canceltext;
        }
        else
            cancelButton.gameObject.SetActive(false);

        alternate = alternateAction;
        if (alternateAction != null)
        {
            alternateButton.gameObject.SetActive(true);
            alternateButton.onClick.AddListener(alternateClicked);
            alternateText.text = alternatetext;
        }
        else
            alternateButton.gameObject.SetActive(false);

        modalWindow.rectTransform.DOAnchorPosY(0, .3f).SetEase(Ease.OutBack);
        modalWindow.rectTransform.DOScale(new Vector3(1, 1), .3f).SetEase(Ease.OutBack);
        backPanel.DOFade(.4f, .3f);
    }

    public void Hide()
    {
        if (!IsActive) return;
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        alternateButton.onClick.RemoveAllListeners();
        modalWindow.rectTransform.DOAnchorPosY(-800, .3f).SetEase(Ease.InBack);
        backPanel.DOFade(0, .3f);
        modalWindow.rectTransform.DOScale(new Vector3(0, 0), .3f).SetEase(Ease.InBack).onComplete +=
            () => { backPanel.gameObject.SetActive(false); };
        StartCoroutine(SetInactive());
    }

    private IEnumerator SetInactive()
    {
        yield return new WaitForSecondsRealtime(.3f);
        IsActive = false;
        InGameUIManager.CheckWindowToShow();
    }

    public void HideForcely()
    {
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        alternateButton.onClick.RemoveAllListeners();
        modalWindow.rectTransform.anchoredPosition = new Vector2(modalWindow.rectTransform.anchoredPosition.x, -800);
        backPanel.SetAlpha(0);
        modalWindow.rectTransform.localScale = new Vector3(0, 0);
        backPanel.gameObject.SetActive(false);
        IsActive = false;
        InGameUIManager.CheckWindowToShow();
    }

    private void confirmClicked()
    {
        confirm.Invoke();
    }

    private void cancelClicked()
    {
        cancel.Invoke();
    }

    private void alternateClicked()
    {
        alternate.Invoke();
    }
}