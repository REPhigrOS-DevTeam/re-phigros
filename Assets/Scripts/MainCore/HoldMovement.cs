using System;
using MainCore.Utilities;
using UnityEngine;

namespace MainCore
{
    public class HoldMovement : NoteMovement
    {
        public Sprite HLspriteHoldBody;
        public Sprite HLspriteHoldEnd;
        public SpriteRenderer[] holdRenders = new SpriteRenderer[3];
        public Transform[] holdParts = new Transform[3];
        private bool holdCatched = false;
        private float holdEffectCnt = 0.2f;

        private float holdEventScale = 1f;
        private float holdLengthFactor = 4.25f;
        private bool holdMissed = false;
        private bool holdOK = false;
        private float holdOriginLength = 0;
        private float holdRealLength = 0;

        private float releaseCounter = 0f;

        private float holdBodyUnit;

        public override void OnStart()
        {
            destroyed = false;
            status = NoteStat.None;
            holdEffect = null;
            holdMissed = false;
            holdCatched = false;
            holdOK = false;
            holdRealLength = 0;
            holdOriginLength = 0;
            holdEffectCnt = 0.2f;
            releaseCounter = 0f;

            var scaleFactor = Note.size;
            originalSize = new Vector3(scaleFactor, isAbove, 1);
            gameObject.transform.localScale = originalSize;

            if (Note.isMulti)
            {
                holdRenders[0].sprite = HLsprite;
                holdRenders[1].sprite = HLspriteHoldBody;
                holdRenders[2].sprite = HLspriteHoldEnd;
            }
            else
            {
                holdRenders[0].sprite = NormalSprites[0];
                holdRenders[1].sprite = NormalSprites[1];
                holdRenders[2].sprite = NormalSprites[2];
            }

            if (GlobalSetting.CurrentSkinInfo.holdRepeat)
            {
                holdRenders[1].drawMode = SpriteDrawMode.Tiled;
                holdRenders[1].tileMode = SpriteTileMode.Continuous;
            }

            holdOriginLength = (parentLine.CalculateNoteHeight(Note.time + Note.holdTime) -
                                parentLine.CalculateNoteHeight(Note.time));
            holdBodyUnit = holdRenders[1].sprite.rect.height / holdRenders[1].sprite.pixelsPerUnit;

            cachedTransform = transform;
            cachedTransform.localEulerAngles = new Vector3(0, 0, 0);
            cachedTransform.localPosition = new Vector3(Note.positionX, 0, 0);

            holdParts[0].localScale = new Vector3(GlobalSetting.globalNoteScale, GlobalSetting.globalNoteScale, 1.0f);
            holdParts[1].localScale = new Vector3(GlobalSetting.globalNoteScale, GlobalSetting.globalNoteScale, 1.0f);
            holdParts[2].localScale = new Vector3(GlobalSetting.globalNoteScale, GlobalSetting.globalNoteScale, 1.0f);
        }


        protected override void CheckOverLine()
        {
            bool isOverLine = parentLine.Line.isCover && cachedTransform.localPosition.y * isAbove < -0.01f;
            if (isOverLine)
            {
                temporaryColors[0] = new Color(1, 1, 1, 0);
                temporaryColors[1] = new Color(1, 1, 1, 0);
                temporaryColors[2] = new Color(1, 1, 1, 0);
            }
            else
            {
                if (parentLine.PgrTime < Note.time)
                    temporaryColors[0] = new Color(1, 1, 1, Note.alpha);
                else
                    temporaryColors[0] = new Color(1, 1, 1, 0f);
                temporaryColors[1] = new Color(1, 1, 1, Note.alpha);
                temporaryColors[2] = new Color(1, 1, 1, Note.alpha);
            }
        }

