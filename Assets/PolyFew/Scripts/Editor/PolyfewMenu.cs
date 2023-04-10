#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BrainFailProductions.PolyFew
{
    public class PolyfewMenu : MonoBehaviour
    {


#if UNITY_2019_1_OR_NEWER

        [MenuItem("Window/Brainfail Products/PolyFew/Cleanup Missing Scripts")]
        static void CleanupMissingScripts()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;

            for (int a = 0; a < UnityEngine.SceneManagement.SceneManager.sceneCount; a++)
            {

                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(a);

                var rootGameObjects = scene.GetRootGameObjects();

                if (rootGameObjects != null && rootGameObjects.Length > 0)
                {

                    List<GameObject> allObjectsinScene = new List<GameObject>();


                    EditorUtility.DisplayProgressBar("Preprocessing", $"Fetching GameObjects in active scene \"{scene.name}\"", 0);

                    foreach (var gameObject in rootGameObjects)
                    {
                        var childObjects = gameObject.GetComponentsInChildren<Transform>();

                        if (childObjects != null && childObjects.Length > 0)
                        {
                            foreach (var obj in childObjects)
                            {
                                if (obj != null) { allObjectsinScene.Add(obj.gameObject); }
                            }
                        }

                    }

                    EditorUtility.ClearProgressBar();


                    for (int b = 0; b < allObjectsinScene.Count; b++)
                    {

                        var gameObject = allObjectsinScene[b];

                        EditorUtility.DisplayProgressBar("Removing missing script references", $"Inspecting GameObject  {b + 1}/{allObjectsinScene.Count} in active scene \"{scene.name}\"", (float)(b) / allObjectsinScene.Count);

                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    }

                    EditorSceneManager.MarkSceneDirty(scene);

                    EditorUtility.ClearProgressBar();
                }

                EditorUtility.ClearProgressBar();
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("Operation Completed", "Successfully removed missing script references. Please save all currently open scenes to keep these changes persistent", "Ok");

        }


        public static void CleanMissingScripts()
        {
            CleanupMissingScripts();
        }
#endif


        [MenuItem("Window/Brainfail Products/PolyFew/Remove All Scripts")]
        static void RemoveAllPolyFewScripts()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;

            for (int a = 0; a < UnityEngine.SceneManagement.SceneManager.sceneCount; a++)
            {

                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(a);

                var rootGameObjects = scene.GetRootGameObjects();

                if (rootGameObjects != null && rootGameObjects.Length > 0)
                {

                    List<GameObject> allObjectsinScene = new List<GameObject>();


                    EditorUtility.DisplayProgressBar("Preprocessing", $"Fetching GameObjects in active scene \"{scene.name}\"", 0);

                    foreach (var gameObject in rootGameObjects)
                    {
                        var childObjects = gameObject.GetComponentsInChildren<Transform>();

                        if (childObjects != null && childObjects.Length > 0)
                        {
                            foreach (var obj in childObjects)
                            {
                                if (obj != null) { allObjectsinScene.Add(obj.gameObject); }
                            }
                        }
                    }

                    EditorUtility.ClearProgressBar();


                    for (int b = 0; b < allObjectsinScene.Count; b++)
                    {
                        var polyfewComponent = allObjectsinScene[b].GetComponent<PolyFew>();
                        var objectMatLinks  = allObjectsinScene[b].GetComponent<ObjectMaterialLinks>();

                        EditorUtility.DisplayProgressBar("Removing polyfew components", $"Inspecting GameObject  {b + 1}/{allObjectsinScene.Count} in active scene \"{scene.name}\"", (float)(b) / allObjectsinScene.Count);

                        if(polyfewComponent != null) { DestroyImmediate(polyfewComponent); }
                        if(objectMatLinks != null)  { DestroyImmediate(objectMatLinks); }
                    }

                    EditorSceneManager.MarkSceneDirty(scene);

                    EditorUtility.ClearProgressBar();
                }

                EditorUtility.ClearProgressBar();
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("Operation Completed", "Successfully removed lod backup components. Please save all currently open scenes to keep these changes persistent", "Ok");

        }



#if UNITY_2019_1_OR_NEWER


        [MenuItem("Assets/Brainfail Products/PolyFew/Clean Missing Scripts From Prefabs")]
        public static void CleanMissingScriptsFromFolders()
        {
            string folderPath = null;
            string[] assetPaths = null;

            if (Selection.activeObject != null)
            {
                folderPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            }

            if(!string.IsNullOrWhiteSpace(folderPath))
            {
                assetPaths = AssetDatabase.FindAssets("t:GameObject", new string[] { folderPath });
            }

            else
            {
                assetPaths = AssetDatabase.FindAssets("t:GameObject");
            }


            EditorUtility.DisplayProgressBar("Removing missing components", "Please wait...", 0);


            foreach(var guid in assetPaths)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                var type = PrefabUtility.GetPrefabAssetType(obj);


                if (type == PrefabAssetType.Model || type == PrefabAssetType.NotAPrefab)
                {
                    continue;
                }


                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

                if (prefabRoot != null)
                {
                    Transform transform = prefabRoot.transform;
                    var children = transform.GetComponentsInChildren<Transform>();

                    foreach (Transform child in children)
                    {
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string tempPath = path.Replace(fileName, GUID.Generate().ToString());
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, tempPath);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    UtilityServices.OverwriteAssetWith(path, tempPath, false);
                    bool success1 = FileUtil.DeleteFileOrDirectory(tempPath);
                    bool success2 = FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(tempPath) + Path.DirectorySeparatorChar + Path.GetFileName(tempPath) + ".meta");
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }


#endif


#if UNITY_2018_3_OR_NEWER

        [MenuItem("Assets/Brainfail Products/PolyFew/Remove Polyfew Scripts From Prefabs")]
        public static void RemovePolyfewScriptsFromPrefabs()
        {
            string folderPath = null;
            string[] assetPaths = null;

            if (Selection.activeObject != null)
            {
                folderPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            }

            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                assetPaths = AssetDatabase.FindAssets("t:GameObject", new string[] { folderPath });
            }

            else
            {
                assetPaths = AssetDatabase.FindAssets("t:GameObject");
            }


            EditorUtility.DisplayProgressBar("Removing polyfew scripts", "Please wait...", 0);


            foreach (var guid in assetPaths)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                var type = PrefabUtility.GetPrefabAssetType(obj);


                if (type == PrefabAssetType.Model || type == PrefabAssetType.NotAPrefab)
                {
                    continue;
                }

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

                if (prefabRoot != null)
                {
                    Transform transform = prefabRoot.transform;
                    var children = transform.GetComponentsInChildren<Transform>();

                    foreach (Transform child in children)
                    { 
                        var host = child.GetComponent<PolyFew>();
                        var objectMatLinks = child.GetComponent<ObjectMaterialLinks>();
                        if (host != null) { DestroyImmediate(host); }
                        if(objectMatLinks != null) { DestroyImmediate(objectMatLinks); }
                    }

                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string tempPath = path.Replace(fileName, GUID.Generate().ToString());
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, tempPath);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    UtilityServices.OverwriteAssetWith(path, tempPath, false);
                    bool success1 = FileUtil.DeleteFileOrDirectory(tempPath);
                    bool success2 = FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(tempPath) + Path.DirectorySeparatorChar + Path.GetFileName(tempPath) + ".meta");
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

#endif

        public static void RemovePolyFewScripts()
        {
            RemoveAllPolyFewScripts();
        }
    }

}

#endif
