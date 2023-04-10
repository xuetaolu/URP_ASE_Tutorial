using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using static BrainFailProductions.PolyFewRuntime.PolyfewRuntime;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Asynchronous importer and loader
/// </summary>
namespace BrainFailProductions.PolyFew.AsImpL
{

    /// <summary>
    /// Abstract loader to be used as a base class for specific loaders.
    /// </summary>
    public abstract class Loader : MonoBehaviour
    {
        /// <summary>
        /// Total loading progress, for all the models currently loading.
        /// </summary>
        public static LoadingProgress totalProgress = new LoadingProgress();

        /// <summary>
        /// Options to define how the model will be loaded and imported.
        /// </summary>
        public ImportOptions buildOptions;


        public ReferencedNumeric<float> individualProgress = new ReferencedNumeric<float>(0);


#if UNITY_EDITOR
        /// <summary>
        /// Alternative texture path: if not null textures will be loaded from here.
        /// </summary>
        public string altTexPath = null;
#endif

        // raw subdivision in percentages of the loading phases (empirically computed loading a large sample OBJ file)
        // TODO: refine or change this method
        protected static float LOAD_PHASE_PERC = 8f;
        protected static float TEXTURE_PHASE_PERC = 1f;
        protected static float MATERIAL_PHASE_PERC = 1f;
        protected static float BUILD_PHASE_PERC = 90f;

        protected static Dictionary<string, GameObject> loadedModels = new Dictionary<string, GameObject>();
        protected static Dictionary<string, int> instanceCount = new Dictionary<string, int>();

        protected DataSet dataSet = new DataSet();
        protected ObjectBuilder objectBuilder = new ObjectBuilder();

        protected List<MaterialData> materialData;

        protected SingleLoadingProgress objLoadingProgress = new SingleLoadingProgress();

        protected Stats loadStats;

        private Texture2D loadedTexture = null;

        /// <summary>
        /// Load the file assuming its vertical axis is Z instead of Y 
        /// </summary>
        public bool ConvertVertAxis
        {
            get
            {
                return buildOptions != null ? buildOptions.zUp : false;
            }
            set
            {
                if (buildOptions == null)
                {
                    buildOptions = new ImportOptions();
                }
                buildOptions.zUp = value;
            }
        }


        /// <summary>
        /// Rescaling for the model (1 = no rescaling)
        /// </summary>
        public float Scaling
        {
            get
            {
                return buildOptions != null ? buildOptions.modelScaling : 1f;
            }
            set
            {
                if (buildOptions == null)
                {
                    buildOptions = new ImportOptions();
                }
                buildOptions.modelScaling = value;
            }
        }


        /// <summary>
        /// Check if a material library is defined for this model
        /// </summary>
        protected abstract bool HasMaterialLibrary { get; }

#if UNITY_EDITOR
        /// <summary>
        /// Import data as assets in the project (Editor only)
        /// </summary>
        public bool ImportingAssets { get { return !string.IsNullOrEmpty(altTexPath); } }
#endif

        /// <summary>
        /// Event triggered when an object is created.
        /// </summary>
        public event Action<GameObject, string> ModelCreated;

        /// <summary>
        /// Event triggered when an object is successfully loaded.
        /// </summary>
        public event Action<GameObject, string> ModelLoaded;

        /// <summary>
        /// Event triggered if failed to load an object
        /// </summary>
        public event Action<string> ModelError;


        /// <summary>
        /// Get a previusly loaded model by its absolute path
        /// </summary>
        /// <param name="absolutePath">absolute path used to load the model</param>
        /// <returns>The game object previously loaded</returns>
        public static GameObject GetModelByPath(string absolutePath)
        {
            if (loadedModels.ContainsKey(absolutePath))
            {
                return loadedModels[absolutePath];
            }
            return null;
        }


