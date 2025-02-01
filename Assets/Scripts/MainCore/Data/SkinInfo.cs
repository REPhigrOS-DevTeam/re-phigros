using System;
using Newtonsoft.Json;
using UnityEngine;

namespace MainCore.Data
{
    [Serializable]
    public class SkinInfo : ICloneable
    {
        [HideInInspector] public bool isExternal = true;
        [JsonIgnore] public Skin skin; // 内置的才需要
        [HideInInspector] public string id = ""; // 外置的才需要
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

        public Sprite clickBad;
        public Sprite[] hitFx;
        public Sprite hitParticle;
        public AudioClip clickAc;
        public AudioClip dragAc;
        public AudioClip flickAc;
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
        public object Clone()
        {
            SkinInfo skinInfo = new SkinInfo
            {
                isExternal = isExternal,
                skin = skin,
                id = id,
                skinName = skinName,
                author = author,
                description = description,
                click = click,
                clickMh = clickMh,
                drag = drag,
                dragMh = dragMh,
                flick = flick,
                flickMh = flickMh,
                holdHead = holdHead,
                holdHeadMh = holdHeadMh,
                holdBody = holdBody,
                holdBodyMh = holdBodyMh,
                holdEnd = holdEnd,
                holdEndMh = holdEndMh,
                clickBad = clickBad,
                hitFx = hitFx,
                hitParticle = hitParticle,
                clickAc = clickAc,
                dragAc = dragAc,
                flickAc = flickAc,
                paintBadColor = paintBadColor,
                perfectColor = perfectColor,
                goodColor = goodColor,
                hitFxDuration = hitFxDuration,
                hitFxScale = hitFxScale,
                hitFxRotate = hitFxRotate,
                hitFxTinted = hitFxTinted,
                hideParticles = hideParticles,
                holdKeepHead = holdKeepHead,
                holdRepeat = holdRepeat,
                holdCompact = holdCompact,
                holdLengthFactor = holdLengthFactor,
                holdMhLengthFactor = holdMhLengthFactor
            };

            return skinInfo;
        }
    }
}