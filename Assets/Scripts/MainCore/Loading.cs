using System.Collections;
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
            StartCoroutine(AsyncLoading());
            if (GlobalSetting.chartPath.Contains(".pec"))
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

        private IEnumerator AsyncLoading()
        {
            operation = SceneManager.LoadSceneAsync("PlayingScene");
            operation.allowSceneActivation = false;
            yield return operation;
        }
    }
}