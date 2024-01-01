#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using MainCore.Common;
using MainCore.Data;
using MainCore.UI;
using MainCore.UI.Utils;
using MainCore.Utilities;
using Newtonsoft.Json;
using SFB;
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
        ifPpu.onEndEdit += _ => OnValueChanged();
        ifPivotX.onEndEdit += _ => OnValueChanged();
        ifPivotY.onEndEdit += _ => OnValueChanged();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("customCharacter"))
        {
            CharacterImage characterImage =
                Util.GetCharacterImage(Convert.FromBase64String(PlayerPrefs.GetString("customCharacter")));
            ifPpu.Value = characterImage.Info.PixelsPerUnit;
            ifPivotX.Value = characterImage.Info.PivotX;
            ifPivotY.Value = characterImage.Info.PivotY;
            spriteRenderer.sprite = characterImage.Sprite;
        }
        else
        {
            spriteRenderer.sprite = defaultCharacter;
            ifPpu.Value = defaultCharacter.pixelsPerUnit;
            ifPivotX.Value = defaultCharacter.pivot.x / defaultCharacter.texture.width;
            ifPivotY.Value = defaultCharacter.pivot.y / defaultCharacter.texture.height;
        }

        panelSwitch.IsOn = true;
        SetInput(PlayerPrefs.HasKey("customCharacter"));
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
        if (characterTextureData == null) return;

        OpenFile.SaveFile(OnExportPathSelected, () => { }, new []{new ExtensionFilter("REP角色包", "charapkg")},
            Util.DataPath, "保存到...", "确定");
    }

    private void OnExportPathSelected(string path)
    {
        try
        {
            string tmpPkgPath = Application.temporaryCachePath + "/tmpCharaPkg.zip";
            if (File.Exists(tmpPkgPath)) File.Delete(tmpPkgPath);
            ExternalCharacterInfo externalCharacterInfo = new ExternalCharacterInfo
            {
                Id = "", // TODO
                PixelsPerUnit = ifPpu.Value,
                PivotX = ifPivotX.Value,
                PivotY = ifPivotY.Value,
            };
            byte[] configData =
                new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(externalCharacterInfo, Formatting.None));
            FileStream fileStream = new FileStream(tmpPkgPath, FileMode.Create, FileAccess.Write, FileShare.None);
            ZipOutputStream zipOutputStream =
                new ZipOutputStream(fileStream, StringCodec.FromEncoding(new UTF8Encoding(false)));
            Crc32 crc32 = new Crc32();
            DateTime now = DateTime.Now;
            PutEntry("chara", characterTextureData);
            PutEntry("index.json", configData);
            byte[] imageHash = FileEncryptor.ComputeSha256(characterTextureData);
            byte[] configHash = FileEncryptor.ComputeSha256(configData);
            List<byte> hashList = new List<byte>();
            hashList.AddRange(imageHash);
            hashList.AddRange(configHash);
            byte[] encryptedHash = FileEncryptor.RsaEncrypt(hashList.ToArray());
            PutEntry("hash", encryptedHash);
            InGameUIManager.ShowModalWindowWithClose("提示", "成功导出", () => { }, "确认");

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

            zipOutputStream.Close();
            fileStream.Close();
            File.WriteAllBytes(path, FileEncryptor.Encrypt(File.ReadAllBytes(tmpPkgPath)));
            File.Delete(tmpPkgPath);
        }
        catch (IOException e)
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "请检查要覆盖的输出文件是否被占用", () => { }, "确定");
            Debug.LogException(e);
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
        OpenFile.LoadFile(path =>
        {
            SetInput(true);
            OnImageSelected(path);
        }, () => { }, new []{new ExtensionFilter("立绘贴图", "png"), new ExtensionFilter("REP角色包", "charapkg")}, null, "选择人物...", "确定");
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
                // 对png文件头和文件尾进行校验，只截取png部分，防止png头尾前后数据注入造成传播奇怪的文件或hash冲突（虽然sha256冲突不起来）
                long[] headers = data.IndexOfByBoyerMooreHorspool(new byte[] { 0x89, 0x50, 0x4E, 0x47 }).ToArray();
                long[] ends = data.IndexOfByBoyerMooreHorspool(new byte[] { 0xAE, 0x42, 0x60, 0x82 }).ToArray();
                if (headers.Length + ends.Length > 2)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", "立绘文件不合法", () => { }, "确定");
                    return;
                }

                data = data.Skip((int)headers[0]).Take((int)(ends[0] - headers[0] + 4L)).ToArray(); // 检查完毕
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
                    ifPpu.Value = characterImage.Info.PixelsPerUnit;
                    ifPivotX.Value = characterImage.Info.PivotX;
                    ifPivotY.Value = characterImage.Info.PivotY;
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