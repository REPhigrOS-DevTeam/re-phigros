using UnityEditor;
using UnityEngine;

namespace MainCore.Utilities
{
    [CreateAssetMenu(fileName = "New Skin Info", menuName = "RPGR/ Skin Info")]
    public class SkinInfo : ScriptableObject
    {
        public bool isExternal = true;
        public Skin skin;
        public string id = "";
        public string skinName;
        public string author;
        public string description;
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
        public bool hitFxRotate;
        public bool hitFxTinted;
        public bool hideParticles;
        public bool holdKeepHead;
        public bool holdRepeat;
        public bool holdCompact;

        public float holdLengthFactor;
        public float holdMhLengthFactor;
    }
}