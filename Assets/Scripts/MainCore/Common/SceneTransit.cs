using System;
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
        
        void Start()
        {
            transitMaterial = transitionImage.material;
            JumpScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 跳转场景，不能返回
        /// </summary>
        /// <param name="sceneName"></param>
        public void JumpScene(string sceneName)
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
            TransitTo(sceneName);
        }

        public void Back()
        {
            if (navStack.Count == 1)
                return;

            navStack.Pop();
            var lastScene = navStack.Peek();
            TransitTo(lastScene.SceneName);
        }

        /// <summary>
        /// 跳转场景 可以返回
        /// </summary> 
        public void LoadScene(string sceneName)
        {
            AppendScene(sceneName);
            TransitTo(sceneName);
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
    }
}