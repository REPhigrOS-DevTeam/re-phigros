using System;
using System.Collections;
using System.IO;
using MainCore.Common;
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

        [SerializeField] private Animator animator;
        private static readonly int Loaded = Animator.StringToHash("Loaded");

        public static string Charter { get; set; } = "Unknown";
        public static string Composer { get; set; } = "Unknown";
        public static string Illustrator { get; set; } = "Unknown";


        // Start is called before the first frame update
        void Start()
        {
            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
            {
                GlobalSetting.ChartName = "♡枇杷树上挂♡粒粒油滴下♡让我们一起守护最好的枇杷油♡";
                GlobalSetting.Difficulty = "枇杷油嘿嘿枇杷油";
            }
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
            {
                GlobalSetting.ChartName = "夜夜爱的嗫中毒";
                GlobalSetting.Difficulty = "夜夜ღ醉可爱";
            }

            chartName.text = GlobalSetting.ChartName;

            songCover.sprite = GlobalSetting.BackgroundImage;
            backgroundImage.sprite = GlobalSetting.BackgroundImage;

            level.text = GlobalSetting.Difficulty;

            Charter = GlobalSetting.Charter;
            Composer = GlobalSetting.Composer;
            Illustrator = GlobalSetting.Illustrator;

            charter.text = Charter;
            composer.text = Composer;
            illustrator.text = Illustrator;

            StartCoroutine(YieldDoAnimation());
        }

        IEnumerator YieldDoAnimation()
        {
            yield return new WaitForSeconds(1);
            SceneTransit.Instance.ReplaceScene("PlayingScene");
            var operation = SceneManager.LoadSceneAsync("PlayingScene");
            operation.allowSceneActivation = false;
            yield return new WaitForSeconds(3);
            while (operation.progress < .9f)
            {
                yield return new WaitForSeconds(.2f);
            }
            animator.SetBool(Loaded, true);
            yield return new WaitForSeconds(1.5f);
            operation.allowSceneActivation = true;
        }
    }
}