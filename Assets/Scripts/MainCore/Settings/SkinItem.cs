using MainCore.Settings;
using UnityEngine;
using UnityEngine.UI;

public class SkinItem : MonoBehaviour
{
    private bool isExternal;
    private SettingManager settingManager;
    private string id;
    private Image image;
    private Text text;

    public void Init(SettingManager settingManager, bool isExternal, string id, string name)
    {
        image = gameObject.GetComponent<Image>();
        text = gameObject.GetComponent<Text>();
        text.text = name;
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
        this.settingManager = settingManager;
        this.isExternal = isExternal;
        this.id = id;
    }

    private void OnClick()
    {
        settingManager.UpdateSelectedSkinItem(isExternal, id);
    }

    public void SetSelected(bool isExternal, string id)
    {
        bool state = isExternal == this.isExternal && id == this.id;
        text.color = state ? Color.white : Color.black;
        image.color = state ? Color.black : Color.white;
    }
}