        /// <summary>
        /// Load a model.
        /// </summary>
        /// <param name="objName">name of the GameObject, if empty use file name</param>
        /// <param name="objAbsolutePath">absolute file path</param>
        /// <param name="parentObj">Transform to which attach the loaded object (null=scene)</param>
        /// <returns>You can use StartCoroutine( loader.Load(...) )</returns>
        public async Task<GameObject> Load(string objName, string objAbsolutePath, Transform parentObj, string texturesFolderPath = "", string materialsFolderPath = "")
        {
            string fileName = Path.GetFileName(objAbsolutePath);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(objAbsolutePath);
            string name = objName;
            if (name == null || name == "") objName = fileNameNoExt;

            totalProgress.singleProgress.Add(objLoadingProgress);
            objLoadingProgress.fileName = fileName;
            objLoadingProgress.error = false;
            objLoadingProgress.message = "Loading " + fileName + "...";
            //Debug.LogFormat("Loading {0}\n  from: {1}...", objName, absolutePath);

            await Task.Yield();
            //yield return null;

            // TODO: implementation of a caching mechanism for models downloaded from an URL

            /*
            // if the model was already loaded duplicate the existing object
            if (buildOptions != null && buildOptions.reuseLoaded && loadedModels.ContainsKey(absolutePath) && loadedModels[absolutePath] != null)
            {
                Debug.LogFormat("File {0} already loaded, creating instance.", absolutePath);
                instanceCount[absolutePath]++;
                if (name == null || name == "") objName = objName + "_" + instanceCount[absolutePath];
                objLoadingProgress.message = "Instantiating " + objName + "...";
                while (loadedModels[absolutePath] == null)
                {
                    await Task.Yield();
                    //yield return null;
                }


                GameObject newObj = Instantiate(loadedModels[absolutePath]);
                //yield return newObj;
                OnCreated(newObj, absolutePath);
                newObj.name = objName;

                if (parentObj != null) newObj.transform.parent = parentObj.transform;
                totalProgress.singleProgress.Remove(objLoadingProgress);
                OnLoaded(newObj, absolutePath);

                return;

                //yield break;
            }
            */

            loadedModels[objAbsolutePath] = null; // define a key for the dictionary
            instanceCount[objAbsolutePath] = 0; // define a key for the dictionary

            float lastTime = Time.realtimeSinceStartup;
            float startTime = lastTime;

            await LoadModelFile(objAbsolutePath, texturesFolderPath, materialsFolderPath);

            loadStats.modelParseTime = Time.realtimeSinceStartup - lastTime;

            if (objLoadingProgress.error)
            {
                OnLoadFailed(objAbsolutePath);
                return null;
            }

            lastTime = Time.realtimeSinceStartup;

            if (HasMaterialLibrary)
            {
                await LoadMaterialLibrary(objAbsolutePath, materialsFolderPath);
            }

            loadStats.materialsParseTime = Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;

            await Build(objAbsolutePath, objName, parentObj, texturesFolderPath);

            loadStats.buildTime = Time.realtimeSinceStartup - lastTime;
            loadStats.totalTime = Time.realtimeSinceStartup - startTime;
            /*
            Debug.Log("Done: " + objName
                + "\n  Loaded in " + loadStats.totalTime + " seconds"
                + "\n  Model data parsed in " + loadStats.modelParseTime + " seconds"
                + "\n  Material data parsed in " + loadStats.materialsParseTime + " seconds"
                + "\n  Game objects built in " + loadStats.buildTime + " seconds"
                + "\n    textures: " + loadStats.buildStats.texturesTime + " seconds"
                + "\n    materials: " + loadStats.buildStats.materialsTime + " seconds"
                + "\n    objects: " + loadStats.buildStats.objectsTime + " seconds"
                );
            */
            totalProgress.singleProgress.Remove(objLoadingProgress);
            OnLoaded(loadedModels[objAbsolutePath], objAbsolutePath);

            return loadedModels[objAbsolutePath];
        }


        

        public async Task<GameObject> LoadFromNetwork(string objURL, string diffuseTexURL, string bumpTexURL, string specularTexURL, string opacityTexURL, string materialURL, string objName)
        {
            string fileName = objName + ".obj";
#pragma warning disable
            string fileNameNoExt = objName;


            totalProgress.singleProgress.Add(objLoadingProgress);
            objLoadingProgress.fileName = fileName;
            objLoadingProgress.error = false;
            objLoadingProgress.message = "Loading " + fileName + "...";
            //Debug.LogFormat("Loading {0}\n  from: {1}...", objName, absolutePath);

            await Task.Yield();


            loadedModels[objURL] = null; // define a key for the dictionary
            instanceCount[objURL] = 0; // define a key for the dictionary

            float lastTime = Time.realtimeSinceStartup;
            float startTime = lastTime;

            // base model here
            try
            {
                await LoadModelFileNetworked(objURL);
            }

            catch (Exception ex)
            {
                throw ex;
            }


            loadStats.modelParseTime = Time.realtimeSinceStartup - lastTime;

            if (objLoadingProgress.error)
            {
                OnLoadFailed(objURL);
                return null;
            }

            lastTime = Time.realtimeSinceStartup;

            if (HasMaterialLibrary)
            {
                // materials here
                try
                {
                    await LoadMaterialLibrary(materialURL);
                }

                catch(Exception ex)
                {
                    throw ex;
                }
            }

            else
            {
                ObjectImporter.activeDownloads -= 1;
            }


            loadStats.materialsParseTime = Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;

            try
            {
                await NetworkedBuild(null, objName, objURL, diffuseTexURL, bumpTexURL, specularTexURL, opacityTexURL);
            }

            catch (Exception ex)
            {
                throw ex;
            }


            loadStats.buildTime = Time.realtimeSinceStartup - lastTime;
            loadStats.totalTime = Time.realtimeSinceStartup - startTime;
            /*
            Debug.Log("Done: " + objName
                + "\n  Loaded in " + loadStats.totalTime + " seconds"
                + "\n  Model data parsed in " + loadStats.modelParseTime + " seconds"
                + "\n  Material data parsed in " + loadStats.materialsParseTime + " seconds"
                + "\n  Game objects built in " + loadStats.buildTime + " seconds"
                + "\n    textures: " + loadStats.buildStats.texturesTime + " seconds"
                + "\n    materials: " + loadStats.buildStats.materialsTime + " seconds"
                + "\n    objects: " + loadStats.buildStats.objectsTime + " seconds"
                );
            */
            totalProgress.singleProgress.Remove(objLoadingProgress);
            OnLoaded(loadedModels[objURL], objURL);

            return loadedModels[objURL];
        }





