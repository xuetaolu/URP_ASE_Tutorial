// @author : xue
// @created : 2023,07,04,14:06
// @desc:


using System.IO;
using Common;
using UnityEditor;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(TerrainAltasMaskGenerator))]
public class TerrainAltasMaskGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        using (new EditorGUILayout.VerticalScope())
        {
            TerrainAltasMaskGenerator t =  target as TerrainAltasMaskGenerator;
            GUI.enabled = t.generateMaskRT != null;
            if (GUILayout.Button("导出Mask.png"))
            {
                ExportMaskPng();
            }

            GUI.enabled = true;
        }
    }
    
    public void ExportMaskPng()
    {
        TerrainAltasMaskGenerator t =  target as TerrainAltasMaskGenerator;
        RenderTexture rt = t.generateMaskRT;
        if (rt == null)
            return;
        
        
        // 保存的路径
        string output_path;
        TerrainData terrainData = t.m_TerrainData;
        if (terrainData != null)
        {
            string input_path = AssetDatabase.GetAssetPath(terrainData);
            string input_folder = Path.GetDirectoryName(input_path);
            string input_file_name_no_ext = Path.GetFileNameWithoutExtension(input_path);
            output_path = $"{input_folder}/{input_file_name_no_ext}_genMask.png";
        }
        else
        {
            output_path = "Assets/genMask.png";
        }
        
        // 保存 RT 到 png
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            GLUtils.ReadPixels(tex, rt);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(output_path, bytes);
            objcleaner.Destroy(tex);
        }
        
        // 设置导入
        {
            // 设置贴图格式
            AssetDatabase.Refresh();
            var imp = AssetImporter.GetAtPath(output_path) as TextureImporter;
            imp.textureType = TextureImporterType.Default;
            imp.mipmapEnabled = false;
            // var androidOverrides = imp.GetPlatformTextureSettings("Android");
            // androidOverrides.overridden = true;
            // // androidOverrides.format = TextureImporterFormat.ASTC_4x4;
            // imp.SetPlatformTextureSettings(androidOverrides);
            imp.alphaSource = TextureImporterAlphaSource.None;
            imp.SaveAndReimport();
        }
        
        // ping一下结果
        {
            var tex = AssetDatabase.LoadAssetAtPath(output_path, typeof(Texture2D));
            EditorGUIUtility.PingObject(tex);
        }
    }
}