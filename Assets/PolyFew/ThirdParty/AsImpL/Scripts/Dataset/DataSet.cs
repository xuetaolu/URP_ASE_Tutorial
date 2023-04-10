using UnityEngine;
using System.Collections.Generic;

namespace BrainFailProductions.PolyFew.AsImpL
{
    /// <summary>
    /// Data set for storing data in a neutral format for a generic model
    /// </summary>
    /// <remarks>This should be completed and extended to support more formats.</remarks>
    /// TODO: Classes to hold data for internal hierarchy should be defined.
    public class DataSet
    {
        /// <summary>
        /// List of objects
        /// </summary>
        public List<ObjectData> objectList = new List<ObjectData>();

        /// <summary>
        /// List of vertices
        /// </summary>
        public List<Vector3> vertList = new List<Vector3>();

        /// <summary>
        /// List of texture coordinates (UV)
        /// </summary>
        public List<Vector2> uvList = new List<Vector2>();

        /// <summary>
        /// List of normals
        /// </summary>
        public List<Vector3> normalList = new List<Vector3>();

        /// <summary>
        /// List of colors
        /// </summary>
        public List<Color> colorList = new List<Color>();

        // naming index for unnamed group (e.g. "Unnamed-1")
        private int unnamedGroupIndex = 1;
        private ObjectData currObjData;

        private FaceGroupData currGroup;

        private bool noFaceDefined = true;


        /// <summary>
        /// Name of the current group.
        /// </summary>
        public string CurrGroupName
        {
            get
            {
                return currGroup != null ? currGroup.name : "";
            }
        }

        /// <summary>
        /// Check if there is no vertex defined.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return vertList.Count == 0;
            }
        }


        /// <summary>
        /// Get a string key based on the given face indices
        /// </summary>
        /// <param name="fi">face indices structure</param>
        /// <returns></returns>
        public static string GetFaceIndicesKey(FaceIndices fi)
        {
            return fi.vertIdx.ToString() + "/" + fi.uvIdx.ToString() + "/" + fi.normIdx.ToString();
        }


        /// <summary>
        /// Change the material name to be compliant with Unity material asset naming.
        /// </summary>
        /// <param name="mtlName">original material name</param>
        /// <returns>Returns the name modified to be compliant with Unity material asset naming.</returns>
        public static string FixMaterialName(string mtlName)
        {
            return mtlName
                .Replace(':', '_')
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace('*', '_')
                .Replace('?', '_')
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace('|', '_');
        }


        /// <summary>
        /// Constructor: create data lists and initialzize the default object.
        /// </summary>
        public DataSet()
        {
            ObjectData d = new ObjectData();
            d.name = "default";
            objectList.Add(d);
            currObjData = d;

            FaceGroupData g = new FaceGroupData();
            g.name = "default";
            d.faceGroups.Add(g);
            currGroup = g;
        }


        /// <summary>
        /// Add and initialize a new object.
        /// </summary>
        /// <param name="objectName">name of the new object</param>
        public void AddObject(string objectName)
        {
            //Debug.Log("Adding new object " + name + ". Current is empty: " + isEmpty);
            string currentMaterial = currObjData.faceGroups[currObjData.faceGroups.Count - 1].materialName;

            if (noFaceDefined) objectList.Remove(currObjData);

            ObjectData objData = new ObjectData();
            objData.name = objectName;
            objectList.Add(objData);

            FaceGroupData grp = new FaceGroupData();
            grp.materialName = currentMaterial;
            grp.name = "default";
            objData.faceGroups.Add(grp);

            currGroup = grp;
            currObjData = objData;
        }


        /// <summary>
        /// Add and initialize a new group and add it to the current object.
        /// </summary>
        /// <param name="groupName">name of the new group</param>
        public void AddGroup(string groupName)
        {
            string currentMaterial = currObjData.faceGroups[currObjData.faceGroups.Count - 1].materialName;

            if (currGroup.IsEmpty) currObjData.faceGroups.Remove(currGroup);
            FaceGroupData grp = new FaceGroupData();
            grp.materialName = currentMaterial;
            if (groupName == null)
            {
                groupName = "Unnamed-" + unnamedGroupIndex;
                unnamedGroupIndex++;
            }
            grp.name = groupName;
            currObjData.faceGroups.Add(grp);
            currGroup = grp;
        }


        /// <summary>
        /// Set a new material name to the current group (add a group if not yet added).
        /// </summary>
        /// <param name="matName">name of the new material</param>
        /// TODO: do not split by materials if there is only one meterial
        public void AddMaterialName(string matName)
        {
            if (!currGroup.IsEmpty) AddGroup(matName);
            if (currGroup.name == "default") currGroup.name = matName;
            currGroup.materialName = matName;
        }


        /// <summary>
        /// Add a vertex to the global vertex list
        /// </summary>
        /// <param name="vertex">new vertex</param>
        public void AddVertex(Vector3 vertex)
        {
            vertList.Add(vertex);
        }


        /// <summary>
        /// Add a texture coordinate (U,V) to the global list
        /// </summary>
        /// <param name="uv">texture coordinate (U,V)</param>
        public void AddUV(Vector2 uv)
        {
            uvList.Add(uv);
        }


        /// <summary>
        /// Add a new normal to the global list
        /// </summary>
        /// <param name="normal">normal</param>
        public void AddNormal(Vector3 normal)
        {
            normalList.Add(normal);
        }


        /// <summary>
        /// Add a new color to the global list
        /// </summary>
        /// <param name="color">color</param>
        public void AddColor(Color color)
        {
            colorList.Add(color);
            currObjData.hasColors = true;
        }


        /// <summary>
        /// Add a new face indices entry to the current faces group
        /// </summary>
        /// <param name="faceIdx">new vertex indices</param>
        public void AddFaceIndices(FaceIndices faceIdx)
        {
            noFaceDefined = false;
            currGroup.faces.Add(faceIdx);
            currObjData.allFaces.Add(faceIdx);
            if (faceIdx.normIdx >= 0)
            {
                currObjData.hasNormals = true;
            }
        }


        /// <summary>
        /// Print a summary of the stored data
        /// </summary>
        public void PrintSummary()
        {
            string stats = "This data set has " + objectList.Count + " object(s)" +
                "\n  " + vertList.Count + " vertices" +
                "\n  " + uvList.Count + " uvs" +
                "\n  " + normalList.Count + " normals";
            foreach (ObjectData od in objectList)
            {
                stats += "\n  " + od.name + " has " + od.faceGroups.Count + " group(s)";
                foreach (FaceGroupData gd in od.faceGroups)
                {
                    stats += "\n    " + gd.name + " has " + gd.faces.Count + " faces(s)";
                }
            }
            Debug.Log(stats);
        }


        /// <summary>
        /// Indices for each vertex
        /// </summary>
        public struct FaceIndices
        {
            public int vertIdx;
            public int uvIdx;
            public int normIdx;
        }


        /// <summary>
        /// Container class for object data.
        /// </summary>
        public class ObjectData
        {
            public string name;
            public List<FaceGroupData> faceGroups = new List<FaceGroupData>();
            public List<FaceIndices> allFaces = new List<FaceIndices>();
            public bool hasNormals = false;
            public bool hasColors = false;
            public ObjectData()
            {
            }
        }


        /// <summary>
        /// Container class for faces group data.
        /// </summary>
        public class FaceGroupData
        {
            public string name;
            public string materialName;
            public List<FaceIndices> faces;
            public FaceGroupData()
            {
                faces = new List<FaceIndices>();
            }
            public bool IsEmpty { get { return faces.Count == 0; } }
        }

    }
}
