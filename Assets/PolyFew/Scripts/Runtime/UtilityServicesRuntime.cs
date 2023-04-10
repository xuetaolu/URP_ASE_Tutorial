//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////



using BrainFailProductions.PolyFew.AsImpL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BrainFailProductions.PolyFewRuntime.PolyfewRuntime;

namespace BrainFailProductions.PolyFewRuntime
{


    public class UtilityServicesRuntime:MonoBehaviour
    {


        public static Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary
            (
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear
            );

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return readableText;
        }


        public static Renderer[] GetChildRenderersForCombining(GameObject forObject, bool skipInactiveChildObjects)
        {
            var resultRenderers = new List<Renderer>();


            if (skipInactiveChildObjects && !forObject.gameObject.activeSelf)
            {
                Debug.LogWarning($"No Renderers under the GameObject \"{forObject.name}\" combined because the object was inactive and was skipped entirely.");
                return null;
            }

            if (forObject.GetComponent<LODGroup>() != null)
            {
                Debug.LogWarning($"No Renderers under the GameObject \"{forObject.name}\" combined because the object had LOD groups and was skipped entirely.");
                return null;
            }

            CollectChildRenderersForCombining(forObject.transform, resultRenderers, skipInactiveChildObjects);
            return resultRenderers.ToArray();
        }


