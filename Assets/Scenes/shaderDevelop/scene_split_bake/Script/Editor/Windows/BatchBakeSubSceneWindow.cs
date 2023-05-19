// @author : xue
// @created : 2023,05,19,15:57
// @desc:

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace scene_split_bake
{

    public class BatchBakeSceneHelper
    {
        public static void BakeMultipleSceneGroups(string[][] groups)
        {
            if (groups.Length <= 0)
                return;
            
            SceneSetup[] sceneSetup = EditorSceneManager.GetSceneManagerSetup();
            
            int index = 0;

            Action callback = null;
            callback = () =>
            {
                // 还有需要bake的内容
                if (index < groups.Length)
                {
                    string[] paths = groups[index];
                    index++;
                    BakeMultipleScenesInner(paths, callback);
                }
                else
                {
                    // 全部bake结束
                    if (sceneSetup.Length != 0) EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
                }
            };

            callback();
        }

        /// <summary>
        ///   <para>Bakes an array of Scenes.</para>
        ///   <para>OnBakeFinishCallback, callback after one group of scenes baked</para>
        /// </summary>
        /// <param name="paths">The path of the Scenes that should be baked.</param>
        private static void BakeMultipleScenesInner(string[] paths, Action OnBakeFinishCallback)
        {
            if (paths.Length == 0) return;
            for (int index1 = 0; index1 < paths.Length; ++index1)
            {
                for (int index2 = index1 + 1; index2 < paths.Length; ++index2)
                {
                    if (paths[index1] == paths[index2]) throw new Exception("no duplication of scenes is allowed");
                }
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            
            // SceneSetup[] sceneSetup = EditorSceneManager.GetSceneManagerSetup();
            Action OnBakeFinish = (Action)null;
            OnBakeFinish = (Action)(() =>
            {
                EditorSceneManager.SaveOpenScenes();
                Lightmapping.bakeCompleted -= OnBakeFinish;
                
                OnBakeFinishCallback();
                // 由外部callback恢复场景
                // if (sceneSetup.Length != 0) EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
            });
            EditorSceneManager.SceneOpenedCallback BakeOnAllOpen = (EditorSceneManager.SceneOpenedCallback)null;
            BakeOnAllOpen = (EditorSceneManager.SceneOpenedCallback)((scene, loadSceneMode) =>
            {
                if (EditorSceneManager.loadedSceneCount != paths.Length) return;
                Lightmapping.BakeAsync();
                Lightmapping.bakeCompleted += OnBakeFinish;
                EditorSceneManager.sceneOpened -= BakeOnAllOpen;
            });
            EditorSceneManager.sceneOpened += BakeOnAllOpen;
            EditorSceneManager.OpenScene(paths[0]);
            for (int index = 1; index < paths.Length; ++index)
                EditorSceneManager.OpenScene(paths[index], OpenSceneMode.Additive);
        }
    }

    [System.Serializable]
    public class BatchBakeSubSceneWindow : EditorWindow
    {
        public SceneAsset lightScene;

        public List<SceneAsset> subScenes = new List<SceneAsset>() { };

        [MenuItem("Tool/BatchBakeSubScene")]
        private static void ShowWindow()
        {
            var window = GetWindow<BatchBakeSubSceneWindow>();
            window.titleContent = new GUIContent("BatchBakeSubScene");
            window.Show();
        }

        private void OnGUI()
        {
            SerializedObject so = new SerializedObject(this);

            EditorGUILayout.PropertyField(so.FindProperty(nameof(lightScene)), true);
            var o = so.FindProperty(nameof(subScenes));
            EditorGUILayout.PropertyField(o, true);

            // if (so.hasModifiedProperties)
            so.ApplyModifiedProperties();
            so.Dispose();

            if (GUILayout.Button("Bake"))
            {
                List<String[]> groups = new List<string[]>();
                foreach (var scene in subScenes)
                {
                    if (scene != null)
                    {
                        List<String> tmppaths = new List<string>();
                        tmppaths.Add(AssetDatabase.GetAssetPath(scene));
                        tmppaths.Add(AssetDatabase.GetAssetPath(lightScene));
                        // BatchBakeSceneHelper.BakeMultipleScenes();
                        groups.Add(tmppaths.ToArray());
                    }
                }
                
                BatchBakeSceneHelper.BakeMultipleSceneGroups(groups.ToArray());
            }
        }




    }
}