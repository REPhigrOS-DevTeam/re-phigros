using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MainCore.Utilities
{
    public class RKSManager
    {
        private const string RKSSaveKey = "Scores";
        public static List<RKSData> HighscoreList = new List<RKSData>();

        public void Read()
        {
            HighscoreList.Clear();
            string s = PlayerPrefs.GetString(RKSSaveKey, "");
            if (s == "") return;
            var strings = s.Split("\n");
            foreach (string line in strings)
            {
                string[] split = line.Split("/");
                if (split.Length != 3) throw new ArgumentException("Illegal Score List format");
                try
                {
                    HighscoreList.Add(new RKSData(int.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])));
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Illegal Score List format");
                }
            }
        }

        public void Save()
        {
            PlayerPrefs.SetString(RKSSaveKey,
                string.Join("\n",
                    HighscoreList.Select(data => string.Join("/", data.songId,
                        data.acc, data.difficulty.ToString("0.0")))));
            PlayerPrefs.Save();
        }

        public static (int?[], float) GetRKS()
        {
            List<RKSData> qwq = HighscoreList.OrderBy(CalculateSingleRKS).ToList();
            if (qwq.Count > 19)
            {
                qwq = qwq.Take(19).ToList();
            }
            else
            {
                while (qwq.Count < 19)
                {
                    qwq.Add(null);
                }
            }

            List<RKSData> rksDatas = HighscoreList.FindAll(data => Mathf.Abs(data.acc - 100f) < 0.01).ToList();
            qwq.Insert(0, rksDatas.Count == 0 ? null : rksDatas.OrderBy(data => data.difficulty).ToArray()[0]);
            return (qwq.Select(a => a?.songId).ToArray(),
                qwq.Select(a => a == null ? 0f : CalculateSingleRKS(a)).Average());
        }

        private static float CalculateSingleRKS(RKSData data)
        {
            if (data.acc < 70.0f) return 0f;
            return Mathf.Pow((data.acc * 100.0f - 55.0f) / 45.0f, 2) * data.difficulty;
        }
    }

    public class RKSData
    {
        public int songId;
        public float acc;
        public float difficulty;

        public RKSData(int songId, float acc, float difficulty)
        {
            this.songId = songId;
            this.acc = acc;
            this.difficulty = difficulty;
        }
    }
}