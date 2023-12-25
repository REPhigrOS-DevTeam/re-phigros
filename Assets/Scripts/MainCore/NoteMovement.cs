using System;
using System.Collections.Generic;
using MainCore.Data;
using MainCore.Utilities;
using UnityEngine;

namespace MainCore
{
    public class NoteMovement : MonoBehaviour
    {
        public int notetype = -1;
        public note Note;
        public int isAbove;
        public List<Sprite> NormalSprites;
        public Sprite HLsprite;
        public bool destroyed = false;
        public GameObject badTap;
        public NoteStat status = NoteStat.None;
        public Vector3 originalSize;

        public SpriteRenderer thisRenderer;
        public Transform cachedTransform;

        //优化
        public JudgeLineMovement parentLine;
        protected EffectManager holdEffect;
        protected Color[] lastColors = new Color[3];
        protected Color[] temporaryColors = new Color[3];
        private Sprite badTapSprite;
        private bool paintBad;

        public virtual void OnStart()
        {
            destroyed = false;
            status = NoteStat.None;
            holdEffect = null;
            var scaleFactor = Note.size;
            originalSize = new Vector3(GlobalSetting.globalNoteScale * scaleFactor,
                isAbove * GlobalSetting.globalNoteScale, 1);
            gameObject.transform.localScale = originalSize;
            status = NoteStat.None;

            cachedTransform = transform;
            cachedTransform.localEulerAngles = new Vector3(0, 0, 0);
            cachedTransform.localPosition = new Vector3(Note.positionX, 0, cachedTransform.localPosition.z);

            if (Note.isMulti)
            {
                thisRenderer.sprite = HLsprite;
            }
            else
            {
                thisRenderer.sprite = NormalSprites[0];
            }
        }

        public void OnUpdate(float noteHeight)
        {
            float noteHeightForNoteControl =
                Note.speed == 0 ? noteHeight : (float) (noteHeight / Note.speed); // Cause repetitive calculation.
            // Fix it later.

            OtherWorksOnUpdate();
            //UpdateNoteHeight(noteHeight, noteHeightForNoteControl);

            CheckOverLine();

            //判定↓
            if (!Note.isFake)
            {
                CheckJudgeStatus();
            }
            else
            {
                if (parentLine.PgrTime >= Note.time + Note.holdTime)
                {
                    destroyed = true;
                }
            }

            //alpha扩展
            var temp = parentLine.AlphaExtension;
            var invisibleFlag = (parentLine.PgrTime < Note.time - Note.visibleTime) ||
                                (temp == AlphaExtendMode.InvisibleAll) ||
                                (temp == AlphaExtendMode.VisibleUpside && isAbove == -1) ||
                                (temp == AlphaExtendMode.VisibleAfterTime && Note.time >= parentLine.VisibleTime);
            if (invisibleFlag)
            {
                temporaryColors[0] = Color.clear;
                temporaryColors[1] = Color.clear;
                temporaryColors[2] = Color.clear;
            }

            var alphaFactor = parentLine.GetNoteAlphaFactor(noteHeightForNoteControl);
            temporaryColors[0] = temporaryColors[0].SetAlpha(temporaryColors[0].a * alphaFactor);
            temporaryColors[1] = temporaryColors[1].SetAlpha(temporaryColors[1].a * alphaFactor);
            temporaryColors[2] = temporaryColors[2].SetAlpha(temporaryColors[2].a * alphaFactor);

            UpdateRenderer();

            if (destroyed)
            {
                parentLine.NotesCanBeUpdated.Remove(this);
                NotePool.GetInstance().RecycleObj(this);
            }
        }

        public virtual void UpdateNoteSkin(SkinInfo skinInfo, int type)
        {
            NormalSprites[0] = type switch
            {
                0 => skinInfo.click,
                1 => skinInfo.drag,
                2 => skinInfo.flick,
                3 => skinInfo.holdHead,
                _ => throw new ArgumentException()
            };
            HLsprite = type switch
            {
                0 => skinInfo.clickMh,
                1 => skinInfo.dragMh,
                2 => skinInfo.flickMh,
                3 => skinInfo.holdHeadMh,
                _ => throw new ArgumentException()
            };
            badTapSprite = skinInfo.click_bad;
            paintBad = skinInfo.paintBadColor;
        }

        protected virtual void OtherWorksOnUpdate()
        {
        }

        //We now move it to Line to do transform works.
        /*protected virtual void UpdateNoteHeight(float noteHeight, float nhfnc)
        {
            var localPosition = cachedTransform.localPosition;
            localPosition = new Vector3(GameUtils.GetAspectX(Note.positionX) * parentLine.GetNoteXFactor(nhfnc), 
                (isAbove * noteHeight) * parentLine.GetNoteYFactor(nhfnc) , 
                localPosition.z);
            cachedTransform.localPosition = localPosition;
            cachedTransform.localScale = originalSize * parentLine.GetNoteSizeFactor(nhfnc);
        }*/

