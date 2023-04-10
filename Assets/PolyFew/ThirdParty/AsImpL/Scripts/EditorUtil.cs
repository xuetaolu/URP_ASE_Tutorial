#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace BrainFailProductions.PolyFew.AsImpL
{
    public class EditorUtil
    {
        /// <summary>
        /// Create a folder, if not yet created, in the asset database
        /// </summary>
        /// <param name="path">path that will contain the new folder</param>
        /// <param name="folder">name of the folder</param>
        public static void CreateAssetFolder(string path, string folder)
        {
            if (!AssetDatabase.IsValidFolder(path + "/" + folder))
            {
                AssetDatabase.CreateFolder(path, folder);
            }
        }


        /// <summary>
        /// Capture a screenshot, naming its file automatically with date and time,
        /// then shows the file in the file manager (e.g. Explorer)
        /// </summary>
        /// <param name="prefix">prefix at the beginning of the file name</param>
        public static void AutoCaptureScreenshot(string prefix)
        {
            string fileName = prefix + "-" + DateTime.Now.ToString("s").Replace('T', '_').Replace(':', '-') + ".png";
#if UNITY_2017_1_OR_NEWER
            ScreenCapture.CaptureScreenshot(fileName);
#else
            Application.CaptureScreenshot(fileName);
#endif
            Debug.Log("Screenshot saved to " + Application.dataPath + "/" + fileName);
            EditorUtility.RevealInFinder(Application.dataPath);
        }


        /// <summary>
        /// Set a texture in the asset database as readable to perform image analisys
        /// </summary>
        /// <param name="texAssetPath"></param>
        public static void SetTextureReadable(string texAssetPath)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(texAssetPath) as TextureImporter;
            if (textureImporter)
            {
                Debug.LogFormat("Changing import settings for texture {0}...", texAssetPath);
                textureImporter.isReadable = true;
                textureImporter.alphaIsTransparency = true;
                textureImporter.SaveAndReimport();
            }
            else
            {
                Debug.LogError("Texture importer for not found for " + texAssetPath);
            }
        }


        /// <summary>
        /// Save a new texture to the asset databse and reimport it
        /// </summary>
        /// <param name="texture">new texture</param>
        /// <param name="texAssetPath">path in the asset database</param>
        /// <param name="postFix">postfix appended to the file name</param>
        /// <param name="isNormalMap">if true the texture importer is set to normal map</param>
        public static void SaveAndReimportPngTexture(ref Texture2D texture, string texAssetPath, string postFix, bool isNormalMap = false)
        {
            string newTexAssetPath = Path.GetDirectoryName(texAssetPath) + "/" + Path.GetFileNameWithoutExtension(texAssetPath) + postFix + ".png";
            if (newTexAssetPath != texAssetPath)
            {
                Debug.LogFormat("Storing texture {0}...", newTexAssetPath);
                // Encode texture into PNG
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + "/../" + newTexAssetPath, bytes);
                //texture.LoadImage(bytes, false);
                //AssetDatabase.CreateAsset(texture, newTexAssetPath);
                //AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //EditorUtil.SetTextureReadable(texture, newTexAssetPath);
                texAssetPath = newTexAssetPath;
                if (isNormalMap)
                {
                    TextureImporter newTextureImporter = AssetImporter.GetAtPath(texAssetPath) as TextureImporter;
                    newTextureImporter.textureType = TextureImporterType.NormalMap;
                    newTextureImporter.SaveAndReimport();
                }
                //newTextureImporter.isReadable = true;
                //AssetDatabase.CreateAsset(newTextureImporter, newTexAssetPath);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texAssetPath);
            }
        }

    }
}
#endif
