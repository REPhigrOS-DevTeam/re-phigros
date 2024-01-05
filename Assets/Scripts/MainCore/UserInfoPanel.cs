using MainCore;
using Network;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoPanel : MonoBehaviour
{
    [SerializeField] private Text usernameText, displayNameText;
    [SerializeField] private RectTransform avatarBackGround, nameSplitLine;

    private const int Offset1 = 234 + 5 + 42;
    
    // Start is called before the first frame update
    void Start()
    {
        usernameText.text = RepAPI.Inited ? $"@{GlobalSetting.Username}" : "Offline...";
        displayNameText.text = PlayerPrefs.GetString("player_name", "kagari939");
        float width = Mathf.Max(usernameText.preferredWidth, displayNameText.preferredWidth);
        avatarBackGround.sizeDelta = new Vector2(Offset1 + width, avatarBackGround.sizeDelta.y);
        nameSplitLine.sizeDelta = new Vector2(width / nameSplitLine.localScale.x, nameSplitLine.sizeDelta.y);
    }
}