        public static MeshRenderer CreateStaticLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials)
        {
            var combinedMeshObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            var levelTransform = combinedMeshObject.transform;

            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var meshFilter = combinedMeshObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = combinedMeshObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            //SetupLevelRenderer(meshRenderer, ref level);
            return meshRenderer;
        }


        public static SkinnedMeshRenderer CreateSkinnedLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials, Transform rootBone, Transform[] bones)
        {
            var levelGameObject = new GameObject(name, typeof(SkinnedMeshRenderer));
            var levelTransform = levelGameObject.transform;

            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var skinnedMeshRenderer = levelGameObject.GetComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMesh = mesh;
            skinnedMeshRenderer.sharedMaterials = materials;
            skinnedMeshRenderer.rootBone = rootBone;
            skinnedMeshRenderer.bones = bones;

            return skinnedMeshRenderer;
        }


        private static void CollectChildRenderersForCombining(Transform transform, List<Renderer> resultRenderers, bool skipInactiveChildObjects)
        {

            var childRenderers = transform.GetComponents<Renderer>();

            resultRenderers.AddRange(childRenderers);

            int childCount = transform.childCount;

            for (int a = 0; a < childCount; a++)
            {

                // Skip children that are not active
                var childTransform = transform.GetChild(a);

                if (skipInactiveChildObjects && !childTransform.gameObject.activeSelf)
                {
                    Debug.LogWarning($"No Renderers under the GameObject \"{transform.name}\" combined because the object was inactive and was skipped entirely.");
                    continue;
                }

                // Skip children that has a LOD Group
                if (childTransform.GetComponent<LODGroup>() != null)
                {
                    Debug.LogWarning($"No Renderers under the GameObject \"{transform.name}\" combined because the object had LOD groups and was skipped entirely.");
                    continue;
                }

                // Continue recursively through the children of this transform
                CollectChildRenderersForCombining(childTransform, resultRenderers, skipInactiveChildObjects);
            }
        }


        private static void ParentAndResetTransform(Transform transform, Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }


        public static void ParentAndOffsetTransform(Transform transform, Transform parentTransform, Transform originalTransform)
        {
            transform.position = originalTransform.position;
            transform.rotation = originalTransform.rotation;
            transform.localScale = originalTransform.lossyScale;
            transform.SetParent(parentTransform, true);
        }



        #region OBJ_EXPORT_IMPORT


        /*
         ======================================================================================
         |	    Special thanks to aaro4130 for the Unity3D Scene OBJ Exporter
         |      This section would not have been made possible or would have been partial 
         |      without his works.
         |
         |      Do check out: 
         |      https://assetstore.unity.com/packages/tools/utilities/scene-obj-exporter-22250
         |  
         ======================================================================================
        */


        public class OBJExporterImporter
        {

            #region OBJ_EXPORT

            private bool applyPosition = true;
            private bool applyRotation = true;
            private bool applyScale = true;
            private bool generateMaterials = true;
            private bool exportTextures = true;
            private string exportPath;
            private MeshFilter meshFilter;

            private Mesh meshToExport;
            private MeshRenderer meshRenderer;



            public OBJExporterImporter() { }



            private void InitializeExporter(GameObject toExport, string exportPath, PolyfewRuntime.OBJExportOptions exportOptions)
            {
                this.exportPath = exportPath;


                if (string.IsNullOrWhiteSpace(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }

                else
                {
                    exportPath = Path.GetFullPath(exportPath);
                    if (exportPath[exportPath.Length - 1] == '\\') { exportPath = exportPath.Remove(exportPath.Length - 1); }
                    else if (exportPath[exportPath.Length - 1] == '/') { exportPath = exportPath.Remove(exportPath.Length - 1); }
                }

                if (!System.IO.Directory.Exists(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }

                if (toExport == null)
                {
                    throw new ArgumentNullException("toExport", "Please provide a GameObject to export as OBJ file.");
                }


                meshRenderer = toExport.GetComponent<MeshRenderer>();
                meshFilter = toExport.GetComponent<MeshFilter>();

                if (meshRenderer == null)
                {

                }

                else
                {
                    if (meshRenderer.isPartOfStaticBatch)
                    {
                        throw new InvalidOperationException("The provided object is static batched. Static batched object cannot be exported. Please disable it before trying to export the object.");
                    }
                }

                if (meshFilter == null)
                {
                    throw new InvalidOperationException("There is no MeshFilter attached to the provided GameObject.");
                }

                else
                {
                    meshToExport = meshFilter.sharedMesh;

                    if (meshToExport == null || meshToExport.triangles == null || meshToExport.triangles.Length == 0)
                    {
                        throw new InvalidOperationException("The MeshFilter on the provided GameObject has invalid or no mesh at all.");
                    }
                }


                if (exportOptions != null)
                {
                    applyPosition = exportOptions.applyPosition;
                    applyRotation = exportOptions.applyRotation;
                    applyScale = exportOptions.applyScale;
                    generateMaterials = exportOptions.generateMaterials;
                    exportTextures = exportOptions.exportTextures;
                }

            }


            private void InitializeExporter(Mesh toExport, string exportPath)
            {
                this.exportPath = exportPath;

                if (string.IsNullOrWhiteSpace(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }


                if (!System.IO.Directory.Exists(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }


                if (toExport == null)
                {
                    throw new ArgumentNullException("toExport", "Please provide a Mesh to export as OBJ file.");
                }


                meshToExport = toExport;


                if (meshToExport == null || meshToExport.triangles == null || meshToExport.triangles.Length == 0)
                {
                    throw new InvalidOperationException("The MeshFilter on the provided GameObject has invalid or no mesh at all.");
                }

            }



            Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
            {
                return angle * (point - pivot) + pivot;
            }

            Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
            {
                return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
            }



            public void ExportGameObjectToOBJ(GameObject toExport, string exportPath, PolyfewRuntime.OBJExportOptions exportOptions = null, Action OnSuccess = null)
            {


                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    Debug.LogWarning("The function cannot run on WebGL player. As web apps cannot read from or write to local file system.");
                    return;
                }

                //init stuff
                Dictionary<string, bool> materialCache = new Dictionary<string, bool>();

                //Debug.Log("Exporting OBJ. Please wait.. Starting to export.");


                InitializeExporter(toExport, exportPath, exportOptions);


                //get list of required export things


                string objectName = toExport.gameObject.name;


                //work on export
                StringBuilder sb = new StringBuilder();
                StringBuilder sbMaterials = new StringBuilder();


                if (generateMaterials)
                {
                    sb.AppendLine("mtllib " + objectName + ".mtl");
                }

                int lastIndex = 0;


                if (meshRenderer != null && generateMaterials)
                {
                    Material[] mats = meshRenderer.sharedMaterials;
                    for (int j = 0; j < mats.Length; j++)
                    {
                        Material m = mats[j];
                        if (!materialCache.ContainsKey(m.name))
                        {
                            materialCache[m.name] = true;
                            sbMaterials.Append(MaterialToString(m));
                            sbMaterials.AppendLine();
                        }
                    }
                }

                //export the meshhh :3

                int faceOrder = (int)Mathf.Clamp((toExport.gameObject.transform.lossyScale.x * toExport.gameObject.transform.lossyScale.z), -1, 1);

                //export vector data (FUN :D)!
                foreach (Vector3 vx in meshToExport.vertices)
                {

                    Vector3 v = vx;
                    if (applyScale)
                    {
                        v = MultiplyVec3s(v, toExport.gameObject.transform.lossyScale);
                    }

                    if (applyRotation)
                    {
                        v = RotateAroundPoint(v, Vector3.zero, toExport.gameObject.transform.rotation);
                    }

                    if (applyPosition)
                    {
                        v += toExport.gameObject.transform.position;
                    }

                    v.x *= -1;
                    sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector3 vx in meshToExport.normals)
                {

                    Vector3 v = vx;

                    if (applyScale)
                    {
                        v = MultiplyVec3s(v, toExport.gameObject.transform.lossyScale.normalized);
                    }
                    if (applyRotation)
                    {
                        v = RotateAroundPoint(v, Vector3.zero, toExport.gameObject.transform.rotation);
                    }

                    v.x *= -1;
                    sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector2 v in meshToExport.uv)
                {
                    sb.AppendLine("vt " + v.x + " " + v.y);
                }

                for (int j = 0; j < meshToExport.subMeshCount; j++)
                {
                    if (meshRenderer != null && j < meshRenderer.sharedMaterials.Length)
                    {
                        string matName = meshRenderer.sharedMaterials[j].name;
                        sb.AppendLine("usemtl " + matName);
                    }
                    else
                    {
                        sb.AppendLine("usemtl " + objectName + "_sm" + j);
                    }

                    int[] tris = meshToExport.GetTriangles(j);

                    for (int t = 0; t < tris.Length; t += 3)
                    {
                        int idx2 = tris[t] + 1 + lastIndex;
                        int idx1 = tris[t + 1] + 1 + lastIndex;
                        int idx0 = tris[t + 2] + 1 + lastIndex;

                        if (faceOrder < 0)
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                        }
                        else
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                        }

                    }
                }

                lastIndex += meshToExport.vertices.Length;


                //write to disk

                string writePath = System.IO.Path.Combine(exportPath, objectName + ".obj");

                System.IO.File.WriteAllText(writePath, sb.ToString());

                if (generateMaterials)
                {
                    writePath = System.IO.Path.Combine(exportPath, objectName + ".mtl");
                    System.IO.File.WriteAllText(writePath, sbMaterials.ToString());
                }

                //export complete, close progress dialog
                OnSuccess?.Invoke();
            }



            public async Task ExportMeshToOBJ(Mesh mesh, string exportPath)
            {

                InitializeExporter(mesh, exportPath);

                string objectName = meshToExport.name;
                StringBuilder sb = new StringBuilder();
                int lastIndex = 0;
                int faceOrder = 1;

                //export vector data (FUN :D)!
                foreach (Vector3 vx in meshToExport.vertices)
                {
                    await Task.Delay(1);

                    Vector3 v = vx;

                    v.x *= -1;

                    sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector3 vx in meshToExport.normals)
                {
                    await Task.Delay(1);

                    Vector3 v = vx;

                    v.x *= -1;
                    sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector2 v in meshToExport.uv)
                {
                    await Task.Delay(1);

                    sb.AppendLine("vt " + v.x + " " + v.y);
                }

                for (int j = 0; j < meshToExport.subMeshCount; j++)
                {
                    await Task.Delay(1);

                    sb.AppendLine("usemtl " + objectName + "_sm" + j);

                    int[] tris = meshToExport.GetTriangles(j);

                    for (int t = 0; t < tris.Length; t += 3)
                    {
                        await Task.Delay(1);

                        int idx2 = tris[t] + 1 + lastIndex;
                        int idx1 = tris[t + 1] + 1 + lastIndex;
                        int idx0 = tris[t + 2] + 1 + lastIndex;

                        if (faceOrder < 0)
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                        }
                        else
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                        }

                    }
                }

                lastIndex += meshToExport.vertices.Length;


                //write to disk

                string writePath = System.IO.Path.Combine(exportPath, objectName + ".obj");

                System.IO.File.WriteAllText(writePath, sb.ToString());

            }



            string TryExportTexture(string propertyName, Material m, string exportPath)
            {
                if (m.HasProperty(propertyName))
                {
                    Texture t = m.GetTexture(propertyName);

                    if (t != null)
                    {
                        return ExportTexture((Texture2D)t, exportPath);
                    }
                }

                return "false";
            }


            string ExportTexture(Texture2D t, string exportPath)
            {
                //Debug.Log($"Exporting texture:  {t.name} to path: {exportPath}");

                string textureName = t.name;

                try
                {
                    Color32[] pixels32 = null;

                    try
                    {
                        pixels32 = t.GetPixels32();
                    }
#pragma warning disable
                    catch (UnityException ex)
                    {
                        t = UtilityServicesRuntime.DuplicateTexture(t);
                        pixels32 = t.GetPixels32();
                    }

                    string qualifiedPath = System.IO.Path.Combine(exportPath, textureName + ".png");
                    Texture2D exTexture = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false);
                    exTexture.SetPixels32(pixels32);

                    System.IO.File.WriteAllBytes(qualifiedPath, exTexture.EncodeToPNG());

                    return qualifiedPath;
                }

                catch (System.Exception ex)
                {
                    Debug.Log("Could not export texture : " + t.name + ". is it readable?");
                    return "null";
                }

            }


            private string ConstructOBJString(int index)
            {
                string idxString = index.ToString();
                return idxString + "/" + idxString + "/" + idxString;
            }


            string MaterialToString(Material m)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("newmtl " + m.name);


                //add properties
                if (m.HasProperty("_Color"))
                {
                    sb.AppendLine("Kd " + m.color.r.ToString() + " " + m.color.g.ToString() + " " + m.color.b.ToString());
                    if (m.color.a < 1.0f)
                    {
                        //use both implementations of OBJ transparency
                        sb.AppendLine("Tr " + (1f - m.color.a).ToString());
                        sb.AppendLine("d " + m.color.a.ToString());
                    }
                }
                if (m.HasProperty("_SpecColor"))
                {
                    Color sc = m.GetColor("_SpecColor");
                    sb.AppendLine("Ks " + sc.r.ToString() + " " + sc.g.ToString() + " " + sc.b.ToString());
                }
                if (exportTextures)
                {
                    //diffuse
                    string exResult = TryExportTexture("_MainTex", m, exportPath);
                    if (exResult != "false")
                    {
                        sb.AppendLine("map_Kd " + exResult);
                    }
                    //spec map
                    exResult = TryExportTexture("_SpecMap", m, exportPath);
                    if (exResult != "false")
                    {
                        sb.AppendLine("map_Ks " + exResult);
                    }
                    //bump map
                    exResult = TryExportTexture("_BumpMap", m, exportPath);
                    if (exResult != "false")
                    {
                        sb.AppendLine("map_Bump " + exResult);
                    }

                }
                sb.AppendLine("illum 2");
                return sb.ToString();
            }


            #endregion OBJ_EXPORT


            #region OBJ_IMPORT




            public async Task ImportFromLocalFileSystem(string objPath, string texturesFolderPath, string materialsFolderPath, Action<GameObject> Callback, OBJImportOptions importOptions = null)
            {

                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    Debug.LogWarning("The function cannot run on WebGL player. As web apps cannot read from or write to local file system.");
                    return;
                }

                if (!String.IsNullOrWhiteSpace(objPath))
                {
                    objPath = Path.GetFullPath(objPath);
                    if (objPath[objPath.Length - 1] == '\\') { objPath = objPath.Remove(objPath.Length - 1); }
                    else if (objPath[objPath.Length - 1] == '/') { objPath = objPath.Remove(objPath.Length - 1); }
                }

                if (!String.IsNullOrWhiteSpace(texturesFolderPath))
                {
                    texturesFolderPath = Path.GetFullPath(texturesFolderPath);
                    if (texturesFolderPath[texturesFolderPath.Length - 1] == '\\') { texturesFolderPath = texturesFolderPath.Remove(texturesFolderPath.Length - 1); }
                    else if (texturesFolderPath[texturesFolderPath.Length - 1] == '/') { texturesFolderPath = texturesFolderPath.Remove(texturesFolderPath.Length - 1); }
                }

                if (!String.IsNullOrWhiteSpace(materialsFolderPath))
                {
                    materialsFolderPath = Path.GetFullPath(materialsFolderPath);
                    if (materialsFolderPath[materialsFolderPath.Length - 1] == '\\') { materialsFolderPath = materialsFolderPath.Remove(materialsFolderPath.Length - 1); }
                    else if (materialsFolderPath[materialsFolderPath.Length - 1] == '/') { materialsFolderPath = materialsFolderPath.Remove(materialsFolderPath.Length - 1); }
                }


                if (!System.IO.File.Exists(objPath))
                {
                    throw new FileNotFoundException("The path provided doesn't point to a file. The path might be invalid or the file is non-existant.");
                }

                if (!string.IsNullOrWhiteSpace(texturesFolderPath) && !System.IO.Directory.Exists(texturesFolderPath))
                {
                    Debug.LogWarning("The directory pointed to by the given path for textures is non-existant.");
                }

                if (!string.IsNullOrWhiteSpace(materialsFolderPath) && !System.IO.Directory.Exists(materialsFolderPath))
                {
                    Debug.LogWarning("The directory pointed to by the given path for materials is non-existant.");
                }


                string fileNameWithExt = System.IO.Path.GetFileName(objPath);
                string dirPath = System.IO.Path.GetDirectoryName(objPath);
                string objName = fileNameWithExt.Split('.')[0];
