using LeTai.Asset.TranslucentImage;
using MainCore.Common;
using Network.Account;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class MainManager : MonoBehaviour
    {
        [SerializeField] private Button settings;
        [SerializeField] private Button singlePlay, multiPlay;
        [SerializeField] private Text usernameText;
        [SerializeField] private RectTransform avatarBackGround;

        private void Awake()
        {
            settings.onClick.AddListener(() => SceneTransit.Instance.LoadScene("SettingsScene"));
            singlePlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("ChartSelectorScene"));
            multiPlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("NetworkTest"));
        }

        public void Start()
        {
            if (PlayerPrefs.GetInt("half_res", 0) == 1)
            {
                Debug.Log("Half Resolution Mode Enabled");
                var currentRes = GlobalSetting.OriginResolution;
                currentRes.height /= 2;
                currentRes.width /= 2;
                Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreenMode);
            }
            else
            {
                Debug.Log("Half Resolution Mode Disabled");
                var currentRes = GlobalSetting.OriginResolution;
                Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreenMode);
            }

            Application.targetFrameRate = PlayerPrefs.GetInt("refresh_rate", 60);

            GlobalSetting.Reset();

            usernameText.text = PlayerPrefs.GetString("player_name", "kagari939") + "@" + LoginManager.Username;
            avatarBackGround.sizeDelta = new Vector2(234 + 5 + 42 + usernameText.preferredWidth, 240f);
        }
    }
}