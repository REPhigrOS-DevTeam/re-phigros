using DG.Tweening;
using LeTai.Asset.TranslucentImage;
using MainCore.Common;
using Network.Account;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class MainManager : MonoBehaviour
    {
        [SerializeField] private TranslucentImageSource foregroundSource;
        private ScalableBlurConfig foregroundConfig;
        [SerializeField] private GameObject foregroundMask;
        [SerializeField] private Button start, settings;
        [SerializeField] private Button singlePlay, multiPlay, exitPlayChoice;
        [SerializeField] private Text usernameText;

        private void Awake()
        {
            foregroundConfig = (ScalableBlurConfig)foregroundSource.BlurConfig;
            start.onClick.AddListener(Blur);
            settings.onClick.AddListener(() => SceneTransit.Instance.TransitTo("SettingsScene"));
            singlePlay.onClick.AddListener(() => SceneTransit.Instance.TransitTo("ChartSelectorScene"));
            multiPlay.onClick.AddListener(() => SceneTransit.Instance.TransitTo("NetworkTest"));
            exitPlayChoice.onClick.AddListener(Unblur);
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

            foregroundConfig.Strength = 0f;
            foregroundMask.SetActive(false);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.J)) Blur();
            else if (Input.GetKeyDown(KeyCode.K)) Unblur();
        }

        private const float blurStrength = 16f;
        private const float blurDuration = 0.3f;

        private async void Blur()
        {
            DOTween.To(() => foregroundConfig.Strength,
                x => foregroundConfig.Strength = x * Mathf.Sin((x / blurStrength * Mathf.PI) / 2f), blurStrength,
                blurDuration);
            await new WaitForSeconds(blurDuration / 2f);
            foregroundMask.SetActive(true);
        }

        private async void Unblur()
        {
            DOTween.To(() => foregroundConfig.Strength,
                x => foregroundConfig.Strength = x * (1f - Mathf.Cos((x / blurStrength * Mathf.PI) / 2f)), 0f,
                blurDuration);
            await new WaitForSeconds(blurDuration / 2f);
            foregroundMask.SetActive(false);
        }
    }
}