        public IEnumerator LoadFromNetworkWebGL(string objURL, string diffuseTexURL, string bumpTexURL, string specularTexURL, string opacityTexURL, string materialURL, string objName, Action<GameObject> OnSuccess, Action<Exception> OnError)
        {
            string fileName = objName + ".obj";
#pragma warning disable
            string fileNameNoExt = objName;


            totalProgress.singleProgress.Add(objLoadingProgress);
            objLoadingProgress.fileName = fileName;
            objLoadingProgress.error = false;
            objLoadingProgress.message = "Loading " + fileName + "...";
            //Debug.LogFormat("Loading {0}\n  from: {1}...", objName, absolutePath);


            loadedModels[objURL] = null; // define a key for the dictionary
            instanceCount[objURL] = 0; // define a key for the dictionary

            float lastTime = Time.realtimeSinceStartup;
            float startTime = lastTime;

            // base model here
            
            yield return StartCoroutine(LoadModelFileNetworkedWebGL(objURL, OnError));
            
            if(ObjectImporter.isException) { yield return null; }

            loadStats.modelParseTime = Time.realtimeSinceStartup - lastTime;

            if (objLoadingProgress.error)
            {
                OnLoadFailed(objURL);
                OnError(new Exception("Load failed due to unknown reasons."));
                yield return null;
            }

            lastTime = Time.realtimeSinceStartup;

            if (HasMaterialLibrary)
            {
                // materials here
                yield return StartCoroutine(LoadMaterialLibraryWebGL(materialURL));
            }

            else
            {
                ObjectImporter.activeDownloads -= 1;
            }
            
            if (ObjectImporter.isException) { yield return null; }


            loadStats.materialsParseTime = Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;


            yield return StartCoroutine(NetworkedBuildWebGL(null, objName, objURL, diffuseTexURL, bumpTexURL, specularTexURL, opacityTexURL));

            if (ObjectImporter.isException) { yield return null; }


            loadStats.buildTime = Time.realtimeSinceStartup - lastTime;
            loadStats.totalTime = Time.realtimeSinceStartup - startTime;
            /*
            Debug.Log("Done: " + objName
                + "\n  Loaded in " + loadStats.totalTime + " seconds"
                + "\n  Model data parsed in " + loadStats.modelParseTime + " seconds"
                + "\n  Material data parsed in " + loadStats.materialsParseTime + " seconds"
                + "\n  Game objects built in " + loadStats.buildTime + " seconds"
                + "\n    textures: " + loadStats.buildStats.texturesTime + " seconds"
                + "\n    materials: " + loadStats.buildStats.materialsTime + " seconds"
                + "\n    objects: " + loadStats.buildStats.objectsTime + " seconds"
                );
            */
            totalProgress.singleProgress.Remove(objLoadingProgress);
            OnLoaded(loadedModels[objURL], objURL);

            OnSuccess(loadedModels[objURL]);
        }



        /// <summary>
        /// Parse the model to get a list of the paths of all used textures
        /// </summary>
        /// <param name="absolutePath">absolute path of the model</param>
        /// <returns>List of paths of the textures referenced by the model</returns>
        public abstract string[] ParseTexturePaths(string absolutePath);

        /// <summary>
        /// Load the main model file
        /// </summary>
        /// <param name="absolutePath">absolute file path</param>
        /// <remarks>This is called by Load() method</remarks>
        protected abstract Task LoadModelFile(string absolutePath, string texturesFolderPath = "", string materialsFolderPath = "");



        protected abstract Task LoadModelFileNetworked(string objURL);

        protected abstract IEnumerator LoadModelFileNetworkedWebGL(string objURL, Action<Exception> OnError);

        /// <summary>
        /// Load the material library from the given path.
        /// </summary>
        /// <param name="absolutePath">absolute file path</param>
        /// <remarks>This is called by Load() method</remarks>
        protected abstract Task LoadMaterialLibrary(string absolutePath, string materialsFolderPath = "");

        
        protected abstract Task LoadMaterialLibrary(string materialURL);

        protected abstract IEnumerator LoadMaterialLibraryWebGL(string materialURL);

        /// <summary>
        /// Build the game objects from data set, materials and textures.
        /// </summary>
        /// <param name="absolutePath">absolute file path</param>
        /// <param name="objName">Name of the main game object (model root)</param>
        /// <param name="parentTransform">transform to which the model root will be attached (if null it will be a root aobject)</param>
        /// <remarks>This is called by Load() method</remarks>
        protected async Task Build(string absolutePath, string objName, Transform parentTransform, string texturesFolderPath = "")
        {
            float prevTime = Time.realtimeSinceStartup;
            if (materialData != null)
            {
                //string basePath = GetDirName(absolutePath);
                string basePath = Path.GetDirectoryName(absolutePath);

                objLoadingProgress.message = "Loading textures...";
                int count = 0;
                foreach (MaterialData mtl in materialData)
                {
                    objLoadingProgress.percentage = LOAD_PHASE_PERC + TEXTURE_PHASE_PERC * count / materialData.Count;
                    count++;
                    if (mtl.diffuseTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.diffuseTex = LoadAssetTexture(mtl.diffuseTexPath);
                        }
                        else
#endif
                        {

                            await LoadMaterialTexture(basePath, mtl.diffuseTexPath, texturesFolderPath);
                            
                            mtl.diffuseTex = loadedTexture;
                        }
                    }

                    if (mtl.bumpTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.bumpTex = LoadAssetTexture(mtl.bumpTexPath);
                        }
                        else
#endif
                        {
                            await LoadMaterialTexture(basePath, mtl.bumpTexPath, texturesFolderPath);
                            
                            mtl.bumpTex = loadedTexture;
                        }
                    }

                    if (mtl.specularTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.specularTex = LoadAssetTexture(mtl.specularTexPath);
                        }
                        else
#endif
                        {
                            await LoadMaterialTexture(basePath, mtl.specularTexPath, texturesFolderPath);
                            
                            mtl.specularTex = loadedTexture;
                        }
                    }

