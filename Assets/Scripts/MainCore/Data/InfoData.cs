namespace MainCore.Data
{
    public class PhiraChartInfoData
    {
        public string name = "";
        public string level = "";
        public string charter = "";
        public string composer = "";
        public string illustrator = "";
        public string chart = "";
        public string music = "";
        public string illustration = "";
        public float offset = 0.0f; //单位：秒
    }

    public class PhiraSkinInfoData
    {
        // 必要
        public string name;
        public string author;
        public int[] hitFx;
        public int[] holdAtlas;
        public int[] holdAtlasMH;
        // 非必要
        public string description = "";
        public float hitFxDuration = 0.5f;
        public float hitFxScale = 1.0f;
        public bool hitFxRotate = false;
        public bool hitFxTinted = true;
        public bool hideParticles = false;
        public bool holdKeepHead = false;
        public bool holdRepeat = false;
        public bool holdCompact = false;
        public string colorPerfect =  "0xfffeffad"; // ARGB, phira: 0xe1ffec9f
        public string colorGood = "0xff8cecff"; // ARGB, phira: 0xebb4e1ff
    }
    
    public class LchzhInfo
    {
        [CsvHelper.Configuration.Attributes.Name("Chart")]
        public string Chart { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Music")]
        public string Music { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Image")]
        public string Image { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Name")]
        public string Name { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Artist")]
        public string Artist { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Level")]
        public string Level { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Illustrator")]
        public string Illustrator { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Charter")]
        public string Charter { get; set; }

        [CsvHelper.Configuration.Attributes.Name("AspectRatio")]
        public string AspectRatio { get; set; } = 16f / 9f + "";

        [CsvHelper.Configuration.Attributes.Name("NoteScale")]
        public string NoteScale { get; set; } = "1.0";


        [CsvHelper.Configuration.Attributes.Name("GlobalAlpha")]
        public string GlobalAlpha { get; set; } = "0.6";
    }
    
    public class LchzhInfoOld
    {
        [CsvHelper.Configuration.Attributes.Name("Chart")]
        public string Chart { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Music")]
        public string Music { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Image")]
        public string Image { get; set; }
        
        [CsvHelper.Configuration.Attributes.Name("AspectRatio")]
        public string AspectRatio { get; set; } = 16f / 9f + "";
        
        [CsvHelper.Configuration.Attributes.Name("ScaleRatio")]
        public string NoteScale { get; set; } = "1.0";
        
        [CsvHelper.Configuration.Attributes.Name("GlobalAlpha")]
        public string GlobalAlpha { get; set; } = "0.6";

        [CsvHelper.Configuration.Attributes.Name("Name")]
        public string Name { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Level")]
        public string Level { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Illustrator")]
        public string Illustrator { get; set; }
        
        [CsvHelper.Configuration.Attributes.Name("Designer")]
        public string Charter { get; set; }
    }
}
