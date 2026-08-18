using System;
using DG.Tweening;
using MainCore.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MainCore
{
    public class EffectSplash : MonoBehaviour
    {
        public float t = 0;
        [SerializeField] SpriteRenderer sr;
        [SerializeField] SpriteRenderer parentSr;
        private float a;
        private float b;
        private float rad;
        private float spd = 0;

        // Start is called before the first frame update
        void Start()
        {
            t = 0;
            spd = Random.Range(0f, 1f) * 80f + 185f;
            rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            sr.sortingLayerName = "AboveNotes";
            sr.sortingOrder = 5;
        }

        // Update is called once per frame
        void Update()
        {
            t += Time.deltaTime * 2f;
            a = (float) (6.234f * Math.Pow(t, 3) - 49.572f * t * t + 49.197f * t + 14.964f);
            b = ((spd) * 9 * t / (8 * t + 1)) * 0.011f;
            transform.localScale = new Vector3(a, a, 1f);
            transform.localPosition = new Vector3(b * Mathf.Cos(rad), b * Mathf.Sin(rad));
            //transform.localPosition += dir.normalized * spd * Time.fixedDeltaTime;
            //spd -= Time.fixedDeltaTime * 10f;
            var tColor = parentSr.color;
            sr.color =
                new Color(tColor.r,
                    tColor.g,
                    tColor.b,
                    sr.color.a);
        }

        private void OnEnable()
        {
            sr.SetAlpha(1);
            sr.DOFade(0, .5f);
            t = 0;
            spd = Random.Range(0f, 1f) * 80f + 185f;
            rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        }

        private void OnDisable()
        {
            t = 0f;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localScale = new Vector3(0, 0, 0);
        }

        public void DestroyThis()
        {
            //Destroy(gameObject);
        }
    }
}