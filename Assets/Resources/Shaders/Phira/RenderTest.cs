using System.Collections.Generic;
using System.IO;
using System.Linq;
using MainCore.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utilities;

public class RenderTest : MonoBehaviour
{
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [SerializeField] private TextAsset extraJson;
    [SerializeField] private float currentTime;
    private List<BpmEvent> bpms = new();

    private Extra data;
    private List<string> enabledShaders = new();

    private Dictionary<string, Material> materials = new();
    private Dictionary<string, Shader> shaders = new();

    private void Awake()
    {
        data = JsonConvert.DeserializeObject<Extra>(extraJson.text);
        ArrangeTime();
    }

    private void Update()
    {
        enabledShaders.Clear();
        foreach (var e in data.Effects)
        {
            if (e.startTime > currentTime || e.endTime < currentTime)
            {
                continue;
            }

            LoadShader(e.shader);
            if (materials.ContainsKey(e.shader))
            {
                enabledShaders.Add(e.shader);
                foreach (var v in e.vars)
                {
                    AnalyzeProperty(materials[e.shader], v.Key, (JArray) v.Value);
                }
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture tempSrc = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
        RenderTexture tempDst = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
        //^grab two temp textures that are the same as the source;

        Graphics.Blit(source, tempSrc); //blit the source into the tempSrc;

        for (int i = 0; i < enabledShaders.Count; i++)
        {
            //for all the materials;
            if ((float) i % 2.0f == 0.0f)
            {
                //if i is even blit from src to dst, if not then dst to src.
                Graphics.Blit(tempSrc, tempDst, materials[enabledShaders[i]]);
            }
            else
            {
                Graphics.Blit(tempDst, tempSrc, materials[enabledShaders[i]]);
            }
        }

        if ((float) enabledShaders.Count % 2.0f == 0.0f)
        {
            //if the total number of materials is even;
            //then we blit from the tempSrc;
            Graphics.Blit(tempSrc, destination); //final blit from tempSrc to dest;
        }
        else
        {
            //if not;
            //then we blit from the tempDst;
            Graphics.Blit(tempDst, destination); //final blit from tempDst to dest;
        }

        RenderTexture.ReleaseTemporary(tempSrc);
        RenderTexture.ReleaseTemporary(tempDst);
        //^release the temp textures;
    }

    void ArrangeTime()
    {
        data.Bpm.OrderBy(x => Frac(x.time)).ToList().ForEach(x =>
        {
            bpms.Add(new BpmEvent(x.bpm, Frac(x.time)));
            if (bpms.Count >= 2)
            {
                bpms[^2].end = bpms[^1].start;
            }
        });
        data.Effects.ForEach(x =>
        {
            x.startTime = RecalcTime(Frac(x.start));
            x.endTime = RecalcTime(Frac(x.end));
        });
        data.Effects.ForEach(x => x.shader = ArrangeShaderName(x.shader));
    }

    private void LoadShader(string shaderName)
    {
        if (materials.ContainsKey(shaderName))
        {
            return;
        }

        if (!shaders.TryGetValue(shaderName, out var shader))
        {
            shader = Shader.Find($"Phira/{shaderName}");
            shaders.Add(shaderName, shader);
        }

        if (shader != null)
        {
            materials.Add(shaderName, new Material(shader));
            Debug.Log("123");
            return;
        }

        Debug.LogError($"Error loading shader : {shaderName}, maybe not builtin shaders.");
    }

    private void AnalyzeProperty(Material mat, string propertyName, JArray property)
    {
        try
        {
            var values = property.ToObject<List<Value>>();
            foreach (var value in values)
            {
                if (value.realStartTime == -1)
                {
                    value.realStartTime = RecalcTime(Frac(value.startTime));
                    value.realEndTime = RecalcTime(Frac(value.endTime));
                }

                if (value.realStartTime > currentTime || value.realEndTime < currentTime)
                {
                    continue;
                }

                var f = EaseUtils.GetEaseResult((EaseUtils.EaseType) value.easingType,
                    currentTime - value.realStartTime, value.realEndTime - value.realStartTime,
                    value.start, value.end, value.easingLeft, value.easingRight);
                mat.SetFloat($"_{propertyName}", f);
            }
        }
        catch
        {
            try
            {
                var f = property.ToObject<float>();
                mat.SetFloat($"_{propertyName}", f);
            }
            catch
            {
                Debug.LogError($"Property {propertyName} of type {property.GetType()} is not supported");
            }
        }
    }

    //Utilities
    private float RecalcTime(float time)
    {
        var timePhi = 0f;
        foreach (var i in bpms)
        {
            if (time > i.end)
            {
                timePhi += (i.end - i.start) * (60f / i.bpm);
            }
            else if (time >= i.start)
            {
                timePhi += (time - i.start) * (60f / i.bpm);
                // 这边是不是可以直接break掉
            }
        }

        return timePhi;
    }

    private static float Frac(int[] frac)
    {
        if (frac.Length == 3)
        {
            if (frac.Length == 3) return frac[0] + (float) frac[1] / frac[2];
            return frac[0];
        }

        return frac.Length > 0 ? frac[0] : 0f;
    }

    private string ArrangeShaderName(string shaderName)
    {
        if (shaderName.EndsWith(".fs"))
        {
            shaderName = shaderName.TrimStart('/', '\\');
            shaderName = Path.GetFileNameWithoutExtension(shaderName).ToLower();
        }

        return shaderName;
    }
}