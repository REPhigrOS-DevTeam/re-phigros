using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MainCore;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SongReturnCtx
{
    public List<SongReturnItem> songReturnItems;
}

[Serializable]
public class SongReturnItem
{
    public int id;
    public string name;
    public string composer;
    public string illustrator;
    public int type;
    public List<int> charts;
    public int chapter;
    public string url;
}

public class PhigrosSongSearcher : MonoBehaviour
{
    public InputField songName;
    public Dropdown info;
    public Dropdown diffDrop;
    public SongReturnCtx songReturnCtx;
    public Toggle highlightToggle;

    private List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private string GetSong(string @songName)
    {
        var request = (HttpWebRequest) WebRequest.Create("https://dev.phi.zone/get_song/?name=" + @songName);

        var response = (HttpWebResponse) request.GetResponse();

        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        songReturnCtx = JsonUtility.FromJson<SongReturnCtx>("{ \"songReturnItems\": " + responseString + "}");

        return responseString;
    }

    public void Search()
    {
        Debug.Log(GetSong(songName.text));
        optionDatas.Clear();
        foreach (SongReturnItem i in songReturnCtx.songReturnItems)
        {
            optionDatas.Add(new Dropdown.OptionData($"{i.name} - {i.composer} - {i.illustrator}"));
        }

        info.options = optionDatas;
    }

    public void DiffChange()
    {
        int id = info.value;
        GlobalSetting.UsingApi = true;
        if (songReturnCtx.songReturnItems[id].url[songReturnCtx.songReturnItems[id].url.Length - 1] != '/')
            songReturnCtx.songReturnItems[id].url += "/";
        GlobalSetting.ChartName = songReturnCtx.songReturnItems[id].name;
        GlobalSetting.ChartPath =
            songReturnCtx.songReturnItems[id].url + $"Chart_{diffDrop.captionText.text.Trim()}.json";
        GlobalSetting.MusicPath = songReturnCtx.songReturnItems[id].url + "music.wav";
        GlobalSetting.IllustrationPath = songReturnCtx.songReturnItems[id].url + "illustration.png";
    }

    public void OnClick()
    {
        GlobalSetting.HighLight = highlightToggle.isOn;
        GlobalSetting.UserOffset = int.Parse(GameObject.Find("DelayInput").GetComponent<InputField>().text) / 1000f;
        GlobalSetting.AutoPlay = GameObject.Find("AutoToggle").GetComponent<Toggle>().isOn;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("LoadingScene");
    }
}