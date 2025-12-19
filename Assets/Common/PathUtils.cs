// @author : xue
// @created : 2024,07,19,14:07
// @desc:
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;

namespace Common
{
    public class PathUtils
    {
        // 变成小写, 并转化斜杠
        public static string FixPathname(string fname)
        {
            return fname.ToLower().Replace("\\", "/");
        }
        
        // 获取 path 目录下的所有文件名列表; 并过滤掉非法文件
        public static List<string> GetFiles(string path, string pattern,
            SearchOption opt, bool fix_pathname = true)
        {
            var list = new List<string>();
            var files = Directory.GetFiles(path, pattern, opt);
            foreach (var fname in files)
            {
                if (fname.EndsWith(".meta")) continue;              // 忽略 .meta
                if (fix_pathname)
                {
                    var fname2 = FixPathname(fname);
                    if (fname2.Contains("/.")) continue;                // 去掉 . 特殊目录
                    list.Add(fname2);
                }
                else
                {
                    if (fname.Contains("./") || fname.Contains("\\.")) continue;
                    list.Add(fname);
                }
            }
            return list;
        }
    }
}
#endif