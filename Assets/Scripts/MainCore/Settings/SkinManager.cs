using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MainCore;
using MainCore.Common;
using MainCore.Utilities;
using Newtonsoft.Json;
using UnityEngine;

public class SkinManager : MonoSingleton<SkinManager>
{
    public AudioClip defaultClickAC, defaultDragAC, defaultFlickAC;

    private Dictionary<string, SkinInfo> externalSkinInfos = new Dictionary<string, SkinInfo>();

    public string skinPath;

    private string GetBasePath()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                return "";
            case RuntimePlatform.WindowsPlayer:
                return new DirectoryInfo(Application.dataPath + "/..").FullName; // exe所在目录
            case RuntimePlatform.WindowsEditor:
                return Application.persistentDataPath; // 这玩意儿在用户的AppData\Low里
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

    protected override void OnAwake()
    {
        skinPath = GetBasePath() + "/Skins";
        if (Directory.Exists(skinPath)) Directory.Delete(skinPath, true);
        Directory.CreateDirectory(skinPath);
        if (!File.Exists(skinPath + "/info.json"))
        {
            File.WriteAllBytes(skinPath + "/info.json", new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(new Skins())));
        }
        else
        {
            ReadLocalSkins();
        }
    }

    private async void ReadLocalSkins()
    {
        Skins skins = JsonConvert.DeserializeObject<Skins>(skinPath + "/info.json");
        foreach (SkinSummary skinSummary in skins.skins)
        {
            externalSkinInfos.Add(skinSummary.id, await GameUtils.ReadSkin($"{skinPath}/{skinSummary.id}"));
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddSkinInfo(string tempPath, SkinInfo skinInfo)
    {
        if (!skinInfo.isExternal) return;
        Skins skins = JsonConvert.DeserializeObject<Skins>(File.ReadAllText(skinPath + "/info.json"));
        List<SkinSummary> skinSummaries = skins.skins.ToList();
        string s = Guid.NewGuid().ToString();
        while (skinSummaries.Select(skinSummary => skinSummary.id).ToList().Contains(s)) s = Guid.NewGuid().ToString();
        skinSummaries.Add(new SkinSummary
        {
            id = s,
            name = skinInfo.skinName
        });
        skins.skins = skinSummaries.ToArray();
        File.WriteAllBytes(skinPath + "/info.json", new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(skins)));
        Util.CopyAll(new DirectoryInfo(tempPath), new DirectoryInfo($"{skinPath}/{s}"));
        externalSkinInfos.Add(s, skinInfo);
    }

    public void DeleteSkinInfo(string id)
    {
        Skins skins = JsonConvert.DeserializeObject<Skins>(File.ReadAllText(skinPath + "/info.json"));
        List<SkinSummary> skinSummaries = skins.skins.ToList();
        skinSummaries.Remove(skinSummaries.Find(skinSummary => skinSummary.id == id));
        skins.skins = skinSummaries.ToArray();
        File.WriteAllBytes(skinPath + "/info.json", new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(skins)));
        Directory.Delete($"{skinPath}/{id}", true);
        externalSkinInfos.Remove(id);
    }

    public SkinInfo GetExternalSkinInfo(string id)
    {
        return externalSkinInfos.ContainsKey(id) ? externalSkinInfos[id] : null;
    }

    public SkinSummary[] GetSkinSummaries() => JsonConvert.DeserializeObject<Skins>(File.ReadAllText(skinPath + "info.json")).skins;
}

public class Skins
{
    [JsonProperty("skins")]
    public SkinSummary[] skins;
}

public class SkinSummary
{
    [JsonProperty("id")]
    public string id = "";
    [JsonProperty("name")]
    public string name = "";
}
