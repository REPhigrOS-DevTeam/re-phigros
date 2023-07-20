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

        public string GetComposer()
        {
            try
            {
                var text = _text.First((x) => x.StartsWith("Composer: "));
                return text.Substring(text.IndexOf(" ") + 1);
            }
            catch
            {
                return "";
            }
        }

        public string GetCharter()
        {
            try
            {
                var text = _text.First((x) => x.StartsWith("Charter: "));
                return text.Substring(text.IndexOf(" ") + 1);
            }
            catch
            {
                return "";
            }
        }

        public string GetIllustrator()
        {
            try
            {
                var text = _text.First((x) => x.StartsWith("Illustrator: "));
                return text.Substring(text.IndexOf(" ") + 1);
            }
            catch
            {
                return "";
            }
        }

        public string GetDifficulty()
        {
            try
            {
                var text = _text.First((x) => x.StartsWith("Level: "));
                return text.Substring(text.IndexOf(" ") + 1);
            }
            catch
            {
                return "";
            }
        }

        public string GetName()
        {
            try
            {
                var text = _text.First((x) => x.StartsWith("Name: "));
                return text.Substring(text.IndexOf(" ") + 1);
            }
            catch
            {
                return "";
            }
        }
    }
}