                    if (mtl.opacityTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.opacityTex = LoadAssetTexture(mtl.opacityTexPath);
                        }
                        else
#endif
                        {                          
                            await LoadMaterialTexture(basePath, mtl.opacityTexPath, texturesFolderPath);
                            
                            mtl.opacityTex = loadedTexture;
                        }
                    }
                }
            }
            loadStats.buildStats.texturesTime = Time.realtimeSinceStartup - prevTime;
            prevTime = Time.realtimeSinceStartup;

            ObjectBuilder.ProgressInfo info = new ObjectBuilder.ProgressInfo();

            objLoadingProgress.message = "Loading materials...";

            //yield return null;

#if UNITY_EDITOR
            objectBuilder.alternativeTexPath = altTexPath;
#endif
            objectBuilder.buildOptions = buildOptions;
            bool hasColors = dataSet.colorList.Count > 0;
            bool hasMaterials = materialData != null;
            objectBuilder.InitBuildMaterials(materialData, hasColors);
            float objInitPerc = objLoadingProgress.percentage;
            if (hasMaterials)
            {
                while (objectBuilder.BuildMaterials(info))
                {
                    objLoadingProgress.percentage = objInitPerc + MATERIAL_PHASE_PERC * objectBuilder.NumImportedMaterials / materialData.Count;
                }
                loadStats.buildStats.materialsTime = Time.realtimeSinceStartup - prevTime;
                prevTime = Time.realtimeSinceStartup;
            }

            objLoadingProgress.message = "Building scene objects...";

            GameObject newObj = new GameObject(objName);

            if (buildOptions.hideWhileLoading)
            {
                newObj.SetActive(false);
            }

            if (parentTransform != null) newObj.transform.SetParent(parentTransform.transform, false);
            OnCreated(newObj, absolutePath);
            ////newObj.transform.localScale = Vector3.one * Scaling;
            float initProgress = objLoadingProgress.percentage;
            objectBuilder.StartBuildObjectAsync(dataSet, newObj);

            while (objectBuilder.BuildObjectAsync(ref info))
            {
                objLoadingProgress.message = "Building scene objects... " + (info.objectsLoaded + info.groupsLoaded) + "/" + (dataSet.objectList.Count + info.numGroups);
                objLoadingProgress.percentage = initProgress + BUILD_PHASE_PERC * (info.objectsLoaded / dataSet.objectList.Count + (float)info.groupsLoaded / info.numGroups);
            }

            objLoadingProgress.percentage = 100.0f;
            loadedModels[absolutePath] = newObj;
            loadStats.buildStats.objectsTime = Time.realtimeSinceStartup - prevTime;

        }




        protected async Task NetworkedBuild(Transform parentTransform, string objName, string objURL, string diffuseTexURL, string bumpTexURL, string specularTexURL, string opacityTexURL)
        {
            float prevTime = Time.realtimeSinceStartup;
            if (materialData != null)
            {

                objLoadingProgress.message = "Loading textures...";
                int count = 0;
                foreach (MaterialData mtl in materialData)
                {
                    objLoadingProgress.percentage = LOAD_PHASE_PERC + TEXTURE_PHASE_PERC * count / materialData.Count;
                    count++;
                    if (mtl.diffuseTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.diffuseTex = LoadAssetTexture(mtl.diffuseTexPath);
                        }
                        else
#endif
                        {
                            if(!string.IsNullOrWhiteSpace(diffuseTexURL))
                            {
                                try
                                {
                                    await LoadMaterialTexture(diffuseTexURL);
                                }
                                catch (Exception exc)
                                {
                                    throw exc;
                                }
                            }

                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }
                            mtl.diffuseTex = loadedTexture;

                        }
                    }

                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;



                    if (mtl.bumpTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.bumpTex = LoadAssetTexture(mtl.bumpTexPath);
                        }
                        else
#endif
                        {

                            if (!string.IsNullOrWhiteSpace(bumpTexURL))
                            {
                                try
                                {
                                    await LoadMaterialTexture(bumpTexURL);
                                }
                                catch (Exception exc)
                                {
                                    throw exc;
                                }
                            }

                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }

                            mtl.bumpTex = loadedTexture;
                        }
                    }
                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;



                    if (mtl.specularTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.specularTex = LoadAssetTexture(mtl.specularTexPath);
                        }
                        else
#endif
                        {
                            if (!string.IsNullOrWhiteSpace(specularTexURL))
                            {
                                try
                                {
                                    await LoadMaterialTexture(specularTexURL);
                                }
                                catch (Exception exc)
                                {
                                    throw exc;
                                }
                            }
                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }

                            mtl.specularTex = loadedTexture;
                        }
                    }
                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;



                    if (mtl.opacityTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.opacityTex = LoadAssetTexture(mtl.opacityTexPath);
                        }
                        else
