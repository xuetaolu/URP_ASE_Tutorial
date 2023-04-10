#if UNITY_EDITOR


using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.IO;

namespace BrainFailProductions.PolyFew
{

    [ScriptedImporter(1, "atim")]
    public class AttributesImage : ScriptedImporter
    {

        [System.Serializable]
        public class Attributes
        {
            public int width;
            public int height;
            public TextureFormat tFormat;
            public bool mips;
            public Color[] colors;
        }

        public override void OnImportAsset(AssetImportContext impContext)
        {
            Attributes attributes = JsonUtility.FromJson<Attributes>(File.ReadAllText(impContext.assetPath));

            Texture2D texture = new Texture2D(attributes.width, attributes.height, attributes.tFormat, attributes.mips, true);
            texture.SetPixels(attributes.colors);
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
#if !UNITY_2017_3_OR_NEWER
            impContext.AddSubAsset("AttributesImage", texture);
            impContext.SetMainAsset("AttributesImage", texture);
#else
            impContext.AddObjectToAsset("AttributesImage", texture);
            impContext.SetMainObject(texture);
#endif

        }

        public static void BurnToAttributesImg(Texture2D texture, string path)
        {
            Attributes data = new Attributes();
            data.width = texture.width;
            data.height = texture.height;
            data.tFormat = texture.format;
            data.mips = texture.mipmapCount > 1;
            data.colors = texture.GetPixels();

            System.IO.File.WriteAllText(path, JsonUtility.ToJson(data));
        }

    }
}

#endif