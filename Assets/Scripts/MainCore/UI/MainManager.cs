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
        [SerializeField] private Text usernameText, displayNameText;
        [SerializeField] private RectTransform avatarBackGround, nameSplitLine;
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

            usernameText.text = $"@{GlobalSetting.username}";
            displayNameText.text = PlayerPrefs.GetString("player_name", "kagari939");
            float width = Mathf.Max(usernameText.preferredWidth, displayNameText.preferredWidth);
            avatarBackGround.sizeDelta = new Vector2(Offset1 + width, avatarBackGround.sizeDelta.y);
            nameSplitLine.sizeDelta = new Vector2(width / nameSplitLine.localScale.x, nameSplitLine.sizeDelta.y);

            if (PlayerPrefs.HasKey("character"))
            {
                string[] strings = PlayerPrefs.GetString("character").Split("\n");
                character.sprite = ReadSprite(Convert.FromBase64String(strings[3]), new Vector2(float.Parse(strings[1]), float.Parse(strings[2])), float.Parse(strings[0]));
            }
            else
            {
                character.sprite = defaultCharacter;
            }
        }

        private void ImportCharacterPackage()
        {
            FileBrowser.ShowLoadDialog(paths =>
                {
                    Sprite sprite = OnSelectedCharacterPackage(paths);
                    if (sprite) character.sprite = sprite;
                }, () => { }, FileBrowser.PickMode.Files, false,
                PlayerPrefs.GetString("file_path", Application.persistentDataPath), null, "选择曲绘包...", "确定");
            FileBrowser.SetFilters(false, ".charapkg");
        }

        private Sprite OnSelectedCharacterPackage(string[] paths)
        {
            string tmpDirPath = Application.temporaryCachePath + "/tmp";
            if (Directory.Exists(tmpDirPath)) Directory.Delete(tmpDirPath, true);
            Directory.CreateDirectory(tmpDirPath);
            try
            {
                ZipUtils.UnZip(paths[0], tmpDirPath);
            }
            catch (Exception)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "文件不合法", () => { }, "确定");
                return null;
            }
            if (!File.Exists(tmpDirPath + "/chara") || !File.Exists(tmpDirPath + "/index.txt"))
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "文件格式不正确", () => { }, "确定");
                return null;
            }

            string[] lines = File.ReadAllLines(tmpDirPath + "/index.txt");
            if (lines.Length < 3)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "文件格式不正确", () => { }, "确定");
                return null;
            }

            if (!float.TryParse(lines[0], out float pixelsPerUnit) || !float.TryParse(lines[1], out float pivotX) || !float.TryParse(lines[2], out float pivotY))
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "文件格式不正确", () => { }, "确定");
                return null;
            }

            try
            {
                byte[] data = File.ReadAllBytes(tmpDirPath + "/chara");
                Sprite readSprite = ReadSprite(data, new Vector2(pivotX, pivotY), pixelsPerUnit);
                PlayerPrefs.SetString("character", string.Join(lines[0], lines[1], lines[2], Convert.ToBase64String(data)));
                PlayerPrefs.Save();
                return readSprite;
            }
            catch (UnimageException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "文件格式不正确", () => { }, "确定");
                return null;
            }
        }

        private Sprite ReadSprite(byte[] data, Vector2 pivot, float pixelsPerUnit = 100f)
        {
            using UnimageProcessor unimageProcessor = new UnimageProcessor();
            unimageProcessor.Load(data);
            Texture2D texture = unimageProcessor.GetTexture(noLongerReadable: false);
            return ReadSprite(texture, pivot, pixelsPerUnit);
        }

        private Sprite ReadSprite(Texture2D texture2D, Vector2 pivot, float pixelsPerUnit = 100f)
        {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), pivot, pixelsPerUnit, 1);
        }
    }
}