#endif
                        {
                            if (!string.IsNullOrWhiteSpace(opacityTexURL))
                            {
                                try
                                {
                                    await LoadMaterialTexture(opacityTexURL);
                                }
                                catch (Exception exc)
                                {
                                    throw exc;
                                }
                            }
                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }

                            mtl.opacityTex = loadedTexture;
                        }
                    }
                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;

                }
            }
            loadStats.buildStats.texturesTime = Time.realtimeSinceStartup - prevTime;
            prevTime = Time.realtimeSinceStartup;

            ObjectBuilder.ProgressInfo info = new ObjectBuilder.ProgressInfo();

            objLoadingProgress.message = "Loading materials...";

            //yield return null;

#if UNITY_EDITOR
            objectBuilder.alternativeTexPath = altTexPath;
#endif
            objectBuilder.buildOptions = buildOptions;
            bool hasColors = dataSet.colorList.Count > 0;
            bool hasMaterials = materialData != null;
            objectBuilder.InitBuildMaterials(materialData, hasColors);
            float objInitPerc = objLoadingProgress.percentage;
            if (hasMaterials)
            {
                while (objectBuilder.BuildMaterials(info))
                {
                    objLoadingProgress.percentage = objInitPerc + MATERIAL_PHASE_PERC * objectBuilder.NumImportedMaterials / materialData.Count;
                    await Task.Delay(0);
                }
                loadStats.buildStats.materialsTime = Time.realtimeSinceStartup - prevTime;
                prevTime = Time.realtimeSinceStartup;
            }

            objLoadingProgress.message = "Building scene objects...";

            GameObject newObj = new GameObject(objName);

            if (buildOptions.hideWhileLoading)
            {
                newObj.SetActive(false);
            }

            if (parentTransform != null) newObj.transform.SetParent(parentTransform.transform, false);
            OnCreated(newObj, objURL);
            ////newObj.transform.localScale = Vector3.one * Scaling;
            float initProgress = objLoadingProgress.percentage;
            objectBuilder.StartBuildObjectAsync(dataSet, newObj);

            while (objectBuilder.BuildObjectAsync(ref info))
            {
                objLoadingProgress.message = "Building scene objects... " + (info.objectsLoaded + info.groupsLoaded) + "/" + (dataSet.objectList.Count + info.numGroups);
                objLoadingProgress.percentage = initProgress + BUILD_PHASE_PERC * (info.objectsLoaded / dataSet.objectList.Count + (float)info.groupsLoaded / info.numGroups);
                await Task.Delay(0);
            }

            objLoadingProgress.percentage = 100.0f;
            loadedModels[objURL] = newObj;
            loadStats.buildStats.objectsTime = Time.realtimeSinceStartup - prevTime;

        }



        
        protected IEnumerator NetworkedBuildWebGL(Transform parentTransform, string objName, string objURL, string diffuseTexURL, string bumpTexURL, string specularTexURL, string opacityTexURL)
        {
            float prevTime = Time.realtimeSinceStartup;

            if (materialData != null)
            {

                objLoadingProgress.message = "Loading textures...";
                int count = 0;
                foreach (MaterialData mtl in materialData)
                {
                    objLoadingProgress.percentage = LOAD_PHASE_PERC + TEXTURE_PHASE_PERC * count / materialData.Count;
                    count++;
                    if (mtl.diffuseTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.diffuseTex = LoadAssetTexture(mtl.diffuseTexPath);
                        }
                        else
#endif
                        {
                            if (!string.IsNullOrWhiteSpace(diffuseTexURL))
                            {
                                yield return StartCoroutine(LoadMaterialTextureWebGL(diffuseTexURL));
                            }

                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }
                            mtl.diffuseTex = loadedTexture;

                        }
                    }

                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;



                    if (mtl.bumpTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.bumpTex = LoadAssetTexture(mtl.bumpTexPath);
                        }
                        else
#endif
                        {

                            if (!string.IsNullOrWhiteSpace(bumpTexURL))
                            {
                                yield return StartCoroutine(LoadMaterialTextureWebGL(bumpTexURL));
                            }

                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }

                            mtl.bumpTex = loadedTexture;
                        }
                    }
                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;



                    if (mtl.specularTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.specularTex = LoadAssetTexture(mtl.specularTexPath);
                        }
                        else
#endif
                        {
                            if (!string.IsNullOrWhiteSpace(specularTexURL))
                            {
                                yield return StartCoroutine(LoadMaterialTextureWebGL(specularTexURL));
                            }
                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }

                            mtl.specularTex = loadedTexture;
                        }
                    }
                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;



                    if (mtl.opacityTexPath != null)
                    {
#if UNITY_EDITOR
                        if (ImportingAssets)
                        {
                            mtl.opacityTex = LoadAssetTexture(mtl.opacityTexPath);
                        }
                        else
#endif
                        {
                            if (!string.IsNullOrWhiteSpace(opacityTexURL))
                            {
                                yield return StartCoroutine(LoadMaterialTextureWebGL(opacityTexURL));
                            }
                            else
                            {
                                ObjectImporter.activeDownloads -= 1;
                            }

                            mtl.opacityTex = loadedTexture;
                        }
                    }
                    else
                    {
                        ObjectImporter.activeDownloads -= 1;
                    }

                    ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;

                }
            }
            loadStats.buildStats.texturesTime = Time.realtimeSinceStartup - prevTime;
            prevTime = Time.realtimeSinceStartup;

            ObjectBuilder.ProgressInfo info = new ObjectBuilder.ProgressInfo();

            objLoadingProgress.message = "Loading materials...";

            //yield return null;

