using System;
using MainCore.Common;
using MainCore.Data;
using MainCore.Utilities;
using Newtonsoft.Json;
using SimpleFileBrowser;
using Unimage;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class MainManager : MonoBehaviour
    {
        [SerializeField] private Button settings;
        [SerializeField] private Button singlePlay, multiPlay;
        [SerializeField] private GameObject characterSelectionObj;
        [SerializeField] private Button openCharaPreview, closeCharaPreview;
        [SerializeField] private DatuPreviewFadeInOut datuPreviewFadeInOut;
        [SerializeField] private Button openCharacterSelections, deleteCharacter, selectCharacter, editCharacter, closeCharacterSelections;
        [SerializeField] private SpriteRenderer character;
        [SerializeField] private Sprite defaultCharacter;

        private void Awake()
        {
            settings.onClick.AddListener(() => SceneTransit.Instance.LoadScene("SettingsScene"));
            singlePlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("ChartSelectorScene"));
            multiPlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("NetworkTest"));
            openCharaPreview.onClick.AddListener(() => datuPreviewFadeInOut.FadeIn(0.15f, 0.05f));
            closeCharaPreview.onClick.AddListener(() => datuPreviewFadeInOut.FadeOut(0.15f, 0.05f));
#if UNITY_EDITOR
            openCharacterSelections.onClick.AddListener(OpenCharacterOptions);
#elif false
            openCharacterSelections.onClick.AddListener(OpenCharacterSelector);
#endif
            deleteCharacter.onClick.AddListener(() =>
            {
                character.sprite = defaultCharacter;
                PlayerPrefs.DeleteKey("character");
                PlayerPrefs.Save();
            });
            deleteCharacter.onClick.AddListener(CloseCharacterOptions);
            selectCharacter.onClick.AddListener(ImportCharacterPackage);
            selectCharacter.onClick.AddListener(CloseCharacterOptions);
            closeCharacterSelections.onClick.AddListener(CloseCharacterOptions);
            editCharacter.onClick.AddListener(() => SceneTransit.Instance.LoadScene("CharacterAdjustScene"));
#if !RELEASE_VERSION && !UNITY_EDITOR
            multiPlay.interactable = false;
#else
            multiPlay.interactable = true;
#endif
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

            if (PlayerPrefs.HasKey("character"))
            {
                string[] strings = PlayerPrefs.GetString("character").Split("\n");
                ExternalCharacterInfo externalCharacterInfo = JsonConvert.DeserializeObject<ExternalCharacterInfo>(strings[0]);
                character.sprite = Util.ReadSprite(Convert.FromBase64String(strings[1]), externalCharacterInfo.Pivot, externalCharacterInfo.PixelsPerUnit);
            }
            else
            {
                character.sprite = defaultCharacter;
            }

            CloseCharacterOptions();
            datuPreviewFadeInOut.FadeOut(0f);
        }

        private void CloseCharacterOptions()
        {
            characterSelectionObj.SetActive(false);
        }

        private void OpenCharacterOptions()
        {
            deleteCharacter.interactable = PlayerPrefs.HasKey("character");
            characterSelectionObj.SetActive(true);
        }

        private void OpenCharacterSelector()
        {
        }

        private void ImportCharacterPackage()
        {
            FileBrowser.SetFilters(false, ".charapkg");
            FileBrowser.ShowLoadDialog(paths =>
                {
                    Sprite sprite = OnSelectedCharacterPackage(paths[0]);
                    if (sprite) character.sprite = sprite;
                }, () => { }, FileBrowser.PickMode.Files, false,
                Util.DataPath, null, "选择立绘包...", "确定");
        }

        private Sprite OnSelectedCharacterPackage(string path)
        {
            try
            {
                CharacterImage characterImage = Util.GetCharacterImage(path,
                    () => InGameUIManager.ShowModalWindowWithClose("错误", "文件不合法", () => { }, "确定"),
                    () => InGameUIManager.ShowModalWindowWithClose("错误", "文件格式不正确", () => { }, "确定"));
                Sprite readSprite = characterImage.Sprite;
                PlayerPrefs.SetString("character", characterImage.ToString());
                PlayerPrefs.Save();
                return readSprite;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (UnimageException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "文件格式不正确", () => { }, "确定");
                return null;
            }
        }
    }

    public class CharacterImage
    {
        public ExternalCharacterInfo Info;
        public byte[] TextureData;
        public Texture2D Texture => Util.ReadFileAsTexture(TextureData);
        public Sprite Sprite => Util.ReadSprite(Texture, Info.Pivot, Info.PixelsPerUnit);
        public override string ToString()
        {
            return JsonConvert.SerializeObject(Info, Formatting.None) + "\n" + Convert.ToBase64String(TextureData);
        }
    }
}