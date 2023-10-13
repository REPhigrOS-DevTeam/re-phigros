using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MainCore.Data
{
    [Serializable]
    public class Bpm
    {
        public int[] time { get; set; }
        public float bpm { get; set; }
    }

    [Serializable]
    public class BpmEvent
    {
        public float start;
        public float end;
        public float bpm;

        public BpmEvent(float b, float s)
        {
            bpm = b;
            start = s;
            end = 1e9f;
           
        }
    }

    [Serializable]
    public class Video
    {
        public string path { get; set; }
        public int[] time { get; set; }
        public float realTime { get; set; }
        public string scale { get; set; }
        public float alpha { get; set; }
        public float dim { get; set; }
    }

    [Serializable]
    public class Effect
    {
        public int[] start { get; set; }
        public float startTime { get; set; }
        public int[] end { get; set; }
        public float endTime { get; set; }
        public string shader { get; set; }
        public bool global { get; set; }
        public Dictionary<string, JToken> vars { get; set; }
        public ExtraPropertyType[] varTypes { get; set; }

        public enum ExtraPropertyType
        {
            Undefined = 0,
            Decimal = 1,
            ExtraList = 2
        }
    }

    [Serializable]
    public class Value
    {
        public int[] startTime { get; set; }
        public int[] endTime { get; set; }
        public int easingType { get; set; } = 1;
        public float easingLeft { get; set; } = 0;
        public float easingRight { get; set; } = 1;
        public float start { get; set; }
        public float end { get; set; }

        public float realStartTime { get; set; } = -1;
        public float realEndTime { get; set; } = -1;
    }

    [Serializable]
    public class Extra
    {
        [JsonProperty("bpm")] public List<Bpm> Bpm { get; set; }
        [JsonProperty("videos")] public List<Video> Videos { get; set; }
        [JsonProperty("effects")] public List<Effect> Effects { get; set; }
    }

    public class ValueConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            Debug.Log("123");
            return sourceType == typeof(double);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type sourceType)
        {
            Debug.Log("123");
            return sourceType == typeof(double);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            List<Value> result = new(1);
            result[0].start = (float) value;
            result[0].end = (float) value;
            result[0].easingType = 0;
            result[0].startTime = new[] {0, 0, 0};
            result[0].endTime = new[] {10000, 0, 0};
            return result;
        }

        /*public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(int);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is int)
            {
                return ((int)value).ToString();
            }
            else
            {
                throw new ArgumentException("Invalid input");
            }
        }*/
    }
}