#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Editor
{
    public static class SceneTools
    {
        [MenuItem("Scene Tools/Into Entry")]
        private static void IntoEntry()
        {
            IntoScene("EntryScene");
        }

        [MenuItem("Scene Tools/Into ChartSelector")]
        private static void IntoChartSelector()
        {
            IntoScene("ChartSelectorScene");
        }

        [MenuItem("Scene Tools/Into Playing")]
        private static void IntoPlaying()
        {
            IntoScene("PlayingScene");
        }

        [MenuItem("Scene Tools/Into Level Over 1")]
        private static void IntoLevelOver1()
        {
            IntoScene("LevelOver 1");
        }
        
        [MenuItem("Scene Tools/Into DSP")]
        private static void IntoDSP()
        {
            IntoScene("DSPScene");
        }

        [MenuItem("Scene Tools/Into Settings")]
        private static void IntoSettings()
        {
            IntoScene("SettingsScene");
        }

        [MenuItem("Scene Tools/Into LoadInto")]
        private static void IntoLoadInto()
        {
            IntoScene("LoadInto");
        }
        
        [MenuItem("Scene Tools/Into WahtThe")]
        private static void IntoWahtThe()
        {
            IntoScene("WahtThe");
        }
        
                
        [MenuItem("Scene Tools/Unbuild/Into Loading")]
        private static void IntoLoading()
        {
            IntoScene("LoadingScene");
        }
        
        [MenuItem("Scene Tools/Unbuild/Into Result")]
        private static void IntoResult()
        {
            IntoScene("ResultScene");
        }

        [MenuItem("Scene Tools/Unbuild/Into Network")]
        private static void IntoNetwork()
        {
            IntoScene("TEST/NetworkTest");
        }
        
                [MenuItem("Scene Tools/Into Login")]
                private static void IntoLogin()
                {
                    IntoScene("LoginScene");
                }

        private static void IntoScene(string name)
        {
            if (EditorApplication.isPlaying)
            {
                SceneManager.LoadSceneAsync($"Scenes/{name}");
                return;
            }
            EditorSceneManager.OpenScene($"Assets/Scenes/{name}.unity");
        }
    }
}

#endif
