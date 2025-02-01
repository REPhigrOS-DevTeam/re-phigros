using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Editor.Scenes
{
    [Serializable]
    public class SceneNode
    {
        public string folderName;
        public string pathFolder;
        public SceneAsset[] scenes;
        public SceneNode[] nodes;

        public Dictionary<string, string> Get(IEnumerable<string> rootFolders, IEnumerable<string> pathFolders)
        {
            string[] array = rootFolders.Append(folderName).ToArray();
            string[] array2 = pathFolders.Append(pathFolder).ToArray();
            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (SceneNode node in nodes)
            {
                Dictionary<string, string> subItems = node.Get(array, array2);
                foreach (KeyValuePair<string,string> pair in subItems)
                {
                    items.Add(pair.Key, pair.Value);
                }
            }
            
            foreach (SceneAsset sceneAsset in scenes)
            {
                items.Add(string.Join("/", array.Append(SceneList.ToUnityName(SceneList.Prefix + sceneAsset.name))), string.Join("/", array2.Append(sceneAsset.name)));
            }

            return items;
        }
    }
}