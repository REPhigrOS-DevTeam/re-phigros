using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MainCore.Data;
using MainCore.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utilities;

namespace MainCore.PostProcessing
{
    public class ExtraShaderProvider : MonoBehaviour
    {
        private List<BpmEvent> bpms = new();

        private Extra data;
        private List<int> enabledShaders = new();
        private List<string> notExistShader = new();

        private List<string> shaderNames = new();
        private Dictionary<string, Shader> shaders = new();
        private Material[] totalMaterials;
        private float currentTime => Main.Instance.progressManager.NowTime;

        public bool IsGlobal { get; set; }

        private void Awake()
        {
            data = GlobalSetting.CurrentBeatmapInfo.ExtraEvents;
            Arrangement();
            totalMaterials = new Material[data.Effects.Count];
            for (var i = 0; i < data.Effects.Count; i++)
            {
                var e = data.Effects[i];
                e.shader = e.shader.Substring(e.shader.Replace('\\', '/')
                    .IndexOf("/", StringComparison.InvariantCulture) + 1).FirstToLowerInvariant();
                LoadShader(i, e.shader);
                if (e.vars.Count == 0) return;
                e.varTypes = new Effect.ExtraPropertyType[e.vars.Count];
                int j = 0;
                foreach (var v in e.vars)
                {
                    e.varTypes[j] = PreloadProperty(e.shader, v.Key, v.Value);
                    j++;
                }
            }
        }

        private void Update()
        {
            if (!GlobalSetting.GameStarted)
            {
                return;
            }

            enabledShaders.Clear();
            for (var i = 0; i < data.Effects.Count; i++)
            {
                var e = data.Effects[i];
                if (!e.global && IsGlobal)
                {
                    continue;
                }

                if (e.startTime > currentTime || e.endTime < currentTime || !shaderNames.Contains(e.shader))
                {
                    continue;
                }

                enabledShaders.Add(i);
                int j = 0;
                foreach (var v in e.vars)
                {
                    AnalyzeProperty(totalMaterials[i], v.Key, v.Value, e.varTypes[j]);
                    j++;
                }
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            RenderTexture tempSrc =
                RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            RenderTexture tempDst =
                RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
#else
            RenderTexture tempSrc =
                RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
            RenderTexture tempDst =
                RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
#endif
            //^grab two temp textures that are the same as the source;

            Graphics.Blit(source, tempSrc); //blit the source into the tempSrc;

            for (int j = 0; j < enabledShaders.Count; j++)
            {
                //for all the materials;
                if ((float) j % 2.0f == 0.0f)
                {
                    //if i is even blit from src to dst, if not then dst to src.
                    Graphics.Blit(tempSrc, tempDst, totalMaterials[enabledShaders[j]]);
                }
                else
                {
                    Graphics.Blit(tempDst, tempSrc, totalMaterials[enabledShaders[j]]);
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

        void Arrangement()
        {
            data.Bpm.OrderBy(x => x.time.Frac()).ToList().ForEach(x =>
            {
                bpms.Add(new BpmEvent(x.bpm, x.time.Frac()));
                if (bpms.Count >= 2)
                {
                    bpms[^2].end = bpms[^1].start;
                }
            });
            data.Effects.ForEach(x =>
            {
                x.startTime = RecalcTime(x.start.Frac());
                x.endTime = RecalcTime(x.end.Frac());
            });
            data.Effects.ForEach(x => x.shader = ArrangeShaderName(x.shader));
        }

        private void LoadShader(int id, string shaderName)
        {
            if (shaders.ContainsKey(shaderName))
            {
                totalMaterials[id] = Instantiate(new Material(shaders[shaderName]));
                return;
            }

            if (notExistShader.Contains(shaderName))
            {
                totalMaterials[id] = null;
                return;
            }

            if (!shaders.TryGetValue(shaderName, out var shader))
            {
                shader = Shader.Find($"Phira/{shaderName}");
                shaders.Add(shaderName, shader);
            }

            if (shader != null)
            {
                shaderNames.Add(shaderName);
                totalMaterials[id] = new Material(shader);
                Debug.Log($"[ExtraShaderProvider] Shader {shaderName} loaded.");
                return;
            }
            
            notExistShader.Add(shaderName);
            totalMaterials[id] = null;
            Debug.LogError($"[ExtraShaderProvider] Error loading shader : {shaderName}, maybe not builtin shaders.");
        }

        private Effect.ExtraPropertyType PreloadProperty(string shaderName, string propertyName, JToken property)
        {
            try
            {
                property.ToObject<List<Value>>();
                return Effect.ExtraPropertyType.ExtraList;
            }
            catch
            {
                try
                {
                    property.ToObject<float>();
                    return Effect.ExtraPropertyType.Decimal;
                }
                catch
                {
                    Debug.LogError($"[ExtraShaderProvider] Property {propertyName} in {shaderName} of type {property.GetType()} is not supported");
                    return Effect.ExtraPropertyType.Undefined;
                }
            }
        }

        private void AnalyzeProperty(Material mat, string propertyName, JToken property, Effect.ExtraPropertyType type)
        {
            switch (type)
            {
                case Effect.ExtraPropertyType.Undefined:
                    break;
                case Effect.ExtraPropertyType.Decimal:
                    var f1 = property.ToObject<float>();
                    // Debug.Log(f1);
                    mat.SetFloat($"_{propertyName}", f1);
                    break;
                case Effect.ExtraPropertyType.ExtraList:
                    var values = property.ToObject<List<Value>>();
                    foreach (var value in values)
                    {
                        if (value.realStartTime == -1)
                        {
                            value.realStartTime = RecalcTime(value.startTime.Frac());
                            value.realEndTime = RecalcTime(value.endTime.Frac());

                            if (value.realEndTime == value.realStartTime)
                            {
                                value.realEndTime += .01f;
                            }
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
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
                }
            }

            return timePhi;
        }

        private string ArrangeShaderName(string shaderName)
        {
            shaderName = shaderName.Trim();
            if (shaderName.Contains(".fs"))
            {
                shaderName = shaderName.TrimStart('/', '\\');
                shaderName = Path.GetFileNameWithoutExtension(shaderName).ToLower();
            }

            return shaderName;
        }
    }
}