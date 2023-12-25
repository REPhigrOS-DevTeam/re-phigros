using UnityEditor;
using UnityEngine;

namespace MainCore.Utilities
{
    [CreateAssetMenu(fileName = "New Skin Info", menuName = "RPGR/ Skin Info")]
    public class SkinInfo : ScriptableObject
    {
        public Sprite click,
            clickMh,
            drag,
            dragMh,
            flick,
            flickMh,
            holdHead,
            holdHeadMh,
            holdBody,
            holdBodyMh,
            holdEnd,
            holdEndMh;
        public Sprite click_bad;
        public Sprite[] hitFx;
        public Sprite hitParticle;
        public AudioClip clickAC, dragAC, flickAC;
        public bool paintBadColor = true; // 6C4343
        public Color perfectColor, goodColor;
        public float hitFxDuration;
        public float hitFxScale;
        public float hitFxRotate;
        public float hitFxTinted;
        public bool hideParticles;
        public bool holdKeepHead;
        public bool holdRepeat;
        
    }
}