#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using MainCore.Common;
using MainCore.UI;
using MainCore.Utilities;
using SimpleFileBrowser;
using Unimage;
using UnityEngine;
using UnityEngine.UI;

public class CharacterAdjustManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultCharacter;
    [SerializeField] private Toggle_Button panelSwitch;
    [SerializeField] private Button selectCharacter, exportCharacter;
    [SerializeField] private InputField_FloatValidation ifPpu, ifPivotX, ifPivotY;
    private byte[] characterTextureData;
    private Texture2D characterTexture;

    private void Awake()
    {
        panelSwitch.OnValueChanged += b =>
        {
            ((RectTransform)panelSwitch.transform.parent.transform).sizeDelta =
                b ? new Vector2(800f, 500f) : new Vector2(100f, 100f);
        };
        selectCharacter.onClick.AddListener(SelectImage);
        exportCharacter.onClick.AddListener(ExportCharacterPackage);
        ifPpu.InputField.onEndEdit.AddListener(_ => OnValueChanged());
        ifPivotX.InputField.onEndEdit.AddListener(_ => OnValueChanged());
        ifPivotY.InputField.onEndEdit.AddListener(_ => OnValueChanged());
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("character"))
        {
            string[] strings = PlayerPrefs.GetString("character").Split("\n");
            ifPpu.Value = float.Parse(strings[0]);
            ifPivotX.Value = float.Parse(strings[1]);
            ifPivotY.Value = float.Parse(strings[2]);
            spriteRenderer.sprite = Util.ReadSprite(Convert.FromBase64String(strings[3]),
                new Vector2(ifPivotX.Value, ifPivotY.Value), ifPpu.Value);
        }
        else
        {
            spriteRenderer.sprite = defaultCharacter;
            ifPpu.Value = defaultCharacter.pixelsPerUnit;
            ifPivotX.Value = defaultCharacter.pivot.x;
            ifPivotY.Value = defaultCharacter.pivot.y;
        }

        panelSwitch.IsOn = true;
        SetInput(PlayerPrefs.HasKey("character"));
    }

    private void SetInput(bool isOn)
    {
        if (ifPpu.InputField.interactable == isOn) return;
        ifPpu.InputField.interactable = isOn;
        ifPivotX.InputField.interactable = isOn;
        ifPivotY.InputField.interactable = isOn;
    }

    private void ExportCharacterPackage()
    {
        if (!characterTexture)
        {
            characterTexture = defaultCharacter.texture;
        }

        ;
        FileBrowser.SetFilters(false, ".charapkg");
        FileBrowser.ShowSaveDialog(paths => OnExportPathSelected(paths[0]), () => { }, FileBrowser.PickMode.Files,
            false, PlayerPrefs.GetString("file_path", Application.persistentDataPath), null, "保存到...", "确定");
    }

    private void OnExportPathSelected(string path)
    {
        try
        {
            using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using ZipOutputStream zipOutputStream =
                new ZipOutputStream(fileStream, StringCodec.FromEncoding(new UTF8Encoding(false)));
            Crc32 crc32 = new Crc32();
            DateTime now = DateTime.Now;
            PutEntry("chara", characterTextureData);
            PutEntry("index.txt",
                new UTF8Encoding(false).GetBytes(string.Join("\n", ifPpu.Value, ifPivotX.Value, ifPivotY.Value)));

            void PutEntry(string name, byte[] data)
            {
                crc32.Reset();
                crc32.Update(data);
                ZipEntry zipEntry = new ZipEntry(name)
                {
                    IsUnicodeText = true,
                    DateTime = now,
                    Size = data.Length,
                    Crc = crc32.Value
                };
                zipOutputStream.PutNextEntry(zipEntry);
                zipOutputStream.Write(data, 0, data.Length);
            }
        }
        catch (Exception e)
        {
            InGameUIManager.ShowModalWindowWithClose("错误", e.Message + "\n" + e.StackTrace, () => { }, "确定");
            Debug.LogException(e);
        }
    }

    private void OnValueChanged()
    {
        spriteRenderer.sprite =
            Util.ReadSprite(characterTexture, new Vector2(ifPivotX.Value, ifPivotY.Value), ifPpu.Value);
    }

    private void SelectImage()
    {
        FileBrowser.SetFilters(false, ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".tga", ".gif", ".charapkg");
        FileBrowser.ShowLoadDialog(paths =>
            {
                SetInput(true);
                OnImageSelected(paths[0]);
            }, () => { }, FileBrowser.PickMode.Files, false,
            PlayerPrefs.GetString("file_path", Application.persistentDataPath), null, "选择人物...", "确定");
    }

    private void OnImageSelected(string path)
    {
        byte[] data;
        Texture2D texture;
        try
        {
            if (!path.EndsWith(".charapkg"))
            {
                data = File.ReadAllBytes(path);
                texture = Util.ReadFileAsTexture(data);
                ifPpu.Reset();
                ifPivotX.Reset();
                ifPivotY.Reset();
            }
            else
            {
                try
                {
                    CharacterImage characterImage = Util.GetCharacterImage(path);
                    data = characterImage.TextureData;
                    texture = characterImage.Texture;
                    ifPpu.Value = characterImage.PixelsPerUnit;
                    ifPivotX.Value = characterImage.Pivot.x;
                    ifPivotY.Value = characterImage.Pivot.y;
                }
                catch (ArgumentException)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", "无法解析立绘包", () => { }, "确定");
                    return;
                }
            }
        }
        catch (IOException)
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "无法读取文件，请检查是否被占用", () => { }, "确定");
            return;
        }
        catch (UnimageException)
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "文件格式未知", () => { }, "确定");
            return;
        }

        characterTextureData = data;
        characterTexture = texture;
        OnValueChanged();
    }

    public void Back()
    {
        SceneTransit.Instance.Back();
    }
}
#endif