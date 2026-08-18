/////////////////////////////////////////////////////////////////////////
//                                                                     //
//   This file is auto-generated. DO NOT MODIFY IT MANUALLY.           //
//   See Assets/Editor/Scenes/SceneList.cs for more information.       //
//                                                                     //
/////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Editor.Scenes
{
    public static class SceneTools
    {
        [MenuItem("Scene Tools/Into Entry Scene", false, 0)]
        private static void IntoEntryScene()
        {
            IntoScene("EntryScene");
        }

        [MenuItem("Scene Tools/Into Login Scene", false, 1)]
        private static void IntoLoginScene()
        {
            IntoScene("LoginScene");
        }

        [MenuItem("Scene Tools/Into Main Scene", false, 2)]
        private static void IntoMainScene()
        {
            IntoScene("MainScene");
        }

        [MenuItem("Scene Tools/Into Beatmap Select Scene", false, 3)]
        private static void IntoBeatmapSelectScene()
        {
            IntoScene("BeatmapSelectScene");
        }

        [MenuItem("Scene Tools/Into Playing Scene", false, 4)]
        private static void IntoPlayingScene()
        {
            IntoScene("PlayingScene");
        }

        [MenuItem("Scene Tools/Into Level Over 1", false, 5)]
        private static void IntoLevelOver_1()
        {
            IntoScene("LevelOver 1");
        }

        [MenuItem("Scene Tools/Into Dspscene", false, 6)]
        private static void IntoDSPScene()
        {
            IntoScene("DSPScene");
        }

        [MenuItem("Scene Tools/Into Settings Scene", false, 7)]
        private static void IntoSettingsScene()
        {
            IntoScene("SettingsScene");
        }

        [MenuItem("Scene Tools/Into Loading Scene", false, 8)]
        private static void IntoLoadingScene()
        {
            IntoScene("LoadingScene");
        }

        [MenuItem("Scene Tools/Into Waht The", false, 9)]
        private static void IntoWahtThe()
        {
            IntoScene("WahtThe");
        }

        [MenuItem("Scene Tools/Into Network Test", false, 10)]
        private static void IntoNetworkTest()
        {
            IntoScene("NetworkTest");
        }

        [MenuItem("Scene Tools/Into About Scene", false, 11)]
        private static void IntoAboutScene()
        {
            IntoScene("AboutScene");
        }

        [MenuItem("Scene Tools/Into Character Adjust Scene", false, 12)]
        private static void IntoCharacterAdjustScene()
        {
            IntoScene("CharacterAdjustScene");
        }

        private static void IntoScene(string name)
        {
            if (EditorApplication.isPlaying)
            {
                SceneManager.LoadSceneAsync($"Scenes/{name}");
                return;
            }

            if (SceneManager.GetActiveScene().isDirty)
            {
                if (EditorUtility.DisplayDialog("提示", "场景未保存，是否保存？", "是", "否"))
                {
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }
            }

            EditorSceneManager.OpenScene($"Assets/Scenes/{name}.unity");
        }
    }
}
