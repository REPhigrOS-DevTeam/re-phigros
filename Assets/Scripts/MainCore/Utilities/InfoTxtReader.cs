using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MainCore.Utilities
{
    public class InfoTxtReader
    {
        private List<string> _text;

        public InfoTxtReader(string fileName)
        {
            _text = File.ReadLines(fileName).ToList();
        }

        public string GetComposer() => Get("Composer");

        public string GetCharter() => Get("Charter");

        public string GetDifficulty() => Get("Illustrator");

        public string GetName() => Get("Name");

        public string GetSongFileName() => Get("Song");
        
        public string GetIllustrationFileName() => Get("Picture");
        
        public string GetChartFileName() => Get("Chart");

        private string Get(string identifier)
        {
            try
            {
                var text = _text.First((x) => x.StartsWith($"{identifier}: "));
                return text.Substring(text.IndexOf(" ", StringComparison.Ordinal) + 1);
            }
            catch
            {
                return "";
            }
        }
    }
}