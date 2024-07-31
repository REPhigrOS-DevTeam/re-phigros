using System;
using LeTai.Asset.TranslucentImage;
using MainCore.Common;
using MainCore.Utilities;
using Network.Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore
{
    public class PhigrosResultManager : MonoBehaviour
    {
        public GameObject[] ranks = new GameObject[10];
        public Text playerName;

        // Start is called before the first frame update
        void Start()
        {
            if (GlobalSetting.DisableBlur)
            {
                GameObject.Find("UICamera").GetComponent<TranslucentImageSource>().enabled = false;
            }

            int lastScore = 0;
            try
            {
                lastScore = SaveManager.GetScore(GlobalSetting.Chart);
            }
            catch
            {
                // ignored
            }

            int deltaScore = Mathf.RoundToInt(GlobalSetting.ScoreCounter.Score) - lastScore;
            acc = (GlobalSetting.ScoreCounter.Accuracy * 100f).ToString("0.00") + "%";
            score = Mathf.RoundToInt(GlobalSetting.ScoreCounter.Score).ToString().PadLeft(7, '0');
            GameObject.Find("SongsName").GetComponent<Text>().text = GlobalSetting.ChartName;
            GameObject.Find("Perfect").GetComponent<Text>().text = GlobalSetting.ScoreCounter.PerfectCnt.ToString();
            GameObject.Find("Good").GetComponent<Text>().text = GlobalSetting.ScoreCounter.GoodCnt.ToString();
            GameObject.Find("Bad").GetComponent<Text>().text = GlobalSetting.ScoreCounter.BadCnt.ToString();
            GameObject.Find("Miss").GetComponent<Text>().text = GlobalSetting.ScoreCounter.MissCnt.ToString();
            GameObject.Find("Accuracy").GetComponent<Text>().text = acc;
            GameObject.Find("ScoreText").GetComponent<Text>().text = score;

            Text history = GameObject.Find("History").GetComponent<Text>();
            Text other = GameObject.Find("Other").GetComponent<Text>();
            bool usePitch = MathF.Round(Mathf.Abs(GlobalSetting.Pitch - 1f), 2) >= 0.01f;
            bool isSlower = usePitch && GlobalSetting.Pitch < 1f;
            if (GlobalSetting.AutoPlay)
                history.text = "1000000   <color=red>AUTO PLAY</color>";
            else if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
                history.text = "枇杷油单推！ --音楽ゲームちゃん";
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
                history.text = "夜々は俺の嫁！ --kagari939";
            else if (GlobalSetting.NewScoreCalcType)
                history.text =
                    "SCORE V2";
            else if (deltaScore > 0)
                history.text =
                    $"NEW BEST   {lastScore.ToString().PadLeft(7, '0')}  +{deltaScore.ToString().PadLeft(7, '0')}";
            else
                history.text = "";
            GameObject.Find("StrictMode").SetActive(GlobalSetting.StrictJudgeMode);
            if (GlobalSetting.NewScoreCalcType || isSlower)
            {
                other.text = $"UNRECORDED   [x{GlobalSetting.Pitch:0.00}]";
            }
            else if (usePitch)
            {
                other.text = $"DT  [x{GlobalSetting.Pitch:0.00}]";
            }
            else
            {
                other.text = "";
            }

            GameObject.Find("MaxCombo").GetComponent<Text>().text = GlobalSetting.ScoreCounter.Maxcombo.ToString();
            GameObject.Find("Difficulty").GetComponent<Text>().text = GlobalSetting.Difficulty;
            GameObject.Find("CoverImage").GetComponent<Image>().sprite = GlobalSetting.BackgroundImage;
            GameObject.Find("Translucent Image").GetComponent<Image>().sprite = GlobalSetting.BackgroundImage;
            GameObject.Find("Early").GetComponent<Text>().text = GlobalSetting.ScoreCounter.Early.ToString();
            GameObject.Find("Late").GetComponent<Text>().text = GlobalSetting.ScoreCounter.Late.ToString();
            if (!GlobalSetting.AutoPlay && !isSlower && !GlobalSetting.NewScoreCalcType)
                SaveManager.SaveScore(GlobalSetting.Chart,
                    Mathf.RoundToInt(GlobalSetting.ScoreCounter.Score).ToString().PadLeft(7, '0'));
            getRank(GlobalSetting.ScoreCounter.Score);
            PlayerPrefs.Save();
            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
            {
                playerName.text = "Poyoroid躁 & 音楽ゲームちゃん";
            }
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
            {
                playerName.text = "Yaya & kagari939";
            }
            else
            {
                playerName.text = PlayerPrefs.GetString("player_name", "kagari939");
            }
        }

        private string score;
        private string acc;

        private void getRank(float scoreNum)
        {
            foreach (GameObject o in ranks)
            {
                if (o) o.SetActive(false);
            }

            if (GlobalSetting.LineStat == JudgeLineStat.FC)
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
            if (GlobalSetting.IsMultiplayer)
            {
                SocketManager.EndGame(score, acc);
            }

            GC.Collect();
            SceneTransit.Instance.Back();
        }

        public void RetryButtonClicked()
        {
            GlobalSetting.Reset();
            //SceneManager.LoadSceneAsync("PlayingScene");
        
            GC.Collect();
            SceneTransit.Instance.JumpScene("PlayingScene");
        }
    }
}