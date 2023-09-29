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
        [SerializeField] private Text difficulty;
        [SerializeField] private Text difficultyNumber;
        [SerializeField] private Text charter;
        [SerializeField] private Text illustrator;
        [SerializeField] private Text composer;

        [SerializeField] private Image songCover;
        [SerializeField] private Image backgroundImage;

        [SerializeField] private Animator slideInto;
        [SerializeField] private Animator cutOut;
        [SerializeField] private AudioClip enter;

        public static string Charter { get; set; } = "Unknown";
        public static string Composer { get; set; } = "Unknown";
        public static string Illustrator { get; set; } = "Unknown";


        // Start is called before the first frame update
        void Start()
        {

            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
            {
                GlobalSetting.chartName = "♡枇杷树上挂♡粒粒油滴下♡让我们一起守护最好的枇杷油♡";
                GlobalSetting.difficulty = "枇杷油嘿嘿枇杷油";
            }
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
            {
                GlobalSetting.chartName = "夜夜爱的嗫中毒";
                GlobalSetting.difficulty = "夜夜ღ醉可爱";
            }

            chartName.text = GlobalSetting.chartName;

            songCover.sprite = GlobalSetting.backgroundImage;
            backgroundImage.sprite = GlobalSetting.backgroundImage;

            try //Try Parse difficulty
            {
                difficultyNumber.text =
                    GlobalSetting.difficulty.Substring(GlobalSetting.difficulty.LastIndexOf('.') + 1);
                difficulty.text = GlobalSetting.difficulty.Substring(0, GlobalSetting.difficulty.LastIndexOf(' '));
            }
            catch
            {
                difficultyNumber.text = GlobalSetting.difficulty;
            }

            Charter = GlobalSetting.charter;
            Composer = GlobalSetting.composer;
            Illustrator = GlobalSetting.illustrator;

            charter.text = Charter;
            composer.text = Composer;
            illustrator.text = Illustrator;

            StartCoroutine(YieldDoAnimation());
        }

        IEnumerator YieldDoAnimation()
        {
            yield return new WaitForSeconds(1);
            GlobalSetting.PlayClipAtPoint(enter, new Vector3(0, 0, -10), 1);
            SceneTransit.Instance.ReplaceScene("PlayingScene");
            var operation = SceneManager.LoadSceneAsync("PlayingScene");
            operation.allowSceneActivation = false;
            slideInto.enabled = true;
            yield return new WaitForSeconds(5);
            while (operation.progress < .9f)
            {
                yield return new WaitForSeconds(.2f);
            }

            cutOut.enabled = true;
            yield return new WaitForSeconds(1);
            operation.allowSceneActivation = true;
        }
    }
}