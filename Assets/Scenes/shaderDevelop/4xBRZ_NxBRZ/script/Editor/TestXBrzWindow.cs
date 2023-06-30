// @author : xue
// @created : 2023,06,30,14:12
// @desc:

using System.IO;
using Common;
using UnityEditor;
using UnityEngine;

namespace XBRZ.Editor
{
    public class TestXBrzWindow : EditorWindow
    {
        [SerializeField] Texture2D texture;
        
        [MenuItem("Tool/TestXBrzWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<TestXBrzWindow>();
            window.titleContent = new GUIContent("TestXBrzWindow");
            window.Show();
        }

        private void OnGUI()
        {
            texture = EditorGUILayout.ObjectField("原始 texture", texture, typeof(Texture2D), false) as Texture2D;
            GUI.enabled = texture != null;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成 x2"))
            {
                Generate(texture, XbrzUtils.EScaleMethod.X2);
            }
            if (GUILayout.Button("生成 x4"))
            {
                Generate(texture, XbrzUtils.EScaleMethod.X4);
            }
            if (GUILayout.Button("生成 x6"))
            {
                Generate(texture, XbrzUtils.EScaleMethod.X6);
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void Generate(Texture2D inTexture, XbrzUtils.EScaleMethod inScaleMethod)
        {
            // 缩放的倍率
            int scale = inScaleMethod.scale;
            
            // 保存的路径
            string input_path = AssetDatabase.GetAssetPath(inTexture);
            string input_folder = Path.GetDirectoryName(input_path);
            string input_file_name_no_ext = Path.GetFileNameWithoutExtension(input_path);
            string output_path = $"{input_folder}/{input_file_name_no_ext}_x{scale}.png";
                
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                texture.width * scale,
                texture.height * scale,
                0, RenderTextureFormat.ARGB32 );
            {
                // 放大的贴图渲染到 RT
                XbrzUtils.DoScale(inTexture, renderTexture, inScaleMethod);
                
                // 保存 RT 到 png
                var tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
                GLUtils.ReadPixels(tex, renderTexture);
                var bytes = tex.EncodeToPNG();
                File.WriteAllBytes(output_path, bytes);
                objcleaner.Destroy(tex);
            }
            RenderTexture.ReleaseTemporary(renderTexture);

            // 设置导入
            {
                // 设置贴图格式
                AssetDatabase.Refresh();
                var imp = AssetImporter.GetAtPath(output_path) as TextureImporter;
                imp.textureType = TextureImporterType.Default;
                imp.mipmapEnabled = false;
                imp.alphaIsTransparency = true;
                imp.npotScale = TextureImporterNPOTScale.None;
                imp.SaveAndReimport();
            }

            // ping一下结果
            {
                var tex = AssetDatabase.LoadAssetAtPath(output_path, typeof(Texture2D));
                EditorGUIUtility.PingObject(tex);
            }
            
        }
    }
}