#pragma warning disable
                bool didFail = false;

                GameObject objectToPopulate = new GameObject();
                objectToPopulate.AddComponent<ObjectImporter>();
                ObjectImporter objImporter = objectToPopulate.GetComponent<ObjectImporter>();

                if (dirPath.Contains("/") && !dirPath.EndsWith("/")) { dirPath += "/"; }
                else if (!dirPath.EndsWith("\\")) { dirPath += "\\"; }

                var split = fileNameWithExt.Split('.');


                if (split[1].ToLower() != "obj")
                {
                    DestroyImmediate(objectToPopulate);
                    throw new System.InvalidOperationException("The path provided must point to a wavefront obj file.");
                }


                if (importOptions == null)
                {
                    importOptions = new OBJImportOptions();
                }


                try
                {
                    GameObject toReturn = await objImporter.ImportModelAsync(objName, objPath, null, importOptions, texturesFolderPath, materialsFolderPath);
                    Destroy(objImporter);
                    Callback(toReturn);
                }

                catch (Exception ex)
                {
                    DestroyImmediate(objectToPopulate);
                    throw ex;
                }

            }


            public async void ImportFromNetwork(string objURL, string objName, string diffuseTexURL, string bumpTexURL, string specularTexURL, string opacityTexURL, string materialURL, ReferencedNumeric<float> downloadProgress, Action<GameObject> OnSuccess, Action<Exception> OnError, OBJImportOptions importOptions = null)
            {

                if (String.IsNullOrWhiteSpace(objURL))
                {
                    throw new InvalidOperationException("Cannot download from empty URL. Please provide a direct URL to the obj file");
                }

                if (String.IsNullOrWhiteSpace(diffuseTexURL))
                {
                    Debug.LogWarning("Cannot download from empty URL. Please provide a direct URL to the accompanying texture file.");
                }

                if (String.IsNullOrWhiteSpace(materialURL))
                {
                    Debug.LogWarning("Cannot download from empty URL. Please provide a direct URL to the accompanying material file.");
                }

                if(downloadProgress == null)
                {
                    throw new ArgumentNullException("downloadProgress", "You must pass a reference to the Download Progress object.");
                }

                GameObject objectToPopulate = new GameObject();
                objectToPopulate.AddComponent<ObjectImporter>();
                ObjectImporter objImporter = objectToPopulate.GetComponent<ObjectImporter>();


                if (importOptions == null)
                {
                    importOptions = new OBJImportOptions();
                }


                try
                {                    
                    GameObject toReturn = await objImporter.ImportModelFromNetwork(objURL, objName, diffuseTexURL, bumpTexURL, specularTexURL, opacityTexURL, materialURL, downloadProgress, importOptions);
                    Destroy(objImporter);
                    OnSuccess(toReturn);
                }

                catch (Exception ex)
                {
                    DestroyImmediate(objectToPopulate);
                    OnError(ex);
                }

            }



            public async void ImportFromNetworkWebGL(string objURL, string objName, string diffuseTexURL, string bumpTexURL, string specularTexURL, string opacityTexURL, string materialURL, ReferencedNumeric<float> downloadProgress, Action<GameObject> OnSuccess, Action<Exception> OnError, OBJImportOptions importOptions = null)
            {

                if (String.IsNullOrWhiteSpace(objURL))
                {
                    OnError(new InvalidOperationException("Cannot download from empty URL. Please provide a direct URL to the obj file"));
                    return;
                }

                if (String.IsNullOrWhiteSpace(diffuseTexURL))
                {
                    Debug.LogWarning("Cannot download from empty URL. Please provide a direct URL to the accompanying texture file.");
                }

                if (String.IsNullOrWhiteSpace(materialURL))
                {
                    Debug.LogWarning("Cannot download from empty URL. Please provide a direct URL to the accompanying material file.");
                }

                if (downloadProgress == null)
                {
                    OnError(new ArgumentNullException("downloadProgress", "You must pass a reference to the Download Progress object."));
                    return;
                }
                
                GameObject objectToPopulate = new GameObject();
                objectToPopulate.AddComponent<ObjectImporter>();
                ObjectImporter objImporter = objectToPopulate.GetComponent<ObjectImporter>();


                if (importOptions == null)
                {
                    importOptions = new OBJImportOptions();
                }


               
                objImporter.ImportModelFromNetworkWebGL(objURL, objName, diffuseTexURL, bumpTexURL, specularTexURL, opacityTexURL, materialURL, downloadProgress, importOptions, (GameObject imported) =>
                {
                    Destroy(objImporter);
                    OnSuccess(imported);
                }, 
                (exception) =>
                {
                    DestroyImmediate(objectToPopulate);
                    OnError(exception);
                });

            }


            #endregion OBJ_IMPORT

        }



        #endregion OBJ_EXPORT_IMPORT

    }


}
