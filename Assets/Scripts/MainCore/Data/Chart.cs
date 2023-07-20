using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainCore.Data
{
    [System.Serializable]
    public class Chart
    {
        public int formatVersion = 0;
        public float offset = 0;
        public int numOfNotes = 0;
        public List<judgeLine> judgeLineList = new List<judgeLine>();
        public int maxZOrder = 0;
    }

    [System.Serializable]
    public class judgeLine
    {
        public int numOfNotes = 0;
        public int numOfNotesAbove = 0;
        public int numOfNotesBelow = 0;
        public float bpm = -1;
        public List<judgeLineSpeedEvent> speedEvents = new List<judgeLineSpeedEvent>();
        public List<note> notesAbove = new List<note>();
        public List<note> notesBelow = new List<note>();
        public List<judgeLineEvent> judgeLineDisappearEvents = new List<judgeLineEvent>();
        public List<judgeLineEvent> judgeLineMoveEvents = new List<judgeLineEvent>();
        public List<judgeLineEvent> judgeLineRotateEvents = new List<judgeLineEvent>();

        public List<judegeLineEventLayer> rpeLayers = new();
        public judgeLineExtendEvent extended = new();
        public bool useImage = false;
        public Sprite customImage;
        public int father = -1;
        public int zOrder;
        public bool isCover = true;
        public List<noteControl> posControl = new();
        public List<noteControl> sizeControl = new();
        public List<noteControl> skewControl = new();
        public List<noteControl> yControl = new();
        public List<noteControl> alphaControl = new();
        public string attachUI = "**tHiSisnOne AtTaCH U_i TEmPlAtE**";

        public void PushNote(int type, bool isAbove, float time, float posX, double speed, double floorPos,
            float holdTime = 0, bool isFake = false, float yOffset = 0, float size = 1, float visibleTime = 99999f,
            float alpha = 1)
        {
            if (isAbove)
            {
                notesAbove.Add(new note
                {
                    type = type,
                    time = time,
                    positionX = posX,
                    holdTime = holdTime,
                    speed = Double.IsNaN(speed) ? 1 : speed,
                    floorPosition = floorPos,
                    isFake = isFake,
                    visibleTime = visibleTime,
                    yOffset = yOffset,
                    size = size,
                    alpha = alpha
                });
                if (!isFake)
                    numOfNotesAbove++;
            }
            else
            {
                notesBelow.Add(new note
                {
                    type = type,
                    time = time,
                    positionX = posX,
                    holdTime = holdTime,
                    speed = Double.IsNaN(speed) ? 1 : speed,
                    floorPosition = floorPos,
                    isFake = isFake,
                    visibleTime = visibleTime,
                    yOffset = yOffset,
                    size = size,
                    alpha = alpha
                });
                if (!isFake)
                    numOfNotesBelow++;
            }

            if (!isFake)
                numOfNotes++;
        }

        public void PushEvent(int type, int easeType, float st, float et, float v11, float v12, float v21, float v22,
            AlphaExtendMode alphaMode = AlphaExtendMode.VisibleAll, float visibleTime = 0f)
        {
            switch (type)
            {
                case 1: //speed
                    speedEvents.Add(new judgeLineSpeedEvent
                    {
                        startTime = st,
                        endTime = et,
                        value = v11,
                    });
                    break;
                case 2: //alpha
                    judgeLineDisappearEvents.Add(new judgeLineEvent
                    {
                        startTime = st,
                        endTime = et,
                        start = v11,
                        end = v12,
                        easeType = easeType,
                        alphaMode = alphaMode,
                        visibleTime = visibleTime
                    });
                    break;
                case 3: //move
                    judgeLineMoveEvents.Add(new judgeLineEvent
                    {
                        startTime = st,
                        endTime = et,
                        start = v11,
                        end = v12,
                        start2 = v21,
                        end2 = v22,
                        easeType = easeType
                    });
                    break;
                case 4: //rotate
                    judgeLineRotateEvents.Add(new judgeLineEvent
                    {
                        startTime = st,
                        endTime = et,
                        start = v11,
                        end = v12,
                        start2 = v21,
                        end2 = v22,
                        easeType = easeType
                    });
                    break;
            }
        }

        public void PushSpeedEvent(double st, double et, double v11)
        {
            speedEvents.Add(new judgeLineSpeedEvent
            {
                startTime = st,
                endTime = et,
                value = v11,
            });
        }
    }

    [System.Serializable]
    public class judgeLineSpeedEvent
    {
        public double startTime = 0;
        public double endTime = 0;
        public double floorPosition = -10;
        public double value = 0;
        public double endValue = 0;
    }

    [System.Serializable]
    public class note
    {
        public int type = 0;
        public float time = 0;
        public float positionX = 0;
        public float holdTime = 0;
        public double speed = 0;
        public double floorPosition = 0;

        public bool isAbove = false;
        public bool isFake = false;
        public float yOffset = 0;
        public float visibleTime = 99999;
        public float size = 1;
        public float alpha = 1;

        public bool isMulti = false;
    }

    [System.Serializable]
    public class judgeLineEvent
    {
        public float startTime = 0;
        public float endTime = 0;
        public float start = 0;
        public float end = 0;
        public float start2 = 0;
        public float end2 = 0;

        public AlphaExtendMode alphaMode = AlphaExtendMode.VisibleAll;
        public float visibleTime = 0f;

        public int easeType = 1;
        public float easingLeft = 0;
        public float easingRight = 1;
    }

    [Serializable]
    public class judgeLineColorEvent
    {
        public float startTime = 0;
        public float endTime = 0;
        public Color start;
        public Color end;

        public int easeType = 1;
    }

    [Serializable]
    public class judegeLineEventLayer
    {
        public List<judgeLineEvent> alphaEvents = new();
        public List<judgeLineEvent> moveXEvents = new();
        public List<judgeLineEvent> moveYEvents = new();
        public List<judgeLineEvent> rotateEvents = new();
        public List<judgeLineSpeedEvent> speedEvents = new();
    }

    [Serializable]
    public class judgeLineTextEvent
    {
        public int easingType;
        public string end;
        public float endTime;
        public string start;
        public float startTime;
    }

    [Serializable]
    public class judgeLineExtendEvent
    {
        public List<judgeLineEvent> scaleXEvents = new();
        public List<judgeLineEvent> scaleYEvents = new();
        public List<judgeLineColorEvent> colorEvents = new();
        public List<judgeLineEvent> paintEvents = new();
        public List<judgeLineTextEvent> textEvents = new();
        public List<judgeLineEvent> inclineEvents = new();
    }

    [Serializable]
    public class noteControl
    {
        public float start;
        public float end;
        public float startValue;
        public float endValue;
        public int easeType;
    }

    [Serializable]
    public class RpeChartData // RPE JSON Chart
    {
        public List<RpeBpmList> BPMList = new();
        public RpeMeta META = new();
        public List<RpeJudgeLineSet> judgeLineList = new();

        [Serializable]
        public class RpeBpmList
        {
            public float bpm;
            public int[] startTime = new int[0];
        }

        [Serializable]
        public class RpeMeta
        {
            public int RPEVersion = -1;
            public string background;
            public string charter;
            public string composer;
            public string level;
            public string name;
            public int offset;
            public string song;
        }

        [Serializable]
        public class RpeJudgeLineSet
        {
            public string Texture;
            public string attachUI = "**tHiSisnOne AtTaCH U_i TEmPlAtE**";
            public int numOfNotes;
            public List<RpeEventLayer> eventLayers = new();
            public RpeEventLayerExtended extended;
            public List<RpeNoteSet> notes = new();
            public int father = -1;
            public int zOrder = 0;
            public int isCover = 1;
            public List<RpePosControl> posControl = new();
            public List<RpeSizeControl> sizeControl = new();
            public List<RpeSkewControl> skewControl = new();
            public List<RpeYControl> yControl = new();
            public List<RpeAlphaControl> alphaControl = new();
        }

        [Serializable]
        public class RpeNoteSet
        {
            public int above; // 1 above other below
            public int alpha = 255;
            public int[] endTime = new int[0];
            public bool isFake;
            public float positionX;
            public float size;
            public float speed;
            public int[] startTime = new int[0];
            public int type; // 1 tap 2 hold 3 flick 4 drag
            public float visibleTime;
            public float yOffset;
        }

        [Serializable]
        public class RpeEventLayer
        {
            public List<RpeValueSet> alphaEvents = new();
            public List<RpeValueSet> moveXEvents = new();
            public List<RpeValueSet> moveYEvents = new();
            public List<RpeValueSet> rotateEvents = new();
            public List<RpeValueSet> speedEvents = new();
        }

        [Serializable]
        public class RpeEventLayerExtended
        {
            public List<RpeValueSet> scaleXEvents = new();
            public List<RpeValueSet> scaleYEvents = new();
            public List<RpeColorEvent> colorEvents = new();
            public List<RpeValueSet> paintEvents = new();
            public List<RpeTextEvent> textEvents = new();
            public List<RpeValueSet> inclineEvents = new();
        }

        [Serializable]
        public class RpeValueSet
        {
            public float easingLeft = 0;
            public float easingRight = 1;
            public int easingType = 0;
            public float end;
            public int[] endTime = new int[0];
            public float start;
            public int[] startTime = new int[0];
        }

        [Serializable]
        public class RpeColorEvent
        {
            public int easingType;
            public int[] end = new int[0];
            public int[] endTime = new int[0];
            public int[] start = new int[0];
            public int[] startTime = new int[0];
        }

        [Serializable]
        public class RpeTextEvent
        {
            public int easingType;
            public string end;
            public int[] endTime = new int[0];
            public string start;
            public int[] startTime = new int[0];
        }

        [Serializable]
        public class RpePosControl : RpeNoteControl
        {
            public float pos;
        }

        [Serializable]
        public class RpeSizeControl : RpeNoteControl
        {
            public float size;
        }

        [Serializable]
        public class RpeSkewControl : RpeNoteControl
        {
            public float skew;
        }

        [Serializable]
        public class RpeYControl : RpeNoteControl
        {
            public float y;
        }

        [Serializable]
        public class RpeAlphaControl : RpeNoteControl
        {
            public float alpha;
        }

        [Serializable]
        public class RpeNoteControl
        {
            public float x;
            public int easing;
        }
    }

    public enum AlphaExtendMode
    {
        VisibleAll,
        InvisibleAll,
        VisibleUpside,
        VisibleAfterTime
    }
}