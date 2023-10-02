using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MainCore.Common
{
    public class SceneTransit : MonoSingleton<SceneTransit>
    {
        public static Action OnSceneClosing = () => { };
        private static readonly int UseColor = Shader.PropertyToID("_UseColor");
        private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
        private static readonly int PatternTex = Shader.PropertyToID("_PatternTex");
        [SerializeField] private Image transitionImage;
        [SerializeField] private List<Texture2D> ruleImages;

        [SerializeField] private Material transitionMaterial;
        private Material transitMaterial = null;

        private class NavigationInfo
        {
            public string SceneName;
            public object Data = null;
        }

        private Stack<NavigationInfo> navStack = new();
        private Exception stackIsEmptyException => new InvalidOperationException("stack is empty");

        protected override void OnAwake()
        {
            transitMaterial = transitionImage.material
#if UNITY_EDITOR
                    = Instantiate(transitionMaterial) // 防止本地mat文件每次都变动
#endif
                ;
            JumpScene(SceneManager.GetActiveScene().name);
            Scale = new float[width, height];
            for (int index1 = 0; index1 < width; ++index1)
            {
                for (int index2 = 0; index2 < height; ++index2)
                    Scale[index1, index2] = 0.0f;
            }
        }

        /// <summary>
        /// 跳转场景，不能返回
        /// </summary>
        /// <param name="sceneName"></param>
        public void JumpScene(string sceneName, int type = 1)
        {
            if (navStack.Count == 0)
            {
                navStack.Push(new()
                {
                    SceneName = sceneName,
                });
                return;
            }

            ReplaceScene(sceneName);
            switch (type)
            {
                case 0:
                    SceneManager.LoadScene(sceneName); // 直入
                    break;
                case 1:
                    TransitTo(sceneName); // 原来的转场
                    break;
                case 2:
                    DoScene(sceneName); // DR3的转场
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 跳转场景 可以返回
        /// </summary> 
        public void LoadScene(string sceneName, int type = 1)
        {
            AppendScene(sceneName);
            switch (type)
            {
                case 0:
                    SceneManager.LoadScene(sceneName); // 直入
                    break;
                case 1:
                    TransitTo(sceneName); // 原来的转场
                    break;
                case 2:
                    DoScene(sceneName); // DR3的转场
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Back()
        {
            if (navStack.Count == 1)
                return;

            navStack.Pop();
            var lastScene = navStack.Peek();
            TransitTo(lastScene.SceneName);
        }

        public void ReplaceScene(string sceneName)
        {
            if (!navStack.TryPeek(out var item))
                throw stackIsEmptyException;

            item.SceneName = sceneName;
        }

        public void AppendScene(string sceneName)
        {
            if (navStack.Count == 0)
                throw stackIsEmptyException;

            navStack.Push(new()
            {
                SceneName = sceneName
            });
        }

        private async void TransitTo(string sceneName)
        {
            transitionImage.raycastTarget = true;
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            GameObject transitAnimation = GameObject.FindWithTag("SceneTransitAnimation");
            if (transitAnimation)
            {
                await UniTask.Delay(transitAnimation.GetComponent<SceneTransitAnimation>().Quit());
            }

            transitMaterial.SetTexture(PatternTex, ruleImages[Random.Range(0, ruleImages.Count)]);
            transitMaterial.DOFloat(1f, Cutoff, .5f);

            OnSceneClosing.Invoke();
            await UniTask.Delay(500);
            operation.allowSceneActivation = true;

            transitMaterial.SetTexture(PatternTex, ruleImages[Random.Range(0, ruleImages.Count)]);
            transitMaterial.DOFloat(0f, Cutoff, .5f).OnComplete(() => { transitMaterial.SetFloat(Cutoff, 0f); })
                .OnKill(() => { transitMaterial.SetFloat(Cutoff, 0f); });
            await UniTask.Delay(500);
            transitAnimation = GameObject.FindWithTag("SceneTransitAnimation");
            if (transitAnimation)
            {
                await UniTask.Delay(transitAnimation.GetComponent<SceneTransitAnimation>().Quit());
            }

            transitionImage.raycastTarget = false;
        }

        // DR3的转场

        [SerializeField] private Texture img;
        [SerializeField] private int width = 16;
        [SerializeField] private int height = 9;
        private float[,] Scale;
        private Vector2 blocksize;
        private bool flag;

        private void DoScene(string SceneName)
        {
            if (flag) return;
            blocksize = new Vector2(Screen.width / (float)width, Screen.height / (float)height);
            StartCoroutine(ShowFade(SceneName));
        }

        private IEnumerator ShowFade(string SceneName)
        {
            Resources.UnloadUnusedAssets();
            int timer = 0;
            flag = true;
            while (true)
            {
                for (int index1 = 0; index1 < width; ++index1)
                {
                    for (int index2 = 0; index2 < height; ++index2)
                    {
                        if (index1 + index2 < timer)
                        {
                            Scale[index1, index2] += 0.1f;
                            if (Scale[index1, index2] >= 1.0)
                                Scale[index1, index2] = 1f;
                        }
                    }
                }

                ++timer;
                if (timer < width + height + 11)
                    yield return new WaitForSeconds(0.01f);
                else
                    break;
            }

            DOTween.KillAll();
            yield return null;
            SceneManager.LoadScene(SceneName);
            yield return null;
            timer = 0;
            while (true)
            {
                for (int index3 = 0; index3 < width; ++index3)
                {
                    for (int index4 = 0; index4 < height; ++index4)
                    {
                        if (index3 + index4 < timer)
                        {
                            Scale[index3, index4] -= 0.1f;
                            if (Scale[index3, index4] <= 0.0)
                                Scale[index3, index4] = 0.0f;
                        }
                    }
                }

                ++timer;
                if (timer < width + height + 11)
                    yield return new WaitForSeconds(0.01f);
                else
                    break;
            }

            flag = false;
            yield return null;
        }

        private void OnGUI()
        {
            if (!flag) return;
            for (int index1 = 0; index1 < width; ++index1)
            {
                for (int index2 = 0; index2 < height; ++index2)
                    GUI.DrawTextureWithTexCoords(
                        new Rect(
                            (float)(blocksize.x * (double)index1 + (1.0 - Scale[index1, index2]) * 0.5 * blocksize.x),
                            (float)(blocksize.y * (double)index2 + (1.0 - Scale[index1, index2]) * 0.5 * blocksize.y),
                            Scale[index1, index2] * blocksize.x, Scale[index1, index2] * blocksize.y), img,
                        new Rect((float)(0.5 * (1.0 - Scale[index1, index2])),
                            (float)(0.5 * (1.0 - Scale[index1, index2])), Scale[index1, index2],
                            Scale[index1, index2]));
            }
        }
    }
}