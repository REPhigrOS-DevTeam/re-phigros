using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using MainCore.Common;
using MainCore.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace MainCore.Settings
{
    public class SkinManager : MonoSingleton<SkinManager>
    {
        public AudioClip defaultClickAC, defaultDragAC, defaultFlickAC;
        public Sprite defaultParticle;
        private Dictionary<string, SkinInfo> externalSkinInfos = new Dictionary<string, SkinInfo>();
        public string SkinPath => GetBasePath() + "/Skins";

        public bool Initialized { get; private set; }

        private static string GetBasePath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "";
                case RuntimePlatform.WindowsPlayer:
                    return new DirectoryInfo(Application.dataPath + "/..").FullName; // exe所在目录
                case RuntimePlatform.WindowsEditor:
                    return Application.persistentDataPath; // 这玩意儿在用户的AppData\LocalLow里
                case RuntimePlatform.IPhonePlayer:
                    return new DirectoryInfo(Application.temporaryCachePath + "/..").FullName; // 沙盒下/Library
                case RuntimePlatform.Android:
                    return Application.persistentDataPath;
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override async void OnAwake()
        {
            Debug.Log("[SkinManager] Loading skin infos...");
            if (Directory.Exists(SkinPath) && !File.Exists(SkinPath + "/info.json")) Directory.Delete(SkinPath, true);
            Directory.CreateDirectory(SkinPath);
            if (!File.Exists(SkinPath + "/info.json"))
            {
                await File.WriteAllBytesAsync(SkinPath + "/info.json", new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(new Skins(), Formatting.None)));
            }
            else
            {
                await ReadLocalSkins();
            }

            LoadCurrentSkinInfo();
            Initialized = true;
            Debug.Log($"[SkinManager] Done. Current skin: {GlobalSetting.CurrentSkinInfo.skinName}");
        }

        private async UniTask ReadLocalSkins()
        {
            var skins = JsonConvert.DeserializeObject<Skins>(await File.ReadAllTextAsync(SkinPath + "/info.json"));
            foreach (var skinSummary in skins.SkinSummaries)
            {
                if (!Directory.Exists($"{SkinPath}/{skinSummary.ID}")) continue;
                var skinInfo = await GameUtils.ReadSkin($"{SkinPath}/{skinSummary.ID}");
                skinInfo.id = skinSummary.ID;
                externalSkinInfos.Add(skinSummary.ID, skinInfo);
            }
        }

        public void AddSkinInfo(string tempPath, SkinInfo skinInfo)
        {
            if (!skinInfo.isExternal) return;
            var skins = JsonConvert.DeserializeObject<Skins>(File.ReadAllText(SkinPath + "/info.json"));
            var skinSummaries = skins.SkinSummaries.ToList();
            var guid = Guid.NewGuid().ToString();
            while (skinSummaries.Select(skinSummary => skinSummary.ID).ToList().Contains(guid)) guid = Guid.NewGuid().ToString();
            skinSummaries.Add(new SkinSummary
            {
                ID = guid,
                Name = skinInfo.skinName
            });
            skins.SkinSummaries = skinSummaries.ToArray();
            File.WriteAllBytes(SkinPath + "/info.json", new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(skins, Formatting.None)));
            Directory.CreateDirectory($"{SkinPath}/{guid}");
            Util.CopyAll(new DirectoryInfo(tempPath), new DirectoryInfo($"{SkinPath}/{guid}"));
            skinInfo.id = guid;
            externalSkinInfos.Add(guid, skinInfo);
            Directory.Delete(tempPath, true);
        }

        public void DeleteSkinInfo(string id)
        {
            var skins = JsonConvert.DeserializeObject<Skins>(File.ReadAllText(SkinPath + "/info.json"));
            var skinSummaries = skins.SkinSummaries.ToList();
            skinSummaries.Remove(skinSummaries.Find(skinSummary => skinSummary.ID == id));
            skins.SkinSummaries = skinSummaries.ToArray();
            File.WriteAllBytes(SkinPath + "/info.json", new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(skins, Formatting.None)));
            if (Directory.Exists($"{SkinPath}/{id}"))
            {
                Directory.Delete($"{SkinPath}/{id}", true);
            }
            externalSkinInfos.Remove(id);
        }

        private void LoadCurrentSkinInfo()
        {
            if (PlayerPrefs.HasKey("skin")) // 给前人擦屁股.jpg
            {
                var skin = PlayerPrefs.GetInt("skin", 0);
                GlobalSetting.CurrentSkinInfo = HitEffectManager.GetInstance().GetInternalSkinInfo((Skin)skin);
                PlayerPrefs.DeleteKey("skin");
                PlayerPrefs.SetString("selected_skin", $"i{skin}");
                PlayerPrefs.Save();
            }
            else
            {
                var s = PlayerPrefs.GetString("selected_skin", "i0");
                GlobalSetting.CurrentSkinInfo = HitEffectManager.GetInstance().GetSkinInfo(s[0] switch // internal external
                {
                    'i' => false,
                    'e' => true,
                    _ => throw new ArgumentException()
                }, s[1..]);
                if (GlobalSetting.CurrentSkinInfo)
                {
                    return;
                }
                if (s[0] != 'e') throw new ArgumentException();
                PlayerPrefs.SetString("selected_skin", "i0");
                GlobalSetting.CurrentSkinInfo = HitEffectManager.GetInstance().GetSkinInfo(false, "0");
                PlayerPrefs.Save();
            }
            HitSoundManager.Instance.RefreshHitSounds();
        }

        public void SaveCurrentSkinInfo()
        {
            PlayerPrefs.SetString("selected_skin",
                GlobalSetting.CurrentSkinInfo.isExternal
                    ? $"e{GlobalSetting.CurrentSkinInfo.id}"
                    : $"i{(int)GlobalSetting.CurrentSkinInfo.skin}");
        }

        public SkinInfo GetExternalSkinInfo(string id) => externalSkinInfos.GetValueOrDefault(id);

        public SkinSummary[] GetSkinSummaries() => JsonConvert.DeserializeObject<Skins>(File.ReadAllText(SkinPath + "/info.json")).SkinSummaries;
    }

    public class Skins
    {
        [JsonProperty("skins")]
        public SkinSummary[] SkinSummaries= Array.Empty<SkinSummary>();
    }

    public class SkinSummary
    {
        [JsonProperty("id")]
        public string ID = "";
        [JsonProperty("name")]
        public string Name = "";
    }
}