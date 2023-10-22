using System;
using System.IO;
using MainCore.Common;
using MainCore.Utilities;
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
        [SerializeField] private Button openCharaPreview, closeCharaPreview;
        [SerializeField] private DatuPreviewFadeInOut datuPreviewFadeInOut;
        [SerializeField] private Button selectCharacter;
        [SerializeField] private SpriteRenderer character;
        [SerializeField] private Sprite defaultCharacter;

        private void Awake()
        {
            settings.onClick.AddListener(() => SceneTransit.Instance.LoadScene("SettingsScene"));
            singlePlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("ChartSelectorScene"));
            multiPlay.onClick.AddListener(() => SceneTransit.Instance.LoadScene("NetworkTest"));
            openCharaPreview.onClick.AddListener(() => datuPreviewFadeInOut.FadeIn(0.15f, 0.05f));
            closeCharaPreview.onClick.AddListener(() => datuPreviewFadeInOut.FadeOut(0.15f, 0.05f));
            selectCharacter.onClick.AddListener(ImportCharacterPackage);
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
                character.sprite = Util.ReadSprite(Convert.FromBase64String(strings[3]),
                    new Vector2(float.Parse(strings[1]), float.Parse(strings[2])), float.Parse(strings[0]));
            }
            else
            {
                character.sprite = defaultCharacter;
            }
        }

        private void ImportCharacterPackage()
        {
            FileBrowser.SetFilters(false, ".charapkg");
            FileBrowser.ShowLoadDialog(paths =>
                {
                    Sprite sprite = OnSelectedCharacterPackage(paths[0]);
                    if (sprite) character.sprite = sprite;
                }, () => { }, FileBrowser.PickMode.Files, false,
                PlayerPrefs.GetString("file_path", Application.persistentDataPath), null, "选择立绘包...", "确定");
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
        public float PixelsPerUnit;
        public Vector2 Pivot;
        public byte[] TextureData;
        public Texture2D Texture => Util.ReadFileAsTexture(TextureData);
        public Sprite Sprite => Util.ReadSprite(Texture, Pivot, PixelsPerUnit);

        public override string ToString()
        {
            return string.Join("\n", PixelsPerUnit, Pivot.x, Pivot.y, Convert.ToBase64String(TextureData));
        }
    }
}