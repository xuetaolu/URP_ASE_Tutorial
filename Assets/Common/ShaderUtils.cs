// @author : xue
// @created : 2023,08,22,15:26
// @desc:

using UnityEngine;

namespace Common
{
    public static class ShaderUtils
    {
        public static void EnableKeyword(this Material mat, string name, bool value)
        {
            if (value) mat.EnableKeyword(name);
            else mat.DisableKeyword(name);
        }
    }
}