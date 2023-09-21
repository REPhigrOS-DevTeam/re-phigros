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
        private Exception stackIsEmptyException => new InvalidOperationException("stack is empty");

        private Stack<string> _sceneTraceStack = new Stack<string>();
        [SerializeField] private Material transitionMaterial;
        private Material transitMaterial = null;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            // DontDestroyOnLoad(gameObject);
            transitMaterial = transitionImage.material = Instantiate(transitionMaterial);
            JumpScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 跳转场景，不能返回
        /// </summary>
        /// <param name="sceneName"></param>
        public void JumpScene(string sceneName)
        {
            if (_sceneTraceStack.Count == 0)
            {
                Debug.Log("初始化场景切换");
                _sceneTraceStack.Push(sceneName);
                return;
            }

            if (!_sceneTraceStack.TryPeek(out string item))
                throw stackIsEmptyException;

            _sceneTraceStack.Pop();
            _sceneTraceStack.Push(sceneName);
            TransitTo(sceneName);
        }

        public void AppendScene(string sceneName)
        {
            if (_sceneTraceStack.Count == 0)
                throw stackIsEmptyException;
            if (!SceneManager.GetSceneByName(sceneName).IsValid()) return;
            string item = _sceneTraceStack.Pop();
            _sceneTraceStack.Push(sceneName);
            _sceneTraceStack.Push(item);
        }

        /// <summary>
        /// 跳转场景 可以返回
        /// </summary> 
        public void LoadScene(string sceneName)
        {
            if (_sceneTraceStack.Count == 0)
                throw stackIsEmptyException;

            _sceneTraceStack.Push(sceneName);
            TransitTo(sceneName);
        }

        public void Back()
        {
            if (_sceneTraceStack.Count == 1)
                return;
            _sceneTraceStack.Pop();
            string lastScene = _sceneTraceStack.Peek();
            TransitTo(lastScene);
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