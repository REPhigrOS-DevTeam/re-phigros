using LeTai.Asset.TranslucentImage;
using MainCore;
using MainCore.Common;
using MainCore.Utilities;
using Network.Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

public class PhigrosResultManager : MonoBehaviour
{
    public GameObject[] ranks = new GameObject[10];
    public Text playerName;
    public Text versionText;

    // Start is called before the first frame update
    void Start()
    {
        if (GlobalSetting.disableBlur)
        {
            GameObject.Find("UICamera").GetComponent<TranslucentImageSource>().enabled = false;
        }

        int lastScore = 0;
        try
        {
            lastScore = SaveManager.GetScore(GlobalSetting.chart);
        }
        catch
        {
            // ignored
        }

        int deltaScore = Mathf.RoundToInt(GlobalSetting.scoreCounter.Score) - lastScore;
        acc = (GlobalSetting.scoreCounter.Accuracy * 100f).ToString("0.00") + "%";
        score = Mathf.RoundToInt(GlobalSetting.scoreCounter.Score).ToString().PadLeft(7, '0');
        GameObject.Find("SongsName").GetComponent<Text>().text = GlobalSetting.chartName;
        GameObject.Find("Perfect").GetComponent<Text>().text = GlobalSetting.scoreCounter.perfectCnt.ToString();
        GameObject.Find("Good").GetComponent<Text>().text = GlobalSetting.scoreCounter.goodCnt.ToString();
        GameObject.Find("Bad").GetComponent<Text>().text = GlobalSetting.scoreCounter.badCnt.ToString();
        GameObject.Find("Miss").GetComponent<Text>().text = GlobalSetting.scoreCounter.missCnt.ToString();
        GameObject.Find("Accuracy").GetComponent<Text>().text = acc;
        GameObject.Find("ScoreText").GetComponent<Text>().text = score;

        if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
            GameObject.Find("History").GetComponent<Text>().text = "枇杷油单推！ --音楽ゲームちゃん";
        else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
            GameObject.Find("History").GetComponent<Text>().text = "夜々は俺の嫁！ --kagari939";
        else if (GlobalSetting.autoPlay)
            GameObject.Find("History").GetComponent<Text>().text =
                GlobalSetting.recordMode ? "RECORD MODE" : "<color=red>AUTO PLAY</color>";
        else if (deltaScore > 0)
            GameObject.Find("History").GetComponent<Text>().text =
                $"NEW BEST   {lastScore.ToString().PadLeft(7, '0')}  +" + deltaScore.ToString().PadLeft(7, '0');
        else
            GameObject.Find("History").GetComponent<Text>().text = "";
        GameObject.Find("MaxCombo").GetComponent<Text>().text = GlobalSetting.scoreCounter.maxcombo.ToString();
        GameObject.Find("Difficulty").GetComponent<Text>().text = GlobalSetting.difficulty;
        GameObject.Find("CoverImage").GetComponent<Image>().sprite = GlobalSetting.backgroundImage;
        GameObject.Find("Translucent Image").GetComponent<Image>().sprite = GlobalSetting.backgroundImage;
        GameObject.Find("Early").GetComponent<Text>().text = GlobalSetting.scoreCounter.early.ToString();
        GameObject.Find("Late").GetComponent<Text>().text = GlobalSetting.scoreCounter.late.ToString();
        if (!GlobalSetting.autoPlay)
            SaveManager.SaveScore(GlobalSetting.chart,
                Mathf.RoundToInt(GlobalSetting.scoreCounter.Score).ToString().PadLeft(7, '0'));
        getRank(GlobalSetting.scoreCounter.Score);
        PlayerPrefs.Save();
        if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
        {
            playerName.text = "Pepoyo & 音楽ゲームちゃん";
        }
        else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
        {
            playerName.text = "Yaya & kagari939";
        }
        else
        {
            playerName.text = GlobalSetting.recordMode
                ? "RPGR RECORD MODE"
                : PlayerPrefs.GetString("player_name", "kagari939");
        }

        versionText.text = $"RE:Phigros {Application.version} by kagari939\n";
    }

    private string score;
    private string acc;

    private void getRank(float scoreNum)
    {
        if (GlobalSetting.lineStat == JudgeLineStat.FC)
        {
            ranks[7].SetActive(true);
            return;
        }

        int a = Mathf.RoundToInt(scoreNum);
        if (a >= 1e6) ranks[0].SetActive(true);
        else if (a >= 9.6e5) ranks[1].SetActive(true);
        else if (a >= 9.2e5) ranks[2].SetActive(true);
        else if (a >= 8.8e5) ranks[3].SetActive(true);
        else if (a >= 8.2e5) ranks[4].SetActive(true);
        else if (a >= 7e5) ranks[5].SetActive(true);
        else ranks[6].SetActive(true);
    }

    public void NextButtonClicked()
    {
        //SceneManager.LoadSceneAsync("ChartSelectorScene");
        GameObject.Find("MaskImage").GetComponent<Animation>().Play("LevelOverCutOut");
        //StartCoroutine(Utils.SwitchSceneAfterSeconds(2f, "ChartSelectorScene"));
        if (GlobalSetting.isMultiplayer)
        {
            SocketManager.EndGame(score, acc);
            SceneTransit.Instance.TransitTo("NetWorkTest");
        }
        else
        {
            SceneTransit.Instance.TransitTo("ChartSelectorScene");
        }
    }

    public void RetryButtonClicked()
    {
        GlobalSetting.Reset();
        //SceneManager.LoadSceneAsync("PlayingScene");
        SceneTransit.Instance.TransitTo("PlayingScene");
    }
}