#if UNITY_EDITOR
            objectBuilder.alternativeTexPath = altTexPath;
#endif
            objectBuilder.buildOptions = buildOptions;
            bool hasColors = dataSet.colorList.Count > 0;
            bool hasMaterials = materialData != null;
            objectBuilder.InitBuildMaterials(materialData, hasColors);
            float objInitPerc = objLoadingProgress.percentage;
            if (hasMaterials)
            {
                while (objectBuilder.BuildMaterials(info))
                {
                    objLoadingProgress.percentage = objInitPerc + MATERIAL_PHASE_PERC * objectBuilder.NumImportedMaterials / materialData.Count;
                }
                loadStats.buildStats.materialsTime = Time.realtimeSinceStartup - prevTime;
                prevTime = Time.realtimeSinceStartup;
            }

            objLoadingProgress.message = "Building scene objects...";

            GameObject newObj = new GameObject(objName);

            if (buildOptions.hideWhileLoading)
            {
                newObj.SetActive(false);
            }

            if (parentTransform != null) newObj.transform.SetParent(parentTransform.transform, false);
            OnCreated(newObj, objURL);
            ////newObj.transform.localScale = Vector3.one * Scaling;
            float initProgress = objLoadingProgress.percentage;
            objectBuilder.StartBuildObjectAsync(dataSet, newObj);

            while (objectBuilder.BuildObjectAsync(ref info))
            {
                objLoadingProgress.message = "Building scene objects... " + (info.objectsLoaded + info.groupsLoaded) + "/" + (dataSet.objectList.Count + info.numGroups);
                objLoadingProgress.percentage = initProgress + BUILD_PHASE_PERC * (info.objectsLoaded / dataSet.objectList.Count + (float)info.groupsLoaded / info.numGroups);
            }

            objLoadingProgress.percentage = 100.0f;
            loadedModels[objURL] = newObj;
            loadStats.buildStats.objectsTime = Time.realtimeSinceStartup - prevTime;

        }


        /// <summary>
        /// Get the directory name of the given path, appending the final slash if eeded.
        /// </summary>
        /// <param name="absolutePath">the absolute path</param>
        /// <returns>the directory name ending with `/`</returns>
        protected string GetDirName(string absolutePath)
        {
            string basePath;
            if (absolutePath.Contains("//"))
            {
                basePath = absolutePath.Remove(absolutePath.LastIndexOf('/') + 1);
            }
            else
            {
                string dirName = Path.GetDirectoryName(absolutePath);
                basePath = string.IsNullOrEmpty(dirName) ? "" : dirName;
                if (!basePath.EndsWith("/"))
                {
                    basePath += "/";
                }
            }
            return basePath;
        }


        protected virtual void OnLoaded(GameObject obj, string absolutePath)
        {
            if (obj == null)
            {
                if (ModelError != null)
                {
                    ModelError(absolutePath);
                }
            }
            else
            {
                if (buildOptions != null)
                {
                    obj.transform.localPosition = buildOptions.localPosition;
                    obj.transform.localRotation = Quaternion.Euler(buildOptions.localEulerAngles); ;
                    obj.transform.localScale = buildOptions.localScale;
                    if (buildOptions.inheritLayer)
                    {
                        obj.layer = obj.transform.parent.gameObject.layer;
                        MeshRenderer[] mrs = obj.transform.GetComponentsInChildren<MeshRenderer>(true);
                        for (int i = 0; i < mrs.Length; i++)
                        {
                            mrs[i].gameObject.layer = obj.transform.parent.gameObject.layer;
                        }
                    }
                }
                if (buildOptions.hideWhileLoading)
                {
                    obj.SetActive(true);
                }

                if (ModelLoaded != null)
                {
                    ModelLoaded(obj, absolutePath);
                }
            }
        }


        protected virtual void OnCreated(GameObject obj, string absolutePath)
        {
            if (obj == null)
            {
                if (ModelError != null)
                {
                    ModelError(absolutePath);
                }
            }
            else
            {
                if (ModelCreated != null)
                {
                    ModelCreated(obj, absolutePath);
                }
            }
        }


        protected virtual void OnLoadFailed(string absolutePath)
        {
            if (ModelError != null)
            {
                ModelError(absolutePath);
            }
        }


