using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MainCore.Common
{
    public class SceneTransit : MonoSingleton<SceneTransit>
    {
        public static readonly UnityEvent OnSceneClosing = new();
        private static readonly int UseColor = Shader.PropertyToID("_UseColor");
        private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
        private static readonly int PatternTex = Shader.PropertyToID("_PatternTex");
        [SerializeField] private Image transitionImage;
        [SerializeField] private List<Texture2D> ruleImages;

        [SerializeField] private Material transitionMaterial;
        private Material _transitMaterial = null;

        private class NavigationInfo
        {
            public string SceneName;
            public object Data = null;
        }

        private Stack<NavigationInfo> navStack = new();
        private Exception stackIsEmptyException => new InvalidOperationException("stack is empty");

        protected override void OnAwake()
        {
            _transitMaterial = transitionImage.material
#if UNITY_EDITOR
                    = Instantiate(transitionMaterial) // 防止本地mat文件每次都变动
#endif
                ;
            JumpScene(SceneManager.GetActiveScene().name);
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
            EnterScene(sceneName, type);
        }

        /// <summary>
        /// 跳转场景 可以返回
        /// </summary> 
        public void LoadScene(string sceneName, int type = 1)
        {
            AppendScene(sceneName);
            EnterScene(sceneName, type);
        }

        private void EnterScene(string sceneName, int type = 1)
        {
            OnSceneClosing?.Invoke();
            switch (type)
            {
                case 0:
                    TransitTo(sceneName, false); // 不用galgame转场
                    break;
                case 1:
                    TransitTo(sceneName); // 原来的转场
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            OnSceneClosing?.RemoveAllListeners();
        }

        public void Back(bool useOldTransition = true)
        {
            if (navStack.Count == 1)
                return;

            navStack.Pop();
            var lastScene = navStack.Peek();
            TransitTo(lastScene.SceneName, useOldTransition);
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

        private async void TransitTo(string sceneName, bool useOldTransition = true)
        {
            transitionImage.raycastTarget = true;
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            
            await LeaveCurrentScene();

            if (useOldTransition)
            {
                _transitMaterial.SetTexture(PatternTex, ruleImages[Random.Range(0, ruleImages.Count)]);
                _transitMaterial.DOFloat(1f, Cutoff, .5f);
                await UniTask.Delay(500);
                operation.allowSceneActivation = true;
                await operation;
                _transitMaterial.SetTexture(PatternTex, ruleImages[Random.Range(0, ruleImages.Count)]);
                _transitMaterial.DOFloat(0f, Cutoff, .5f).OnComplete(() => { _transitMaterial.SetFloat(Cutoff, 0f); })
                    .OnKill(() => { _transitMaterial.SetFloat(Cutoff, 0f); });
                InitAnimation();
                await UniTask.Delay(500);
            }
            else
            {
                await AllowSwitch(operation);
            }
            
            await PlayEnterAnimation();
            transitionImage.raycastTarget = false;
        }
        
        private static async UniTask LeaveCurrentScene()
        {
            await PlayQuitAnimation();
        }

        private static async UniTask AllowSwitch(AsyncOperation operation)
        {
            operation.allowSceneActivation = true;
            await operation;
            InitAnimation();
        }

        public static void InitAnimation()
        {
            GameObject transitAnimation = GameObject.FindWithTag("SceneTransitAnimation");
            if (transitAnimation)
            {
                transitAnimation.GetComponent<SceneTransitAnimation>().Init();
            }
        }

        public static async UniTask PlayEnterAnimation()
        {
            GameObject transitAnimation = GameObject.FindWithTag("SceneTransitAnimation");
            if (transitAnimation)
            {
                await UniTask.Delay(transitAnimation.GetComponent<SceneTransitAnimation>().Enter() + 000);
            }
        }

        public static async UniTask PlayQuitAnimation()
        {
            GameObject transitAnimation = GameObject.FindWithTag("SceneTransitAnimation");
            if (transitAnimation)
            {
                await UniTask.Delay(transitAnimation.GetComponent<SceneTransitAnimation>().Quit() + 000);
            }   
        }
    }
}