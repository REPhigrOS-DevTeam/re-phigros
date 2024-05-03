using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MainCore.Utilities
{
    public class InfoTxtReader
    {
        private Dictionary<string, string> _text;

        public InfoTxtReader(string fileName)
        {
            _text = new Dictionary<string, string>();
            List<string> list = File.ReadLines(fileName).ToList();
            foreach (string str in list)
            {
                int length = 2;
                int indexOf = str.IndexOf(": ", StringComparison.Ordinal);
                if (indexOf < 0)
                {
                    indexOf = str.IndexOf(":", StringComparison.Ordinal);
                    length = 1;
                }
                if (indexOf < 0) continue;
                string key = str.Substring(0, indexOf).Trim();
                string value = str.Substring(indexOf + length).Trim();
                if (!_text.ContainsKey(key)) _text.Add(key, value);
            }
        }

        public string GetComposer() => Get("Composer");

        public string GetCharter() => Get("Charter");

        public string GetDifficulty() => Get("Level");

        public string GetName() => Get("Name");

        public string GetSongFileName() => Get("Song");
        
        public string GetIllustrationFileName() => Get("Picture");
        
        public string GetChartFileName() => Get("Chart");

        private string Get(string identifier)
        {
            return _text.ContainsKey(identifier) ? _text[identifier] : "";
        }
    }
}