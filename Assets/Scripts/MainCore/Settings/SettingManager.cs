using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Lean.Gui;
using MainCore.Common;
using MainCore.Data;
using MainCore.UI;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

namespace MainCore.Settings
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField] private LeanButton saveNExit;
        [SerializeField] private LeanButton dspEnter;
        [SerializeField] private InputField_File_Selector dataPath;
        [SerializeField] private Transform broadCastTarget;
        [SerializeField] private LeanToggle[] toggles;
        [SerializeField] private Dropdown skinDropdown;
        [SerializeField] private DelayCorrect delayCorrect;

        void Start()
        {
            if (!PlayerPrefs.HasKey(dataPath.BaseData.DataTag))
            {
                dataPath.BaseData.SetValue($"{Application.persistentDataPath}");
            }
#if UNITY_IPHONE && !UNITY_EDITOR
            dataPath.Lock();
#endif
            skinDropdown.onValueChanged.AddListener(OnSkinChanged);
            saveNExit.OnClick.AddListener(SaveNExit);
            dspEnter.OnClick.AddListener(IntoDSP);
            SpecialEvent caiDan1 = new SpecialEvent(toggles,
                new[]
                {
                    (int)YayaModeInSettings.吹鸣, (int)YayaModeInSettings.森闲, (int)YayaModeInSettings.光焰,
                    (int)YayaModeInSettings.天崄
                }, () =>
                {
                    Debug.Log("结！");
                    InGameUIManager.ShowModalWindowWithClose("<size=15>再去主界面标题标题点十下</size>", "这都能被你发现", () => { },
                        "芜湖~");
                    GlobalSetting.YayaKawaii = GlobalSetting.YayaMode.结;
                });
            SpecialEvent caiDan2 = new SpecialEvent(toggles,
                new[]
                {
                    (int)PepoyoModeInSettings.Jikkentai, (int)PepoyoModeInSettings.Neurose,
                    (int)PepoyoModeInSettings.Waraninja, (int)PepoyoModeInSettings.Anrakushi
                }, () =>
                {
                    Debug.Log("躁！");
                    InGameUIManager.ShowModalWindowWithClose("<size=15>再去主界面标题标题点十下</size>", "这也被你发现啦", () => { },
                        "枇杷树上挂 粒粒油滴下");
                    GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Poyoroid_sou;
                });
            // SpecialEvent qiYongExtraJson = new SpecialEvent(toggles, new[] { 5, 6, 4, 8 }, () => { });
            OnSkinChanged(skinDropdown.value);
#if !RELEASE_VERSION || UNITY_EDITOR
            skinDropdown.AddOptions(new List<string> { "Phira", "萨卡斑甲鱼" });
#endif
        }

        private void IntoDSP()
        {
            string? qwq = null;
            if (PlayerPrefs.HasKey(dataPath.BaseData.DataTag)) qwq = PlayerPrefs.GetString(dataPath.BaseData.DataTag);
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.Save();
            if (qwq != null) PlayerPrefs.SetString(dataPath.BaseData.DataTag, qwq);
            SceneTransit.Instance.LoadScene("DSPScene");
        }

        private void SaveNExit()
        {
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.Save();
#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.SetString("file_path", Application.persistentDataPath);
            PlayerPrefs.Save();

            if (!Directory.Exists(PlayerPrefs.GetString("file_path", Application.persistentDataPath)))
            {
                InGameUIManager.ShowModalWindowWithClose("故意的是吧", "你这文件夹都不存在啊", () => { }, "确认");
                return;
            }
#endif

            SceneTransit.Instance.Back();
        }

        private void OnSkinChanged(int id)
        {
            GlobalSetting.Skin = (Skin)id;
            HitSoundManager.Instance.RefreshHitSounds(GlobalSetting.Skin);
            delayCorrect.OnSkinChanged();
        }

        private string[] constantSkinFile =
        {
            "info.yml", "click.png", "click_mh.png", "drag.png", "drag_mh.png", "flick.png", "flick_mh.png", "hold.png",
            "hold_mh.png", "hit_fx.png"
        };

        private async UniTask<SkinInfo> ReadSkinPackage(string path)
        {
            string tmpDirPath = Application.temporaryCachePath + "/tmpSkinPackage";
            string dirPath = $"{tmpDirPath}/{Path.GetFileNameWithoutExtension(path)}";
            if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);
            try
            {
                ZipUtils.UnZip(File.ReadAllBytes(path), tmpDirPath + $"/{Path.GetFileNameWithoutExtension(path)}");
            }
            catch (IOException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "无法读取文件", () => { }, "确定");
                return null;
            }

            List<string> files = Directory.GetFiles(dirPath).Select(Path.GetFileName).ToList();
            if (constantSkinFile.Any(s => !files.Contains(s)))
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "资源包不完整", () => { }, "确定");
                return null;
            }

            PhiraSkinInfoData phiraSkinInfoData;
            try
            {
                IDeserializer deserializer = new DeserializerBuilder().Build();
                phiraSkinInfoData =
                    deserializer.Deserialize<PhiraSkinInfoData>(await File.ReadAllTextAsync($"{dirPath}/info.yml"));
            }
            catch (IOException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "无法读取文件", () => { }, "确定");
                return null;
            }
            catch (Exception)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "info.yml格式不正确", () => { }, "确定");
                return null;
            }

            if (phiraSkinInfoData.name == null || phiraSkinInfoData.author == null ||
                phiraSkinInfoData.hitFx == null || phiraSkinInfoData.holdAtlas == null ||
                phiraSkinInfoData.holdAtlasMH == null || phiraSkinInfoData.hitFx.Length != 2 ||
                phiraSkinInfoData.holdAtlas.Length != 2 || phiraSkinInfoData.holdAtlasMH.Length != 2)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "info.yml不完整", () => { }, "确定");
                return null;
            }

            SkinInfo defaultSkinInfo = HitEffectManager.GetSkinInfo(Skin.Official);
            SkinInfo skinInfo = ScriptableObject.CreateInstance<SkinInfo>();
            skinInfo.skinName = Path.GetFileNameWithoutExtension(path);
            skinInfo.author = phiraSkinInfoData.author;
            skinInfo.description = phiraSkinInfoData.desciption == "" ? "无" : phiraSkinInfoData.desciption;
            skinInfo.hitFxDuration = phiraSkinInfoData.hitFxDuration;
            skinInfo.hitFxScale = phiraSkinInfoData.hitFxScale;
            skinInfo.hitFxRotate = phiraSkinInfoData.hitFxRotate;
            skinInfo.hitFxTinted = phiraSkinInfoData.hitFxTinted;
            skinInfo.hideParticles = phiraSkinInfoData.hideParticles;
            skinInfo.holdKeepHead = phiraSkinInfoData.holdKeepHead;
            skinInfo.holdRepeat = phiraSkinInfoData.holdRepeat;
            skinInfo.holdCompact = phiraSkinInfoData.holdCompact;
            // 读取Hit Fx
            List<Sprite> hitFx = new List<Sprite>();
            int numColumns = phiraSkinInfoData.hitFx[0];
            int numRows = phiraSkinInfoData.hitFx[1];
            Texture2D hitFxTexture = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/hit_fx.png"));
            if (hitFxTexture.width % numColumns != 0 || hitFxTexture.height % numRows != 0)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "打击特效图片长宽与info.yml的内容不匹配", () => { }, "确定");
                return null;
            }

            Vector2 spriteSize =
                new Vector2(hitFxTexture.width / numColumns, hitFxTexture.height / numRows); // 按列和行切割图片

            for (int y = 0; y < numRows; ++y)
            {
                for (int x = 0; x < numColumns; ++x)
                {
                    Rect rect = new Rect(x * spriteSize.x, y * spriteSize.y, spriteSize.x, spriteSize.y);

                    // TODO: PPU计算
                    Sprite sprite =
                        Sprite.Create(hitFxTexture, rect, new Vector2(0.5f, 0.5f), 100f, 1); // 创建新的 Sprite 对象
                    sprite.name = $"hit_fx_external_{y * numColumns + x}"; // 自定义 Sprite 的名字

                    hitFx.Add(sprite);
                }
            }

            skinInfo.hitFx = hitFx.ToArray();
            // 读取note们
            skinInfo.click_bad =
                skinInfo.click = Util.ReadFileAsSprite(await File.ReadAllBytesAsync($"{dirPath}/click.png"), out _);
            skinInfo.clickMh = Util.ReadFileAsSprite(await File.ReadAllBytesAsync($"{dirPath}/click_mh.png"), out _);
            Texture2D dragTex = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/drag.png"));
            skinInfo.drag = Sprite.Create(dragTex, new Rect(0, 0, dragTex.width, dragTex.height),
                new Vector2(0.5f, 0.5f), skinInfo.click.pixelsPerUnit * dragTex.width / skinInfo.click.rect.width, 1);
            Texture2D dragMhTex = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/drag_mh.png"));
            skinInfo.dragMh = Sprite.Create(dragMhTex, new Rect(0, 0, dragMhTex.width, dragMhTex.height),
                new Vector2(0.5f, 0.5f), skinInfo.clickMh.pixelsPerUnit * dragMhTex.width / skinInfo.clickMh.rect.width,
                1);
            Texture2D flickTex = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/flick.png"));
            skinInfo.flick = Sprite.Create(flickTex, new Rect(0, 0, flickTex.width, flickTex.height),
                new Vector2(0.5f, 0.5f), skinInfo.click.pixelsPerUnit * flickTex.width / skinInfo.click.rect.width, 1);
            Texture2D flickMhTex = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/flick_mh.png"));
            skinInfo.flickMh = Sprite.Create(flickMhTex, new Rect(0, 0, flickMhTex.width, flickMhTex.height),
                new Vector2(0.5f, 0.5f),
                skinInfo.clickMh.pixelsPerUnit * flickMhTex.width / skinInfo.clickMh.rect.width,
                1);
            try
            {
                // hold
                Texture2D holdTexture = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/hold.png"));
                Sprite[] holdSprites = SplitTexture("hold", holdTexture, phiraSkinInfoData.holdAtlas[0],
                    phiraSkinInfoData.holdAtlas[1], skinInfo.click.pixelsPerUnit, skinInfo.click.rect.width);
                skinInfo.holdHead = holdSprites[0];
                skinInfo.holdBody = holdSprites[1];
                skinInfo.holdEnd = holdSprites[2];
                skinInfo.holdLengthFactor = skinInfo.holdBody.rect.height / skinInfo.holdBody.pixelsPerUnit;
                Texture2D holdMhTexture = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/hold_mh.png"));
                Sprite[] holdMhSprites = SplitTexture("hold_mh", holdMhTexture, phiraSkinInfoData.holdAtlasMH[0],
                    phiraSkinInfoData.holdAtlasMH[1], skinInfo.clickMh.pixelsPerUnit, skinInfo.clickMh.rect.width);
                skinInfo.holdHeadMh = holdMhSprites[0];
                skinInfo.holdBodyMh = holdMhSprites[1];
                skinInfo.holdEndMh = holdMhSprites[2];
                skinInfo.hitParticle = HitEffectManager.GetSkinInfo(Skin.Phira).hitParticle;
                skinInfo.holdMhLengthFactor = skinInfo.holdBodyMh.rect.height / skinInfo.holdBodyMh.pixelsPerUnit;
            }
            catch (ArgumentException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "Hold贴图高度不足", () => {}, "确定");
                return null;
            }

            skinInfo.clickAC = File.Exists($"{dirPath}/click.ogg")
                ? await Util.ReadMusicAsAudioClip($"{dirPath}/click.ogg")
                : defaultSkinInfo.clickAC;
            skinInfo.dragAC = File.Exists($"{dirPath}/drag.ogg")
                ? await Util.ReadMusicAsAudioClip($"{dirPath}/drag.ogg")
                : defaultSkinInfo.dragAC;
            skinInfo.flickAC = File.Exists($"{dirPath}/flick.ogg")
                ? await Util.ReadMusicAsAudioClip($"{dirPath}/flick.ogg")
                : defaultSkinInfo.flickAC;
            return skinInfo;
        }

        Sprite[] SplitTexture(string namePrefix, Texture2D texture, int startPixel, int endPixel, float clickPpu,
            float clickWidth)
        {
            if (startPixel + endPixel > texture.height)
            {
                throw new ArgumentException();
            }

            float ppu = clickPpu * texture.width / clickWidth;
            Sprite head = Sprite.Create(texture, new Rect(0f, 0f, texture.width, startPixel), new Vector2(0.5f, 1f),
                ppu, 1);
            Sprite body = Sprite.Create(texture,
                new Rect(0f, startPixel, texture.width, texture.height - startPixel - endPixel), new Vector2(0.5f, 1f),
                ppu, 1);
            Sprite end = Sprite.Create(texture, new Rect(0f, texture.height - endPixel, texture.width, endPixel),
                new Vector2(0.5f, 0f), ppu, 1);
            head.name = $"{namePrefix}_head";
            body.name = $"{namePrefix}_body";
            end.name = $"{namePrefix}_end";
            return new[] { head, body, end };
        }
    }

    public enum YayaModeInSettings
    {
        吹鸣 = 3,
        森闲 = 1,
        光焰 = 2,
        天崄 = 6
    }

    public enum PepoyoModeInSettings
    {
        Jikkentai = 6,
        Neurose = 5,
        Waraninja = 7,
        Anrakushi = 3
    }
}