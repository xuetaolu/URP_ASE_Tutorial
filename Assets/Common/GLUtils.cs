// @author : xue
// @created : 2023,06,30,14:16
// @desc:

using UnityEngine;

namespace Common
{
    public class GLUtils
    {
        public static void ReadPixels(Texture2D dst, RenderTexture src)
        {
            var tmp = RenderTexture.active;
            RenderTexture.active = src;
            dst.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            dst.Apply();
            RenderTexture.active = tmp;
        }
    }
}