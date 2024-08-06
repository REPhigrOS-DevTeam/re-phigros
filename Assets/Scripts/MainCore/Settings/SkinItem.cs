using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Settings
{
    /// <summary>
    /// 负责设置界面的显示
    /// </summary>
    public class SkinItem : MonoBehaviour
    {
        private bool isExternal;
        private SettingManager settingManager;
        private string id;
        private Image image;
        private TextMeshProUGUI text;

        public void Init(SettingManager settingManager, bool isExternal, string id, string name)
        {
            image = gameObject.GetComponent<Image>();
            text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
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

        public void SetSelected(bool isExternalA, string idA)
        {
            bool state = isExternalA == isExternal && idA == id;
            text.color = state ? Color.white : Color.black;
            image.color = state ? Color.black : Color.white;
        }
    }
}