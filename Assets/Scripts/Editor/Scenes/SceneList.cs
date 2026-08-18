using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MainCore.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor.Scenes
{
    public class SceneList : ScriptableObject
    {
        public SceneAsset[] rootScenes;
        public SceneNode[] nodes;

        public const string RootSpace = "Scene Tools";
        public const string Prefix = "Into";

        [MenuItem("RPGR/Refresh Scene Tools")]
        public static void Refresh()
        {
            SceneList sceneList = Resources.Load<SceneList>("Editor/SceneList");
            if (sceneList) sceneList.RefreshSceneTools();
        }
        
        private void RefreshSceneTools()
        {
            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (SceneNode node in nodes)
            {
                Dictionary<string, string> subItems = node.Get(new[]{RootSpace}, Array.Empty<string>());
                foreach (KeyValuePair<string,string> pair in subItems)
                {
                    items.Add(pair.Key, pair.Value);
                }
            }
            
            foreach (SceneAsset sceneAsset in rootScenes)
            {
                items.Add(RootSpace + "/" + ToUnityName(Prefix + sceneAsset.name), sceneAsset.name);
            }
            
            // TODO: 移动SceneTools.cs位置的时候记得修改这里的路径
            StreamWriter streamWriter = new StreamWriter(Application.streamingAssetsPath + "/../Scripts/Editor/Scenes/SceneTools.cs", false, new UTF8Encoding(false));
            streamWriter.WriteLine("/////////////////////////////////////////////////////////////////////////"                                );
            streamWriter.WriteLine("//                                                                     //"                                                                                                   ); 
            streamWriter.WriteLine("//   This file is auto-generated. DO NOT MODIFY IT MANUALLY.           //"                                           );
            streamWriter.WriteLine("//   See Assets/Editor/Scenes/SceneList.cs for more information.       //"                                   );
            streamWriter.WriteLine("//                                                                     //"                                                                                                   );
            streamWriter.WriteLine("/////////////////////////////////////////////////////////////////////////"                                );
            streamWriter.WriteLine(                                                                                                       );
            streamWriter.WriteLine("using UnityEditor;"                                                                                   );
            streamWriter.WriteLine("using UnityEditor.SceneManagement;"                                                                   );
            streamWriter.WriteLine("using UnityEngine.SceneManagement;"                                                                   );
            streamWriter.WriteLine(                                                                                                       );
            streamWriter.WriteLine("namespace Editor.Scenes"                                                                     );
            streamWriter.WriteLine("{"                                                                                                    );
            streamWriter.WriteLine("    public static class SceneTools"                                                                   );
            streamWriter.WriteLine("    {"                                                                                                );
            string[] template = 
            {
                                   "        [MenuItem(\"{0}\", false, {1})]"                                                              ,
                                   "        private static void {0}()"                                                    ,
                                   "        {"                                                                                            ,
                                   "            IntoScene(\"{0}\");"                                                                      ,
                                   "        }"                                                                                            ,
                                   ""                                                                                                     ,
            };
            int index = 0;
            foreach (KeyValuePair<string,string> pair in items)
            {
                for (var i = 0; i < template.Length; i++)
                {
                    streamWriter.WriteLine(i switch
                    {
                        0 => string.Format(template[i], pair.Key, index),
                        1 => string.Format(template[i], Prefix + pair.Value[(pair.Value.LastIndexOf('/') + 1)..].Replace(" ", "_")),
                        3 => string.Format(template[i], pair.Value),
                        _ => template[i]
                    });
                }

                index++;
            }
            streamWriter.WriteLine("        private static void IntoScene(string name)"                                                   );
            streamWriter.WriteLine("        {"                                                                                            );
            streamWriter.WriteLine("            if (EditorApplication.isPlaying)"                                                         );
            streamWriter.WriteLine("            {"                                                                                        );
            streamWriter.WriteLine("                SceneManager.LoadSceneAsync($\"Scenes/{name}\");"                                     );
            streamWriter.WriteLine("                return;"                                                                              );
            streamWriter.WriteLine("            }"                                                                                        );
            streamWriter.WriteLine(                                                                                                       );
            streamWriter.WriteLine("            if (SceneManager.GetActiveScene().isDirty)"                                               );
            streamWriter.WriteLine("            {"                                                                                        );
            streamWriter.WriteLine("                if (EditorUtility.DisplayDialog(\"提示\", \"场景未保存，是否保存？\", \"是\", \"否\"))"     );
            streamWriter.WriteLine("                {"                                                                                    );
            streamWriter.WriteLine("                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());"                     );
            streamWriter.WriteLine("                }"                                                                                    );
            streamWriter.WriteLine("            }"                                                                                        );
            streamWriter.WriteLine(                                                                                                       );
            streamWriter.WriteLine("            EditorSceneManager.OpenScene($\"Assets/Scenes/{name}.unity\");"                           );
            streamWriter.WriteLine("        }"                                                                                            );
            streamWriter.WriteLine("    }"                                                                                                );
            streamWriter.WriteLine("}"                                                                                                    );
            streamWriter.Close();
            AssetDatabase.Refresh();
        }

        public static string ToUnityName(string s)
        {
            return string.Join(" ",
                NamingStrategyUtil.ToUpperSnakeCase(s).Split(NamingStrategyUtil.Underline)
                    .Select(str => NamingStrategyUtil.ToUpperCamelCase(str)));
        }
    }
}