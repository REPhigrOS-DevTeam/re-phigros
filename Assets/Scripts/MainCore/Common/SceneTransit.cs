using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MainCore.Common
{
    public class SceneTransit : MonoBehaviour
    {
        public static Action OnSceneClosing = () => { };
        private static readonly int UseColor = Shader.PropertyToID("_UseColor");
        private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
        private static readonly int PatternTex = Shader.PropertyToID("_PatternTex");
        [SerializeField] private Image transitionImage;
        [SerializeField] private List<Texture2D> ruleImages;


        private Stack<string> _sceneTraceStack = new Stack<string>();
        [SerializeField] private Material transitionMaterial;
        private Material transitMaterial = null;

        public static SceneTransit Instance { get; private set; }


        // Start is called before the first frame update
        void Start()
        {
            if (Instance != null)
                DestroyImmediate(Instance);
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            transitMaterial = transitionImage.material = Instantiate(transitionMaterial);
            _sceneTraceStack.Push(SceneManager.GetActiveScene().name);
        }

        public async void TransitTo(string sceneName)
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

        public void Back()
        {
            TransitTo(_sceneTraceStack.Pop());
            _sceneTraceStack.Push(SceneManager.GetActiveScene().name);
        }
    }
}