        protected virtual void CheckOverLine()
        {
            bool isOverLine = parentLine.Line.isCover && cachedTransform.localPosition.y * isAbove < -0.01f;
            temporaryColors[0] = isOverLine ? Color.clear : Color.white.SetAlpha(Note.alpha);
        }

        protected virtual void CheckJudgeStatus()
        {
            if (GlobalSetting.autoPlay)
            {
                if (parentLine.PgrTime - Note.time >= 0 && status == NoteStat.None)
                {
                    GlobalSetting.scoreCounter.Add(NoteStat.Perfect);
                    status = NoteStat.Perfect;
                    destroyed = true;
                    GlobalSetting.PlayNoteSound(notetype);
                }

                if (status != NoteStat.None)
                {
                    UpdateEffect();
                }

                return;
            }

            if (Note.time <= parentLine.PgrTime)
            {
                temporaryColors[0] =
                    Color.white.SetAlpha(
                        Mathf.Max(1 - (parentLine.PgrTime - Note.time) / GlobalSetting.GetJudgeTime().bTime, 0) * Note.alpha);
            }

            if (parentLine.PgrTime >= Note.time + GlobalSetting.GetJudgeTime().bTime && status == NoteStat.None)
            {
                GlobalSetting.scoreCounter.Add(NoteStat.Miss);
                status = NoteStat.Miss;
                destroyed = true;
            }

            if (notetype is 2 or 4 && status == NoteStat.Perfect)
            {
                if (parentLine.PgrTime - Note.time > -.001f)
                {
                    GlobalSetting.scoreCounter.Add(status);
                    GlobalSetting.PlayNoteSound(notetype);
                    UpdateEffect();
                    destroyed = true;
                }
            }
        }

        protected virtual void UpdateRenderer()
        {
            //if (lastColors[0] != temporaryColors[0])
            //{
            //     lastColors[0] = temporaryColors[0];
            thisRenderer.color = temporaryColors[0];
            //}
        }

        protected virtual void UpdateEffect()
        {
            if (status is NoteStat.Miss or NoteStat.Bad || holdEffect != null) return;
            var cachedlocalPosition = cachedTransform.localPosition;
            var localPosition = new Vector3(cachedlocalPosition.x,
                0, cachedlocalPosition.z);
            cachedTransform.localPosition = localPosition;
            holdEffect = HitEffectManager.GetInstance().GetObj(status == NoteStat.Perfect ? HitFxJudgeType.Perfect : HitFxJudgeType.Good, GlobalSetting.Skin);
            holdEffect.transform.position = cachedTransform.position;
            holdEffect.PlayParticle();
            holdEffect.PlayEffect();
            cachedTransform.localPosition = cachedlocalPosition;
            temporaryColors[0] = Color.clear;
        }

        public virtual bool Judge(float time, Finger f)
        {
            if (status != NoteStat.None)
                return false;
            float deltaTime = Note.time - time;
            if (notetype == 1 && f.IsFirstClick)
            {
                f.ClearTapFlag();
                if (deltaTime > GlobalSetting.GetJudgeTime().bTime)
                {
                    status = NoteStat.Bad;
                    GlobalSetting.scoreCounter.Add(NoteStat.Bad);
                    GameObject badTapInstance = Instantiate(badTap, cachedTransform.position, cachedTransform.rotation);
                    badTapInstance.transform.localScale = cachedTransform.lossyScale;
                    var spriteRenderer = badTapInstance.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = paintBad ? new Color(108f / 255f, 67f / 255f, 67f / 255f) : Color.white;
                    spriteRenderer.sprite = badTapSprite;
                }
                else if (deltaTime > GlobalSetting.GetJudgeTime().gTime)
                {
                    status = NoteStat.Good;
                    GlobalSetting.scoreCounter.Add(NoteStat.Good);
                    GlobalSetting.scoreCounter.early++;
                    GlobalSetting.PlayNoteSound(notetype);
                }
                else if (deltaTime > -GlobalSetting.GetJudgeTime().gTime)
                {
                    status = NoteStat.Perfect;
                    GlobalSetting.scoreCounter.Add(NoteStat.Perfect);
                    GlobalSetting.PlayNoteSound(notetype);
                }
                else
                {
                    status = NoteStat.Good;
                    GlobalSetting.scoreCounter.Add(NoteStat.Good);
                    GlobalSetting.scoreCounter.late++;
                    GlobalSetting.PlayNoteSound(notetype);
                }

                UpdateEffect();
                destroyed = true;
                return true;
            }
            else if (notetype == 2 && Mathf.Abs(deltaTime) < GlobalSetting.GetJudgeTime().judgeTime)
            {
                status = NoteStat.Perfect;
                return true;
            }
            else if (notetype == 4 && Mathf.Abs(deltaTime) < GlobalSetting.GetJudgeTime().judgeTime && f.IsFlick())
            {
                status = NoteStat.Perfect;
                return true;
            }

            return false;
        }
    }
}