        protected override void CheckJudgeStatus()
        {
            if (GlobalSetting.autoPlay)
            {
                if (parentLine.PgrTime >= Note.time && !holdCatched)
                {
                    holdCatched = true;
                    GlobalSetting.PlayNoteSound(notetype);
                    status = NoteStat.Perfect;
                }

                if (parentLine.PgrTime >=
                    Note.time + Math.Max(0, Note.holdTime - GlobalSetting.GetJudgeTime().judgeTime) && !holdOK)
                {
                    GlobalSetting.scoreCounter.Add(NoteStat.Perfect);
                    holdOK = true;
                }

                if (holdCatched)
                {
                    UpdateEffect();
                }

                if (parentLine.PgrTime >= Note.time + Note.holdTime)
                {
                    destroyed = true;
                }

                return;
            }

            if (holdCatched && !holdOK && !holdMissed && !GlobalSetting.Paused)
                JudgeHold();
            if (Note.time <= parentLine.PgrTime)
            {
                if (parentLine.PgrTime - Note.time >= GlobalSetting.GetJudgeTime().bTime && !holdCatched &&
                    !GlobalSetting.autoPlay)
                {
                    holdMissed = true;
                    if (status != NoteStat.Miss)
                    {
                        GlobalSetting.scoreCounter.Add(NoteStat.Miss);
                        status = NoteStat.Miss;
                    }

                    temporaryColors[1] = new Color(1, 1, 1, 0.45f * Note.alpha);
                    temporaryColors[2] = new Color(1, 1, 1, 0.45f * Note.alpha);
                }
            }

            if (status != NoteStat.None)
                UpdateEffect();
            if (parentLine.PgrTime >= Note.time + Note.holdTime)
            {
                if (holdOK) destroyed = true;
                else if (parentLine.PgrTime >= Note.time + .2f) destroyed = true;
            }

            if (Note.time - parentLine.PgrTime < -GlobalSetting.GetJudgeTime().bTime && !holdCatched &&
                status == NoteStat.None) //没接住miss
            {
                GlobalSetting.scoreCounter.Add(NoteStat.Miss);
                status = NoteStat.Miss;
                holdMissed = true;
            }
        }

        public override void UpdateNoteSkin(SkinInfo skinInfo, int type)
        {
            base.UpdateNoteSkin(skinInfo, type);
            NormalSprites[1] = skinInfo.holdBody;
            NormalSprites[2] = skinInfo.holdEnd;
            HLspriteHoldBody = skinInfo.holdBodyMh;
            HLspriteHoldEnd = skinInfo.holdEndMh;
        }

        protected override void OtherWorksOnUpdate()
        {
            HoldLengthReset();
        }

        /*protected override void UpdateNoteHeight(float noteHeight, float nhfnc)
        {
            if (parentLine.PgrTime < Note.time)
            {
                var localPosition = cachedTransform.localPosition;
                localPosition = new Vector3(GameUtils.GetAspectX(Note.positionX) * parentLine.GetNoteXFactor(nhfnc),
                    (isAbove * noteHeight) * parentLine.GetNoteYFactor(nhfnc), 
                    localPosition.z);
                cachedTransform.localPosition = localPosition;
            }
            else
            {
                var localPosition = cachedTransform.localPosition;
                localPosition = new Vector3(GameUtils.GetAspectX(Note.positionX) * parentLine.GetNoteXFactor(nhfnc),
                    0,
                    localPosition.z);
                cachedTransform.localPosition = localPosition;
            }
            cachedTransform.localScale = originalSize * parentLine.GetNoteSizeFactor(nhfnc);
            
        }*/

        protected override void UpdateRenderer()
        {
            holdRenders[0].color = temporaryColors[0];
            holdRenders[1].color = temporaryColors[1];
            holdRenders[2].color = temporaryColors[2];
        }

        protected override void UpdateEffect()
        {
            if (status is NoteStat.Miss) return;
            if (GlobalSetting.Paused) return;
            if (holdCatched || holdOK)
            {
                holdEffectCnt += Time.deltaTime;
                if (holdEffectCnt >= 0.2f)
                {
                    holdEffect = HitEffectManager.GetInstance().GetObj(status == NoteStat.Perfect ? HitFxJudgeType.Perfect : HitFxJudgeType.Good, GlobalSetting.Skin);
                    holdEffect.transform.position = cachedTransform.position;
                    holdEffect.PlayEffect();
                    holdEffect.PlayParticle();
                    holdEffectCnt = 0;
                }
            }
        }

