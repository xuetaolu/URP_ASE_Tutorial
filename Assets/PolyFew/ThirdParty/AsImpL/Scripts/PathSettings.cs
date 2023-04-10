using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace BrainFailProductions.PolyFew.AsImpL
{
    public enum RootPathEnum
    {
        Url,
        DataPath,
        DataPathParent,
        PersistentDataPath,
        CurrentPath
    }

    public class PathSettings : MonoBehaviour
    {
#if UNITY_EDITOR

        [Tooltip("Root path for models in Unity Editor only")]
        public RootPathEnum editorRootPath = RootPathEnum.Url;
#endif

        [Tooltip("Default root path for models")]
        public RootPathEnum defaultRootPath = RootPathEnum.Url;

        [Tooltip("Root path for models on mobile devices")]
        public RootPathEnum mobileRootPath = RootPathEnum.Url;


        public string RootPath
        {
            get
            {
#if UNITY_EDITOR
                switch (editorRootPath)
#elif UNITY_STANDALONE
                switch (defaultRootPath)
#else
                switch (mobileRootPath)
#endif
                {
                    case RootPathEnum.DataPath:
                        return Application.dataPath + "/";
                    case RootPathEnum.DataPathParent:
                        return Application.dataPath + "/../";
                    case RootPathEnum.PersistentDataPath:
                        return Application.persistentDataPath + "/";
                }
                return "";
            }
        }


        public static PathSettings FindPathComponent(GameObject obj)
        {
            PathSettings pathSettings = obj.GetComponent<PathSettings>();
            if (pathSettings == null)
            {
                pathSettings = FindObjectOfType<PathSettings>();
            }
            if (pathSettings == null)
            {
                pathSettings = obj.AddComponent<PathSettings>();
            }
            return pathSettings;
        }


        public string FullPath(string path)
        {
            string fullPath = path;
            if(!Path.IsPathRooted(path))
            {
                fullPath = RootPath + path;
            }
            return fullPath;
        }
    
    }
}
