using System;
using System.Collections.Generic;
using System.Linq;
using MainCore.Common;
using UnityEngine;

namespace MainCore
{
    public class Finger
    {
        private int flickCnt = 0;
        Vector2 flickDirection;
        public bool isNewFlick;
        private Vector2 lastDirection;
        private TouchPhase lastPhase = TouchPhase.Canceled;
        public Queue<Vector2> lastPositions = new Queue<Vector2>();

        private int listLimit = Application.targetFrameRate / 5; //  1/5 seconds
        public Vector2 newPosition;
        int oldPosCounter;
        public TouchPhase phase;
        Vector3 screenPosition;
        Vector3 worldPosition;
        public bool IsFirstClick => lastPhase == TouchPhase.Began || phase == TouchPhase.Began;
        public bool IsKeyboard { private set; get; } = false;

        public void ClearOldPoss()
        {
            oldPosCounter = 0;
        }

        public void CheckInput(bool isKey = false)
        {
            IsKeyboard = isKey;
            screenPosition.x = newPosition.x;
            screenPosition.y = newPosition.y;
            screenPosition.z = 8f;
            worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            newPosition.x = worldPosition.x;
            newPosition.y = worldPosition.y;

            //CheckFlick
            isNewFlick = false;
            for (int i = 0; i < lastPositions.Count; i++)
            {
                var dir = (lastPositions.ElementAt(i) - newPosition).normalized;
                if (Vector2.Distance(lastPositions.ElementAt(i), newPosition) > 0.0075f)
                {
                    flickDirection = dir;
                    isNewFlick = true;
                    lastPositions.Clear();
                    break;
                }
            }

            //RecordTouchPosition
            if (phase == TouchPhase.Moved)
            {
                lastPositions.Enqueue(newPosition);
                if (lastPositions.Count > listLimit - 1) lastPositions.Dequeue();
            }
            else if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
            {
                flickCnt = 0;
            }
        }

        public void UpdatePhase(TouchPhase touchPhase)
        {
            lastPhase = phase;
            phase = touchPhase;
        }

        public void ClearTapFlag()
        {
            lastPhase = TouchPhase.Canceled;
            phase = TouchPhase.Stationary;
        }

        public bool IsFlick()
        {
            if (IsKeyboard)
            {
                return true;
            }

            if (((Vector2.Dot(lastDirection, flickDirection) < 0 && flickCnt > 0) || flickCnt == 0) && isNewFlick)
            {
                isNewFlick = false;
                flickCnt++;
                lastDirection = flickDirection;
                flickDirection = Vector2.zero;
                return true;
            }

            return false;
        }
    }

    public class JudgementManager : MonoSingleton<JudgementManager>
    {
        public int numOfFingers;
        public Finger[] fingers = new Finger[20];
        private Array keys;
        private List<float> notesDistances = new List<float>();

        private List<NoteMovement> notesInJudge = new List<NoteMovement>();

        // Start is called before the first frame update
        void Start()
        {
            fingers.Initialize();
            for (int i = 0; i < 20; i++)
                fingers[i] = new Finger();
            keys = Enum.GetValues(typeof(KeyCode));
        }

        // Update is called once per frame
        void Update()
        {
            if (GlobalSetting.AutoPlay)
                return;

#if UNITY_EDITOR
            UpdateMouseInput(); //Editor (for test).
#endif


            UpdateTouchInput(); //Touchscreen support.
#if UNITY_STANDALONE || UNITY_EDITOR
            UpdateKeyBoardInput(); //Keyboard(?) support.
#endif

            UpdateJudge();
        }

        public void UpdateTouchInput()
        {
            numOfFingers = Input.touchCount;
            for (int i = 0; i < numOfFingers; i++)
            {
                Touch touch = Input.GetTouch(i);
                fingers[i].UpdatePhase(touch.phase);
                fingers[i].newPosition = touch.position;
                fingers[i].CheckInput();
            }
        }

        public void UpdateKeyBoardInput()
        {
            if (!Input.anyKey) return;
            var t = 0;
            foreach (KeyCode i in keys)
            {
                if (t >= 20)
                    break;
                if (Input.GetKey(i))
                {
                    t++;
                    if (Input.GetKeyDown(i))
                    {
                        fingers[t].UpdatePhase(TouchPhase.Began);
                    }
                    else
                    {
                        fingers[t].UpdatePhase(TouchPhase.Moved);
                    }

                    fingers[t].newPosition = new Vector2(0, 0);
                    fingers[t].CheckInput(isKey: true);
                }
            }

            numOfFingers = t + 1;
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        public void UpdateMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) fingers[0].phase = TouchPhase.Began;
            else if (Input.GetMouseButton(0)) fingers[0].phase = TouchPhase.Moved;
            else if (Input.GetMouseButtonUp(0)) fingers[0].phase = TouchPhase.Ended;
            fingers[0].newPosition = Input.mousePosition;
            fingers[0].CheckInput();
            if (Input.GetMouseButton(0))
                numOfFingers = 1;
            else
                numOfFingers = 0;
        }
#endif

        public void UpdateJudge()
        {
            if (numOfFingers == 0)
                return;
            foreach (var i in GlobalSetting.Lines)
            {
                for (int k = 0; k < numOfFingers; k++)
                {
                    i.PositionX[k] = GetLocalPosition(fingers[k].newPosition, i.transform.parent).x;
                }
            }

            float pTime = Main.Instance.progressManager.NowTime;

            for (int i = 0; i < numOfFingers; i++)
            {
                var judgedFlick = false;
                var judgedFlickTime = 9999f;
                notesInJudge.Clear();
                notesDistances.Clear();
                foreach (var line in GlobalSetting.Lines)
                {
                    var (n, flickTime, absDistance) = line.GetNearestNote(fingers[i], line.PositionX[i]);
                    if (n != null)
                    {
                        notesInJudge.Add(n);
                        notesDistances.Add(absDistance);
                    }

                    if (flickTime < 9990f)
                    {
                        judgedFlick = true;
                        judgedFlickTime = Math.Min(flickTime, judgedFlickTime);
                    }
                }

                if (notesInJudge.Count == 0)
                    continue;

                NoteMovement note = notesInJudge[0];
                float time = notesInJudge[0].Note.time;
                float distance = notesDistances[0];
                for (var index = 0; index < notesInJudge.Count; index++)
                {
                    var t = notesInJudge[index];
                    if (note.Note.time > t.Note.time)
                    {
                        time = t.Note.time;
                        note = t;
                        distance = notesDistances[index];
                    }
                }

                for (var index = 0; index < notesInJudge.Count; index++)
                {
                    var t = notesInJudge[index];
                    if (Math.Abs(t.Note.time - time) < 0.0001f)
                    {
                        if (notesDistances[index] < distance)
                        {
                            distance = notesDistances[index];
                            note = t;
                        }
                    }
                }

                if (judgedFlick && note.Note.time > judgedFlickTime) //如果判定了flick且flick在tap前面 // TODO: 为啥这个有bug
                {
                    fingers[i].ClearTapFlag();
                    continue;
                }

                note.Judge(pTime, fingers[i]);
            }
        }

        private static Vector3 GetLocalPosition(Vector3 worldPosition, Transform parent) =>
            parent.InverseTransformPoint(worldPosition);

        public static bool NoteInJudgeArea(float fingerX, float noteX, bool isKeyboard = false)
        {
            if (isKeyboard)
            {
                return true;
            }

            return (fingerX > noteX - 2.2f && fingerX < noteX + 2.2f);
        }
    }
}