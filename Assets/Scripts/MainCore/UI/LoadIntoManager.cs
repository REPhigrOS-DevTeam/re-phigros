using System;
using System.Collections;
using System.IO;
using Cysharp.Threading.Tasks;
using MainCore.Common;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class LoadIntoManager : MonoBehaviour
    {
        [SerializeField] private Text chartName;
        [SerializeField] private Text level;
        [SerializeField] private Text charter;
        [SerializeField] private Text illustrator;
        [SerializeField] private Text composer;
        [SerializeField] private Image songCover;
        [SerializeField] private Image backgroundImage;

        void Start()
        {
            PopupMessageManager.Instance.Clear();
            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
            {
                GlobalSetting.CurrentBeatmapInfo.SongName = "♡枇杷树上挂♡粒粒油滴下♡让我们一起守护最好的枇杷油♡";
                GlobalSetting.CurrentBeatmapInfo.SongLevel = "枇杷油嘿嘿枇杷油";
            }
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
            {
                GlobalSetting.CurrentBeatmapInfo.SongName = "夜夜爱的嗫中毒";
                GlobalSetting.CurrentBeatmapInfo.SongLevel = "夜夜ღ醉可爱";
            }

            chartName.text = GlobalSetting.CurrentBeatmapInfo.SongName;

            songCover.sprite = GlobalSetting.CurrentBeatmapInfo.Illustration;
            backgroundImage.sprite = GlobalSetting.CurrentBeatmapInfo.Illustration;

            level.text = GlobalSetting.CurrentBeatmapInfo.SongLevel;

            charter.text = GlobalSetting.CurrentBeatmapInfo.Charter;
            composer.text = GlobalSetting.CurrentBeatmapInfo.Composer;
            illustrator.text = GlobalSetting.CurrentBeatmapInfo.Illustrator;
            
            LoadIn();
        }

        private async void LoadIn()
        {
            await UniTask.Delay(1000);
            SceneTransit.Instance.ReplaceScene("PlayingScene");
            var operation = SceneManager.LoadSceneAsync("PlayingScene");
            operation.allowSceneActivation = false;
            await UniTask.Delay(3000);
            await operation.WaitForSceneLoaded();
            await SceneTransit.PlayQuitAnimation();
            operation.allowSceneActivation = true;
        }
    }
}