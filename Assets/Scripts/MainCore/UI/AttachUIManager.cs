using System.Collections.Generic;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class AttachUIManager : MonoBehaviour
    {
        [SerializeField] private List<RectTransform> transforms;
        [SerializeField] private List<Graphic> drawAbles;

        [SerializeField] private RectTransform progress;

        public static readonly Dictionary<string, int> Mapper = new()
        {
            { "combonumber", 0 },
            { "combo", 1 },
            { "score", 2 },
            { "level", 3 },
            { "bar", 4 },
            { "name", 5 },
            { "pause", 6 },
            { "accuracy", 7}
        };
        private List<Vector2> originalPositions = new();

        public static AttachUIManager Instance { get; private set; }

        void Start()
        {
            progress.localPosition *= new Vector2(GameUtils.ScreenDelta, 1);
            foreach (var t in transforms)
            {
                t.localPosition *= new Vector2(GameUtils.ScreenDelta, 1);
                originalPositions.Add(t.localPosition);
            }

            Instance = this;
        }

        public void FillUIStates(string uiName, Vector2 pos, Vector2 scale, float rotation, Color color)
        {
            var index = Mapper[uiName];
            pos = new Vector2(pos.x * 63f, pos.y * 64f);
            transforms[index].localPosition =
                new Vector2(originalPositions[index].x + pos.x, (originalPositions[index].y + pos.y));
            transforms[index].localScale = scale.SetZ(1);
            transforms[index].localEulerAngles = new Vector3(0, 0, rotation);
            drawAbles[index].color = color;
            if (uiName == "score") drawAbles[Mapper["accuracy"]].color = color;
        }

        private int IsUp(int i) => new int[] {0, 1, 2, 4, 6}.Contains(i) ? -1 : 1;
    }
}