        public override bool Judge(float time, Finger f)
        {
            if (status != NoteStat.None)
                return false;
            float deltaTime = Note.time - time;
            if (!holdOK && !holdMissed)
            {
                if (!holdCatched && f.IsFirstClick)
                {
                    f.ClearTapFlag();
                    if (deltaTime > GlobalSetting.GetJudgeTime().gTime)
                    {
                        status = NoteStat.Early;
                        //GlobalSetting.scoreCounter.early++;
                        GlobalSetting.PlayNoteSound(notetype);
                    }
                    else if (deltaTime > -GlobalSetting.GetJudgeTime().gTime)
                    {
                        status = NoteStat.Perfect;
                        GlobalSetting.PlayNoteSound(notetype);
                    }
                    else
                    {
                        status = NoteStat.Late;
                        //GlobalSetting.scoreCounter.late++;
                        GlobalSetting.PlayNoteSound(notetype);
                    }

                    holdCatched = true;
                    return true;
                }
            }

            return false;
        }

        private void HoldLengthReset()
        {
            if (Note.time <= parentLine.PgrTime)
            {
                var clampedLength = Math.Max(0, parentLine.CalculateNoteHeight(Note.time + Note.holdTime));
                //holdRealLength = (float) (.3f * Note.speed * clampedLength * speedFactor / 6.75f);
                // if (GlobalSetting.CurrentSkinInfo.holdRepeat)
                // {
                //     holdRenders[1].size = new Vector2(holdRenders[1].size.x, clampedLength * holdBodyUnit);
                // }
                holdRealLength = (float) (Note.speed * clampedLength * parentLine.SpeedFactor / holdLengthFactor);
                holdParts[1].localScale = new Vector3(GlobalSetting.globalNoteScale, holdRealLength, 1.0f);
                holdParts[2].localPosition = new Vector3(0, holdRealLength * holdLengthFactor, 0);
            }
            else
            {
                // if (GlobalSetting.CurrentSkinInfo.holdRepeat)
                // {
                //     holdRenders[1].size = new Vector2(holdRenders[1].size.x, holdOriginLength * holdBodyUnit);
                // }
                holdRealLength = (float) (Note.speed * holdOriginLength * parentLine.SpeedFactor / holdLengthFactor);
                holdParts[1].localScale = new Vector3(GlobalSetting.globalNoteScale, holdRealLength, 1.0f);
                holdParts[2].localPosition = new Vector3(0, holdRealLength * holdLengthFactor, 0);
            }
        }

        private void JudgeHold()
        {
            if (Note.time + Note.holdTime - GlobalSetting.GetJudgeTime().judgeTime <= parentLine.PgrTime)
            {
                holdOK = true;
                GlobalSetting.scoreCounter.Add(status);
                return;
            }

            holdCatched = false;
            for (int i = 0; i < JudgementManager.Instance.numOfFingers; i++)
            {
                var f = JudgementManager.Instance.fingers[i];
                float dx = parentLine.PositionX[i];
                if (f.phase != TouchPhase.Canceled &&
                    JudgementManager.NoteInJudgeArea(dx, cachedTransform.localPosition.x, f.IsKeyboard))
                {
                    releaseCounter = 0;
                    holdCatched = true;
                    break;
                }
            }

            if (!holdCatched) //按住后断了hold => miss
            {
                releaseCounter += Time.deltaTime;
                holdCatched = true;
                if (releaseCounter > GlobalSetting.GetJudgeTime().gTime)
                {
                    holdCatched = false;
                    holdMissed = true;
                    GlobalSetting.scoreCounter.Add(NoteStat.Miss);
                    status = NoteStat.Miss;
                }
            }
        }
    }
}