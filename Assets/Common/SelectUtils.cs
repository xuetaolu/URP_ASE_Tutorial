// @author : xue
// @created : 2024,07,19,14:07
// @desc:
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Common
{
    public class SelectUtils
    {
        // 获取 当前选中 的文件列表(fixed), 文件名小写, 包含目录, 忽略 /.* 和 *.meta
        //  . exts = 扩展名列表, 例如: ".prefab", ".png"
        public static HashSet<string> GetSelectedFiles(params string[] exts)
        {
            var hash = new HashSet<string>();
            void AddFile(string fname)
            {
                if (exts != null && exts.Length > 0)
                {
                    var ext = Path.GetExtension(fname);
                    if (!exts.Contains(ext)) return;
                }
                hash.Add(fname);
            }
            var objs = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);
            foreach (var obj in objs)
            {
                GetFiles(obj, AddFile);
            }
            return hash;
        }
        
        // 获取 obj 对应的 文件列表(fixed), 回调 callback
        public static void GetFiles(Object obj, Action<string> callback)
        {
            if (obj == null) return;                              // 非对象
            if (!EditorUtility.IsPersistent(obj)) return;         // 非磁盘文件
            if (!AssetDatabase.IsMainAsset(obj)) return;          // 非主对象
            var pathname = AssetDatabase.GetAssetPath(obj);
            // 如果是目录, 递归获取文件列表
            if (obj is DefaultAsset && Directory.Exists(pathname))
            {
                var files = GetFiles(pathname, "*.*", SearchOption.AllDirectories);
                foreach (var fname in files)
                {
                    callback(fname);
                }
            }
            // 否则, 添加单个对象
            else
            {
                pathname = PathUtils.FixPathname(pathname);
                callback(pathname);
            }
        }
        
        // 获取 path 目录下的 所有文件名(fixed) 列表
        //  . 文件名 全部小写, 以 / 分割路径
        public static List<string> GetFiles(string path, string pattern, SearchOption opt,
            bool fix_pathname = true)
        {
            return PathUtils.GetFiles(path, pattern, opt, fix_pathname);
        }
    }
}
#endif