#if UNITY_EDITOR
        /// <summary>
        /// Load a texture from the asset database
        /// </summary>
        /// <param name="texturePath">texture path inside the asset database</param>
        /// <returns>the loaded texture or null on error</returns>
        private Texture2D LoadAssetTexture(string texturePath)
        {
            FileInfo textFileInfo = new FileInfo(texturePath);
            string texpath = altTexPath + textFileInfo.Name;
            texpath = texpath.Replace("//", "/");
            Debug.LogFormat("Loading texture asset '{0}'", texpath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(texpath);
        }
#endif


        /// <summary>
        /// Convert a texture path to a texture URL and update the progress message
        /// </summary>
        /// <param name="basePath">base texture path</param>
        /// <param name="texturePath">relative texture path</param>
        /// <returns>URL of the texture</returns>
        private string GetTextureUrl(string basePath, string texturePath)
        {
            string texPath = texturePath.Replace("\\", "/").Replace("//", "/");
            if (!Path.IsPathRooted(texPath))
            {
                texPath = basePath + texturePath;
            }
            if (!texPath.Contains("//"))
            {
                texPath = "file:///" + texPath;
            }
            objLoadingProgress.message = "Loading textures...\n" + texPath;
            return texPath;
        }

        private async Task LoadMaterialTexture(string basePath, string path, string texturesFolderPath="")
        {
            loadedTexture = null;
            //string texPath = GetTextureUrl(basePath, path);
            string textPath = string.IsNullOrWhiteSpace(texturesFolderPath) ? basePath + path : (texturesFolderPath + "\\" + path);

            textPath = Path.GetFullPath(textPath);

            //WWW loader = new WWW(texPath);
            //yield return loader;


            if (File.Exists(textPath))
            {

                byte[] result;

                using (FileStream stream = File.Open(textPath, FileMode.Open))
                {
                    result = new byte[stream.Length];
                    await stream.ReadAsync(result, 0, (int)stream.Length);
                }

                if (result.Length > 0)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(result);
                    loadedTexture = tex;
                }
            }

            else
            {
                Debug.LogWarning("Failed to load texture at path  " + textPath + "   BasePath  " + basePath + "  path  " + path);
            }


        }



        private async Task LoadMaterialTexture(string textureURL)
        {
            loadedTexture = null;

            bool isWorking = true;
            byte[] downloadedBytes = null;
            float oldProgress = individualProgress.Value;


            try
            {
                StartCoroutine(DownloadFile(textureURL, individualProgress, (bytes) =>
                {
                    isWorking = false;
                    downloadedBytes = bytes;
                    //loadedText = Encoding.UTF8.GetString(bytes);
                },
                (error) =>
                {
                    ObjectImporter.activeDownloads -= 1;
                    isWorking = false;
                    Debug.LogWarning("Failed to load the associated texture file." + error);
                }));

            }

            catch (Exception exc)
            {
                ObjectImporter.activeDownloads -= 1;
                individualProgress.Value = oldProgress;
                ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;
                isWorking = false;
                throw exc;
            }


            while (isWorking)
            {
                ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;
                await Task.Delay(3);
            }

            ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;

            if (downloadedBytes != null && downloadedBytes.Length > 0)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(downloadedBytes);
                loadedTexture = tex;
            }
            

            else
            {
                Debug.LogWarning("Failed to load texture.");
            }


        }





        private IEnumerator LoadMaterialTextureWebGL(string textureURL)
        {
            loadedTexture = null;

            bool isWorking = true;
            float oldProgress = individualProgress.Value;


            try
            {
                StartCoroutine(DownloadTexFileWebGL(textureURL, individualProgress, (texture) =>
                {
                    isWorking = false;
                    loadedTexture = texture;
                    //loadedText = Encoding.UTF8.GetString(bytes);
                },
                (error) =>
                {
                    ObjectImporter.activeDownloads -= 1;
                    isWorking = false;
                    Debug.LogWarning("Failed to load the associated texture file." + error);
                }));

            }

            catch (Exception exc)
            {
                ObjectImporter.activeDownloads -= 1;
                individualProgress.Value = oldProgress;
                ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;
                isWorking = false;
                throw exc;
            }


            while (isWorking)
            {
                yield return new WaitForSeconds(0.1f);
                ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;
            }

            ObjectImporter.downloadProgress.Value = (individualProgress.Value / ObjectImporter.activeDownloads) * 100f;


            if (loadedTexture == null)
            {
                Debug.LogWarning("Failed to load texture.");
            }


        }


        /// <summary>
        /// Load a texture from the URL got from the parameter.
        /// </summary>
#if UNITY_2018_3_OR_NEWER
        private Texture2D LoadTexture(UnityWebRequest loader)
#else
        private Texture2D LoadTexture(WWW loader)
