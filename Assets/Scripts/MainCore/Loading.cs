using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainCore
{
    public class Loading : MonoBehaviour
    {
        private AsyncOperation operation;

        private double time;

        // Start is called before the first frame update
        void Start()
        {
            AsyncLoading();
            if (GlobalSetting.ChartPath.Contains(".pec"))
                GameObject.Find("Text").GetComponent<Text>().text = "Converting PEC to JSON\nBy lchzh3473...";
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
            if (time >= 1)
            {
                time = 0;
                GetComponent<UnityEngine.UI.Text>().text += '.';
            }

            if (operation.progress >= 0.9f)
                operation.allowSceneActivation = true;
        }

        private async void AsyncLoading()
        {
            await UniTask.SwitchToMainThread();
            operation = SceneManager.LoadSceneAsync("PlayingScene");
            operation.allowSceneActivation = false;
            await operation;
        }
    }
}