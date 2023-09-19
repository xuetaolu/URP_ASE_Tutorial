// @author : xue
// @created : 2023,09,19,15:53
// @desc:

using UnityEngine;
using System.Linq;

namespace Common
{
    public class RenderTextureUtils
    {
        // 普通格式
        static readonly RenderTextureFormat[] s_fmts = new RenderTextureFormat[]{
        RenderTextureFormat.ARGB4444,
        RenderTextureFormat.ARGB32,
    };

        // hdr 格式, 需要 float 才能保存超出的颜色值
        static readonly RenderTextureFormat[] s_fmts_hdr = new RenderTextureFormat[]{
        RenderTextureFormat.ARGBHalf,
        RenderTextureFormat.ARGBFloat,
    };
        public static RenderTextureFormat GetSupportedFormat(bool hdr, bool reverse = false)
        {
            //if (hdr) return RenderTextureFormat.Default;    // 模拟部分手机不支持
            var arr = hdr ? s_fmts_hdr : s_fmts;
            
            // #bug13884 【合并1.26】【周更】【6-27】【sgp】新增资源-海洋乐园环绕特效在背包界面有光晕显示
            // 需要优先使用高精度 RT
            foreach (var fmt in reverse ? arr.Reverse() : arr)
            {
                if (SystemInfo.SupportsRenderTextureFormat(fmt))
                {
                    return fmt;
                }
            }
            return RenderTextureFormat.Default;
        }
    }
}