#endif
        {
            string ext = Path.GetExtension(loader.url).ToLower();
            Texture2D tex = null;

            // TODO: add support for more formats (bmp, gif, dds, ...)
            if (ext == ".tga")
            {
                tex = TextureLoader.LoadTextureFromUrl(loader.url);
                //tex = TgaLoader.LoadTGA(new MemoryStream(loader.bytes));
            }
            else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
#if UNITY_2018_3_OR_NEWER
                tex = DownloadHandlerTexture.GetContent(loader);
#else
                tex = loader.texture;
#endif
            }
            else
            {
                Debug.LogWarning("Unsupported texture format: " + ext);
            }

            if (tex == null)
            {
                Debug.LogErrorFormat("Failed to load texture {0}", loader.url);
            }
            else
            {
                //tex.alphaIsTransparency = true;
                //tex.filterMode = FilterMode.Trilinear;
            }

            return tex;
        }


        protected struct BuildStats
        {
            public float texturesTime;
            public float materialsTime;
            public float objectsTime;
        }


        protected struct Stats
        {
            public float modelParseTime;
            public float materialsParseTime;
            public float buildTime;
            public BuildStats buildStats;
            public float totalTime;
        }



        public IEnumerator DownloadFile(string url, ReferencedNumeric<float> downloadProgress, Action<byte[]> DownloadComplete, Action<string> OnError)
        {
            //Debug.Log("Into the DownloadFile Coroutine");
            UnityWebRequest webrequest = null;
            float oldProgress = downloadProgress.Value;

            try
            {
                webrequest = new UnityWebRequest(url);
                webrequest.downloadHandler = new DownloadHandlerBuffer();
            }

            catch (Exception ex)
            {
                downloadProgress.Value = oldProgress;
                OnError(ex.ToString());
            }
            

            Coroutine progress = StartCoroutine(GetProgress(webrequest, downloadProgress));

            yield return webrequest.SendWebRequest();

            if (!string.IsNullOrWhiteSpace(webrequest.error))
            {
                downloadProgress.Value = oldProgress;
                OnError(webrequest.error);
            }

            else
            {
                if (webrequest.downloadedBytes == 0)
                {
                    if (string.IsNullOrWhiteSpace(webrequest.error))
                    {
                        downloadProgress.Value = oldProgress;
                        OnError("No bytes downloaded. The file might be empty.");
                    }

                    else
                    {
                        downloadProgress.Value = oldProgress;
                        OnError(webrequest.error);
                    }
                }

                else
                {
                    if (string.IsNullOrWhiteSpace(webrequest.error))
                    {
                        //Debug.Log("Calling Download Complete");

                        downloadProgress.Value = oldProgress + 1;
                        DownloadComplete(webrequest.downloadHandler.data);
                    }
                    else
                    {
                        downloadProgress.Value = oldProgress;
                        OnError(webrequest.error);
                    }
                }


            }

            StopCoroutine(progress);

            webrequest.Dispose();
            //Debug.Log("End of File Download Coroutine.");

        }



        private IEnumerator GetProgress(UnityWebRequest webrequest, ReferencedNumeric<float> downloadProgress)
        {
            float oldProgress = downloadProgress.Value;

            if (webrequest != null && downloadProgress != null)
            {
                while (!webrequest.downloadHandler.isDone && string.IsNullOrWhiteSpace(webrequest.error))
                {
                    yield return new WaitForSeconds(0.1f);
                    downloadProgress.Value = oldProgress + webrequest.downloadProgress;
                }

                if(webrequest.downloadHandler.isDone && string.IsNullOrWhiteSpace(webrequest.error))
                {
                    downloadProgress.Value = oldProgress + webrequest.downloadProgress;
                    Debug.Log("Progress  " + webrequest.downloadProgress);
                }

            }

        }



        public IEnumerator DownloadFileWebGL(string url, ReferencedNumeric<float> downloadProgress, Action<string> DownloadComplete, Action<string> OnError)
        {
            UnityWebRequest webrequest = null;
            float oldProgress = downloadProgress.Value;

            try
            {
                webrequest = new UnityWebRequest(url);
                webrequest.downloadHandler = new DownloadHandlerBuffer();
            }

            catch (Exception ex)
            {
                downloadProgress.Value = oldProgress;
                OnError(ex.ToString());
            }


            Coroutine progress = StartCoroutine(GetProgress(webrequest, downloadProgress));

            yield return webrequest;

            if (!string.IsNullOrWhiteSpace(webrequest.error))
            {
                downloadProgress.Value = oldProgress;
                OnError(webrequest.error);
            }

            else
            {
                if (webrequest.downloadedBytes == 0)
                {
                    if (string.IsNullOrWhiteSpace(webrequest.error))
                    {
                        downloadProgress.Value = oldProgress;
                        OnError("No bytes downloaded. The file might be empty.");
                    }

                    else
                    {
                        downloadProgress.Value = oldProgress;
                        OnError(webrequest.error);
                    }
                }

                else
                {
                    if (string.IsNullOrWhiteSpace(webrequest.error))
                    {
                        downloadProgress.Value = oldProgress + 1;
                        DownloadComplete(webrequest.downloadHandler.text); 
                    }
                    else
                    {
                        downloadProgress.Value = oldProgress;
                        OnError(webrequest.error);
                    }
                }


            }

            try
            {
                StopCoroutine(progress);
            }

            catch (Exception ex)
            {

            }

            webrequest.Dispose();

        }



        public IEnumerator DownloadTexFileWebGL(string url, ReferencedNumeric<float> downloadProgress, Action<Texture2D> DownloadComplete, Action<string> OnError)
        {
            UnityWebRequest webrequest = null;
            float oldProgress = downloadProgress.Value;

            try
            {
                webrequest = new UnityWebRequest(url);
                webrequest.downloadHandler = new DownloadHandlerBuffer();
            }

            catch (Exception ex)
            {
                downloadProgress.Value = oldProgress;
                OnError(ex.ToString());
            }


            Coroutine progress = StartCoroutine(GetProgress(webrequest, downloadProgress));

            yield return webrequest;

            if (!string.IsNullOrWhiteSpace(webrequest.error))
            {
                downloadProgress.Value = oldProgress;
                OnError(webrequest.error);
            }

            else
            {
                if (webrequest.downloadedBytes == 0)
                {
                    if (string.IsNullOrWhiteSpace(webrequest.error))
                    {
                        downloadProgress.Value = oldProgress;
                        OnError("No bytes downloaded. The file might be empty.");
                    }

                    else
                    {
                        downloadProgress.Value = oldProgress;
                        OnError(webrequest.error);
                    }
                }

                else
                {
                    if (string.IsNullOrWhiteSpace(webrequest.error))
                    {

                        downloadProgress.Value = oldProgress + 1;
                        DownloadComplete(((DownloadHandlerTexture)webrequest.downloadHandler).texture);
                    }
                    else
                    {
                        downloadProgress.Value = oldProgress;
                        OnError(webrequest.error);
                    }
                }


            }

            StopCoroutine(progress);

            webrequest.Dispose();
        }

        
    }
}
