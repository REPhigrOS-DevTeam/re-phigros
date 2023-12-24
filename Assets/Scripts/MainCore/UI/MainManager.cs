using System;
using System.Text;
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

        [SerializeField] private Button openCharacterSelections,
            deleteCharacter,
            selectCharacter,
            editCharacter,
            closeCharacterSelections;

        [SerializeField] private SpriteRenderer character, datuCharacter;
        [SerializeField] private Sprite defaultCharacter;

        private static CharacterImage characterImage;

        private void Awake()
        {
            settings.onClick.AddListener(() => SceneTransit.Instance.LoadScene("SettingsScene"));
            singlePlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("ChartSelectorScene"));
            multiPlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("NetworkTest"));
            openCharaPreview.onClick.AddListener(() =>
            {
                datuPreviewFadeInOut.FadeIn(0.15f, 0.05f);
                datuCharacter.sprite = character.sprite;
            });
            closeCharaPreview.onClick.AddListener(() => datuPreviewFadeInOut.FadeOut(0.15f, 0.05f));
            openCharacterSelections.onClick.AddListener(OpenCharacterOptions);
#if false
            openCharacterSelections.onClick.AddListener(OpenCharacterSelector);
#endif
            deleteCharacter.onClick.AddListener(() =>
            {
                characterImage = null;
                character.sprite = defaultCharacter;
                PlayerPrefs.DeleteKey("character");
                PlayerPrefs.Save();
            });
            deleteCharacter.onClick.AddListener(CloseCharacterOptions);
            selectCharacter.onClick.AddListener(ImportCharacterPackage);
            selectCharacter.onClick.AddListener(CloseCharacterOptions);
            closeCharacterSelections.onClick.AddListener(CloseCharacterOptions);
#if UNITY_EDITOR
            editCharacter.onClick.AddListener(() => SceneTransit.Instance.LoadScene("CharacterAdjustScene"));
            editCharacter.interactable = true;
            editCharacter.transform.Find("Mask").Find("Icon").gameObject.GetComponent<Image>().SetAlpha(1f);
#else
            editCharacter.interactable = false;
            editCharacter.transform.Find("Mask").Find("Icon").gameObject.GetComponent<Image>().SetAlpha(0.5f);
#endif
            multiPlay.interactable = true;
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

            if (characterImage == null && PlayerPrefs.HasKey("character"))
            {
                try
                {
                    string[] strings = PlayerPrefs.GetString("character").Split("\n");
                    if (strings.Length == 3)
                    {
                        byte[] infoData = Encoding.UTF8.GetBytes(strings[0]);
                        byte[] imageData = Convert.FromBase64String(strings[1]);
                        byte[] hashData = Convert.FromBase64String(strings[2]);

                        if (Util.ValidateFileHash(hashData, imageData, infoData))
                        {
                            characterImage = new CharacterImage
                            {
                                TextureData = imageData,
                                InfoData = infoData,
                                HashData = hashData,
                                Info = JsonConvert.DeserializeObject<ExternalCharacterInfo>(strings[0])
                            };
                            if (characterImage.Info == null) throw new NullReferenceException();
                        }
                        else
                        {
                            PlayerPrefs.DeleteKey("character");
                            PlayerPrefs.Save();
                        }
                    }
                    else
                    {
                        PlayerPrefs.DeleteKey("character");
                        PlayerPrefs.Save();
                    }
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogError("无法读取自定义角色");
                    Debug.LogException(ex);
#endif
                    PlayerPrefs.DeleteKey("character");
                    PlayerPrefs.Save();
                }
            }
            character.sprite = characterImage == null ? character.sprite = defaultCharacter : characterImage.Sprite;

            CloseCharacterOptions();
            datuPreviewFadeInOut.FadeOut(0f);
            Update();
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (!InGameUIManager.IsActive) InGameUIManager.ShowModalWindowWithClose("提示", "确定要退出吗？", Util.QuitApp, "是", () => {},  "否");
#endif
        }

        private void CloseCharacterOptions()
        {
            characterSelectionObj.SetActive(false);
        }

        private void OpenCharacterOptions()
        {
            deleteCharacter.interactable = characterImage != null;
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
                characterImage = Util.GetCharacterImage(path,
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
        public byte[] InfoData;
        public byte[] TextureData;
        public byte[] HashData;
        public ExternalCharacterInfo Info;
        public Texture2D Texture => Util.ReadFileAsTexture(TextureData);
        public Sprite Sprite => Util.ReadSprite(Texture, Info.Pivot, Info.PixelsPerUnit);

        public override string ToString()
        {
            return Encoding.UTF8.GetString(InfoData) + "\n" + Convert.ToBase64String(TextureData) + "\n" + Convert.ToBase64String(HashData);
        }
    }
}