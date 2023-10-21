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
        [SerializeField] private Button openCharaPreview, closeCharaPreview;
        [SerializeField] private DatuPreviewFadeInOut datuPreviewFadeInOut;

        private void Awake()
        {
            settings.onClick.AddListener(() => SceneTransit.Instance.LoadScene("SettingsScene"));
            singlePlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("ChartSelectorScene"));
            multiPlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("NetworkTest"));
            openCharaPreview.onClick.AddListener(() => datuPreviewFadeInOut.FadeIn(0.15f, 0.05f));
            closeCharaPreview.onClick.AddListener(() => datuPreviewFadeInOut.FadeOut(0.15f, 0.05f));
#if !RELEASE_VERSION && !UNITY_EDITOR
            multiPlay.interactable = false;
#else
            multiPlay.interactable = true;
#endif
        }

        private const int Offset1 = 234 + 5 + 42,
            Offset2 = 5 + 42 + 11;
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

            usernameText.text =
                $"{PlayerPrefs.GetString("player_name", "kagari939")}\n<size=55>@{GlobalSetting.username}</size>";
            avatarBackGround.sizeDelta = new Vector2(Offset1 + usernameText.preferredWidth, 240f);
            RectTransform rectTransform = avatarBackGround.parent as RectTransform;
            rectTransform.anchoredPosition =
                new Vector2(-454f - avatarBackGround.sizeDelta.x / 2f - avatarBackGround.anchoredPosition.x + Offset2,
                    rectTransform.anchoredPosition.y);
        }
    }
}