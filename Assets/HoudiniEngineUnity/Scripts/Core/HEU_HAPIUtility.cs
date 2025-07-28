/*
 * Copyright (c) <2020> Side Effects Software Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *
 * 2. The name of Side Effects Software may not be used to endorse or
 *    promote products derived from this software without specific prior
 *    written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY SIDE EFFECTS SOFTWARE "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN
 * NO EVENT SHALL SIDE EFFECTS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#if (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX)
#define HOUDINIENGINEUNITY_ENABLED
#endif

using System.Text;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Expose public classes/functions
#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HoudiniEngineUnityEditor")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityEditorTests")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityPlayModeTests")]
#endif

namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;
    using HAPI_PartId = System.Int32;
    using HAPI_AssetLibraryId = System.Int32;
    using HAPI_StringHandle = System.Int32;
    using HAPI_ErrorCodeBits = System.Int32;


    /// <summary>
    /// General utlitity functions.
    /// </summary>
    public static class HEU_HAPIUtility
    {
        /// <summary>
        /// Return Houdini Engine installation and session information.
        /// Tries to use existing or creates new session to find information.
        /// </summary>
        /// <returns>String containing installation and session information.</returns>
        public static string GetHoudiniEngineInstallationInfo()
        {
#if HOUDINIENGINEUNITY_ENABLED
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Plugin was built with:");
            sb.AppendFormat("  Houdini: {0}.{1}.{2}\n",
                HEU_HoudiniVersion.HOUDINI_MAJOR,
                HEU_HoudiniVersion.HOUDINI_MINOR,
                HEU_HoudiniVersion.HOUDINI_BUILD);

            sb.AppendFormat("  Houdini Engine: {0}.{1}.{2}\n\n",
                HEU_HoudiniVersion.HOUDINI_ENGINE_MAJOR,
                HEU_HoudiniVersion.HOUDINI_ENGINE_MINOR,
                HEU_HoudiniVersion.HOUDINI_ENGINE_API);

            // Check if existing session is valid, or create a new session. Then query installation information.
            HEU_SessionBase session = HEU_SessionManager.GetDefaultSession();
            if (session != null && session.IsSessionValid())
            {
                sb.AppendLine("Current  session is using:");

                int hMajor = session.GetEnvInt(HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_MAJOR);
                int hMinor = session.GetEnvInt(HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_MINOR);
                int hBuild = session.GetEnvInt(HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_BUILD);

                int heuPatch = session.GetEnvInt(HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_PATCH);

                int heuMajor = session.GetEnvInt(HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MAJOR);
                int heuMinor = session.GetEnvInt(HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MINOR);
                int heuAPI = session.GetEnvInt(HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_API);

                sb.AppendFormat("  Houdini: {0}.{1}.{2}{3}\n", hMajor, hMinor, hBuild, (heuPatch > 0) ? "." + heuPatch.ToString() : "");
                sb.AppendFormat("  Houdini Engine: {0}.{1}.{2}\n\n", heuMajor, heuMinor, heuAPI);

                sb.AppendFormat("  Houdini binaries: {0}\n", HEU_Platform.GetHoudiniEnginePath() + HEU_HoudiniVersion.HAPI_BIN_PATH);
                sb.AppendFormat("  HEU: {0}\n\n", HEU_HoudiniVersion.UNITY_PLUGIN_VERSION);

                sb.Append("  License acquired: ");
                HAPI_License license = HEU_SessionManager.GetCurrentLicense(false);
                switch (license)
                {
                    case HAPI_License.HAPI_LICENSE_NONE:
                        sb.Append("None\n");
                        break;
                    case HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE:
                        sb.Append("Houdini Engine\n");
                        break;
                    case HAPI_License.HAPI_LICENSE_HOUDINI:
                        sb.Append("Houdini (Escape)\n");
                        break;
                    case HAPI_License.HAPI_LICENSE_HOUDINI_FX:
                        sb.Append("Houdini FX\n");
                        break;
                    case HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE_INDIE:
                        sb.Append("Houdini Engine Indie");
                        break;
                    case HAPI_License.HAPI_LICENSE_HOUDINI_INDIE:
                        sb.Append("Houdini Indie\n");
                        break;
                    case HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE_UNITY_UNREAL:
                        sb.Append("Houdini Engine for Unity/Unreal\n");
                        break;
                    default:
                        sb.Append("Unknown\n");
                        break;
                }

                HEU_SessionData sessionData = session.GetSessionData();
                if (sessionData != null)
                {
                    sb.AppendLine(session.GetSessionInfo());
                }
            }
            else // Unable to establish a session
            {
                sb.AppendLine("Unable to detect Houdini Engine installation.");
                sb.AppendLine("License Type Acquired: Unknown\n");
                if (session != null)
                {
                    sb.AppendLine("Failure possibly due to: " + session.GetLastSessionError());
                }
            }

            sb.AppendLine();
            sb.Append("System PATH: \n" + GetEnvironmentPath());

            HEU_Logger.Log(sb.ToString());

            return sb.ToString();
#else
	    return "";
#endif
        }


        /// <summary>
        /// Return the PATH environment value for current process.
        /// </summary>
        /// <returns>The PATH environment string.</returns>
        public static string GetEnvironmentPath()
        {
            string pathStr = System.Environment.GetEnvironmentVariable("PATH", System.EnvironmentVariableTarget.Process);

#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
            pathStr = pathStr.Replace(";", "\n");
#elif (UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX)))
	    pathStr = pathStr.Replace(":", "\n");
#endif
            return pathStr;
        }

        /// <summary>
        /// Returns the real file path if inPath has HFS environment mapping.
        /// </summary>
        /// <param name="inPath">Path to map</param>
        /// <returns>Mapped path or same path if no mapping</returns>
        public static string GetRealPathFromHFSPath(string inPath)
        {
            if (inPath.StartsWith(HEU_Defines.HEU_PATH_KEY_HFS, System.StringComparison.InvariantCulture))
            {
                string hpath = HEU_Platform.GetHoudiniEnginePath();
                if (!string.IsNullOrEmpty(hpath))
                {
                    return inPath.Replace(HEU_Defines.HEU_PATH_KEY_HFS, hpath);
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Unable to convert {0} in path {1} due to not getting valid Houdini path!",
                        HEU_Defines.HEU_PATH_KEY_HFS, inPath);
                }
            }

            return inPath;
        }

        /// <summary>
        /// Returns true if file or folder at inPath exists. Supports inPath with environment mapped values
        /// such as <HFS> or $.
        /// </summary>
        /// <param name="inPath">Path to check. Could be either file or folder.</param>
        /// <returns>True if file or folder exists</returns>
        public static bool DoesMappedPathExist(string inPath)
        {
            string realPath = HEU_PluginStorage.Instance.ConvertEnvKeyedPathToReal(inPath);
            return HEU_Platform.DoesPathExist(HEU_Platform.GetFullPath(realPath));
        }

        /// <summary>
        /// Returns true if given file is a Houdini Digital Asset file.
        /// </summary>
        /// <param name="filePath">File name to check</param>
        /// <returns>True if file is a Houdini Digital Asset</returns>
        public static bool IsHoudiniAssetFile(string filePath)
        {
            return (filePath.EndsWith(".otl", System.StringComparison.OrdinalIgnoreCase)
                    || filePath.EndsWith(".otllc", System.StringComparison.OrdinalIgnoreCase)
                    || filePath.EndsWith(".otlnc", System.StringComparison.OrdinalIgnoreCase)
                    || filePath.EndsWith(".hda", System.StringComparison.OrdinalIgnoreCase)
                    || filePath.EndsWith(".hdalc", System.StringComparison.OrdinalIgnoreCase)
                    || filePath.EndsWith(".hdanc", System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds Houdini asset at given file path by matching potential extensions.
        /// </summary>
        /// <param name="filePath">Path of file without extension</param>
        /// <returns>Valid path with extension or null of no asset found</returns>
        public static string FindHoudiniAssetFileInPathWithExt(string filePath)
        {
            string[] extensions = new string[] { ".otl", ".otllc", ".otlnc", ".hda", ".hdalc", ".hdanc" };
            foreach (string ext in extensions)
            {
                string newPath = filePath + ext;
                if (HEU_Platform.DoesFileExist(newPath))
                {
                    return newPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Abstraction around Unity warning logger so provide some control for logging.
        /// </summary>
        /// <param name="message">String message to log</param>
        public static void Log(string message)
        {
            HEU_Logger.Log(message);
        }

        /// <summary>
        /// Abstraction around Unity warning logger so provide some control for logging.
        /// </summary>
        /// <param name="message">String message to log</param>
        public static void LogWarning(string message)
        {
            HEU_Logger.LogWarning(message);
        }

        /// <summary>
        /// Abstraction around Unity error logger so provide some control for logging.
        /// </summary>
        /// <param name="message">String message to log</param>
        public static void LogError(string message)
        {
            HEU_Logger.LogError(message);
        }

        /// <summary>
        /// For the given object, returns its file path if it exists.
        /// </summary>
        /// <param name="inObject">Object to get the path for.</param>
        /// <returns>Valid path or null if none found.</returns>
        public static string LocateValidFilePath(UnityEngine.Object inObject)
        {
            return inObject != null ? HEU_AssetDatabase.GetAssetPath(inObject) : null;
        }

        /// <summary>
        /// For the file path, returns a valid location if exists.
        /// If inFilePath is not valid, it uses the file name to search the asset database to 
        /// find the actual valid location (in case it was moved).
        /// </summary>
        /// <param name="gameObjectName">Name of the asset for which to find the path.</param>
        /// <param name="inFilePath">Current path of the asset to validate. Could be null or invalid.</param>
        /// <returns>Valid path or null if none found.</returns>
        public static string LocateValidFilePath(string assetName, string inFilePath)
        {
#if UNITY_EDITOR
            // Convert in path to real path if it was environment mapped previously (ie. has $key/blah.hda)
            inFilePath = HEU_PluginStorage.Instance.ConvertEnvKeyedPathToReal(inFilePath);

            // Find asset if its not at given path
            if (!HEU_Platform.DoesFileExist(inFilePath))
            {
                string fileName = HEU_Platform.GetFileNameWithoutExtension(inFilePath);
                string[] guids = AssetDatabase.FindAssets(fileName);
                if (guids.Length > 0)
                {
                    foreach (string guid in guids)
                    {
                        string newPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (newPath != null && newPath.Length > 0)
                        {
                            HEU_Logger.Log(string.Format("Note: changing asset path for {0} to {1}.", assetName, newPath));
                            return newPath;
                        }
                    }
                }

                // No valid path
                throw new HEU_HoudiniEngineError(string.Format("Houdini Asset file has moved from last location: {0}", inFilePath));
            }
#endif
            return inFilePath;
        }

        /// <summary>
        /// Load and instantiate an HDA asset in Unity and Houdini.
        /// </summary>
        /// <param name="filePath">Full path to the HDA in Unity project</param>
        /// <param name="initialPosition">Initial location to create the instance in Unity.</param>
        /// <param name="session">Houdini Engine session to load into</param>
        /// <param name="bBuildAsync">Whether to load, cook, and build asynchronously</param>
        /// <param name="bLoadFromMemory">Whether to load file into memory, then load from memory in HAPi</param>
        /// <returns></returns>
        public static GameObject InstantiateHDA(string filePath, Vector3 initialPosition, HEU_SessionBase session,
            bool bBuildAsync,
            bool bLoadFromMemory = false,
            bool bAlwaysOverwriteOnLoad = false,
            GameObject rootGO = null)
        {
            if (filePath == null || !DoesMappedPathExist(filePath))
            {
                return null;
            }

            // This will be the root GameObject for the HDA. Adding HEU_HoudiniAssetRoot
            // allows to use a custom Inspector.
            if (rootGO == null)
            {
                rootGO = HEU_GeneralUtility.CreateNewGameObject(HEU_Defines.HEU_DEFAULT_ASSET_NAME);
            }
            else
            {
                HEU_GeneralUtility.RenameGameObject(rootGO, HEU_Defines.HEU_DEFAULT_ASSET_NAME);
            }

            HEU_HoudiniAssetRoot assetRoot = rootGO.AddComponent<HEU_HoudiniAssetRoot>();

            // Under the root, we'll add the HEU_HoudiniAsset onto another GameObject
            // This will be marked as EditorOnly to strip out for builds
            GameObject hdaGEO = HEU_GeneralUtility.CreateNewGameObject(HEU_PluginSettings.HDAData_Name);
            hdaGEO.transform.parent = rootGO.transform;

            // This holds all Houdini Engine data
            HEU_HoudiniAsset asset = hdaGEO.AddComponent<HEU_HoudiniAsset>();
            // Marking as EditorOnly to be excluded from builds
            if (HEU_GeneralUtility.DoesUnityTagExist(HEU_PluginSettings.EditorOnly_Tag))
            {
                hdaGEO.tag = HEU_PluginSettings.EditorOnly_Tag;
            }

            // Bind the root to the asset
            assetRoot._houdiniAsset = asset;

            // Populate asset with what we know
            asset.SetupAsset(HEU_HoudiniAsset.HEU_AssetType.TYPE_HDA, filePath, rootGO, session);

            asset.LoadAssetFromMemory = bLoadFromMemory;
            asset.AlwaysOverwriteOnLoad = bAlwaysOverwriteOnLoad;

            // Build it in Houdini Engine
            asset.RequestReload(bBuildAsync);

            // Apply Unity transform and possibly upload to Houdini Engine
            rootGO.transform.position = initialPosition;

            //HEU_Logger.LogFormat("{0}: Created new HDA asset from {1} of type {2}.", HEU_Defines.HEU_NAME, filePath, asset.AssetType);

            return rootGO;
        }

        public static bool LoadHDAFile(HEU_SessionBase session, string assetPath, out HAPI_NodeId assetLibraryID, out string[] assetNames)
        {
            assetLibraryID = HEU_Defines.HEU_INVALID_NODE_ID;
            assetNames = new string[0];

            // Load the file
            string validAssetPath = HEU_PluginStorage.Instance.ConvertEnvKeyedPathToReal(assetPath);
            if (validAssetPath != null)
            {
                assetPath = validAssetPath;

                HAPI_AssetLibraryId libraryID = 0;
                bool bResult = session.LoadAssetLibraryFromFile(assetPath, false, out libraryID);
                if (!bResult)
                {
                    return false;
                }

                int assetCount = 0;
                bResult = session.GetAvailableAssetCount(libraryID, out assetCount);
                if (!bResult)
                {
                    return false;
                }

                Debug.AssertFormat(assetCount > 0, "Houdini Engine: Invalid Asset Count of {0}", assetCount);

                HAPI_StringHandle[] assetNameLengths = new HAPI_StringHandle[assetCount];
                bResult = session.GetAvailableAssets(libraryID, ref assetNameLengths, assetCount);
                if (!bResult)
                {
                    return false;
                }

                // Sanity check that our array hasn't changed size
                Debug.Assert(assetNameLengths.Length == assetCount, "Houdini Engine: Invalid Asset Names");

                assetNames = new string[assetCount];
                for (int i = 0; i < assetCount; ++i)
                {
                    assetNames[i] = HEU_SessionManager.GetString(assetNameLengths[i]);
                }

                return true;
            }

            return false;
        }


        public static bool CreateAndCookAssetNode(HEU_SessionBase session, string assetName, bool bCookTemplatedGeos, out HAPI_NodeId newAssetID)
        {
            newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;

            // Create top level node. Note that CreateNode will cook the node if HAPI was initialized with threaded cook setting on.
            bool bResult = session.CreateNode(-1, assetName, "", false, out newAssetID);
            if (!bResult)
            {
                return false;
            }

            // Make sure cooking is successfull before proceeding. Any licensing or file data issues will be caught here.
            if (!ProcessHoudiniCookStatus(session, assetName))
            {
                return false;
            }

            // In case the cooking wasn't done previously, force it now.
            bResult = CookNodeInHoudini(session, newAssetID, bCookTemplatedGeos, assetName);
            if (!bResult)
            {
                // When cook failed, deleted the node created earlier
                session.DeleteNode(newAssetID);
                newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
                return false;
            }

            // Get the asset ID
            HAPI_AssetInfo assetInfo = new HAPI_AssetInfo();
            bResult = session.GetAssetInfo(newAssetID, ref assetInfo);
            if (bResult)
            {
                // Check for any errors
                HAPI_ErrorCodeBits errors =
                    session.CheckForSpecificErrors(newAssetID, (HAPI_ErrorCodeBits)HAPI_ErrorCode.HAPI_ERRORCODE_ASSET_DEF_NOT_FOUND);
                if (errors > 0)
                {
                    // TODO: revisit for UI improvement
                    HEU_EditorUtility.DisplayDialog("Asset Missing Sub-asset Definitions",
                        "There are undefined nodes. This is due to not being able to find specific " +
                        "asset definitions.", "Ok");
                    return false;
                }
            }

            return true;
        }

        public static bool CreateAndCookCurveAsset(HEU_SessionBase session, string assetName, bool bCookTemplatedGeos, out HAPI_NodeId newAssetID)
        {
            newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (HEU_PluginSettings.UseLegacyInputCurves)
            {
                if (!session.CreateNode(HEU_Defines.HEU_INVALID_NODE_ID, "SOP/curve::", assetName, true, out newAssetID))
                {
                    return false;
                }

                // Make sure cooking is successfull before proceeding. Any licensing or file data issues will be caught here.
                if (!HEU_HAPIUtility.ProcessHoudiniCookStatus(session, assetName))
                {
                    return false;
                }

                // In case the cooking wasn't done previously, force it now.
                bool bResult = HEU_HAPIUtility.CookNodeInHoudini(session, newAssetID, bCookTemplatedGeos, assetName);
                if (!bResult)
                {
                    // When cook failed, delete the node created earlier
                    session.DeleteNode(newAssetID);
                    newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
                    return false;
                }
            }
            else
            {
                if (!session.CreateInputCurveNode(out newAssetID, assetName))
                {
                    // When cook failed, delete the node created earlier
                    session.DeleteNode(newAssetID);
                    newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
                    return false;
                }
            }

            return true;
        }

        public static bool CreateAndCookInputAsset(HEU_SessionBase session, string assetName, bool bCookTemplatedGeos, out HAPI_NodeId newAssetID)
        {
            newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
            if (!session.CreateInputNode(out newAssetID, assetName))
            {
                return false;
            }

            // Make sure cooking is successfull before proceeding. Any licensing or file data issues will be caught here.
            if (!HEU_HAPIUtility.ProcessHoudiniCookStatus(session, assetName))
            {
                return false;
            }

            // In case the cooking wasn't done previously, force it now.
            bool bResult = HEU_HAPIUtility.CookNodeInHoudini(session, newAssetID, bCookTemplatedGeos, assetName);
            if (!bResult)
            {
                // When cook failed, deleted the node created earlier
                session.DeleteNode(newAssetID);
                newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
                return false;
            }

            // After cooking, set an empty partinfo
            HAPI_GeoInfo inputGeoInfo = new HAPI_GeoInfo();
            if (!session.GetDisplayGeoInfo(newAssetID, ref inputGeoInfo))
            {
                return false;
            }

            HAPI_PartInfo newPart = new HAPI_PartInfo();
            newPart.init();
            newPart.id = 0;
            newPart.vertexCount = 0;
            newPart.faceCount = 0;
            newPart.pointCount = 0;
            // TODO: always set to mesh type?
            newPart.type = HAPI_PartType.HAPI_PARTTYPE_MESH;

            if (!session.SetPartInfo(inputGeoInfo.nodeId, 0, ref newPart))
            {
                HEU_Logger.LogErrorFormat(HEU_Defines.HEU_NAME + ": Failed to set partinfo for input node!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cooks node and returns true if successfull.
        /// </summary>
        /// <param name="nodeID">The node to cook</param>
        /// <param name="bCookTemplatedGeos">Whether to cook templated geos</param>
        /// <returns>True if successfully cooked node</returns>
        public static bool CookNodeInHoudini(HEU_SessionBase session, HAPI_NodeId nodeID, bool bCookTemplatedGeos, string assetName)
        {
            bool bResult = session.CookNode(nodeID, bCookTemplatedGeos);
            if (bResult)
            {
                return HEU_HAPIUtility.ProcessHoudiniCookStatus(session, assetName);
            }

            return bResult;
        }

        public static bool CookNodeInHoudiniWithOptions(HEU_SessionBase session, HAPI_NodeId nodeID, HAPI_CookOptions options, string assetName)
        {
            bool bResult = session.CookNodeWithOptions(nodeID, options);
            if (bResult)
            {
                return HEU_HAPIUtility.ProcessHoudiniCookStatus(session, assetName);
            }

            return bResult;
        }

        public static HAPI_CookOptions GetDefaultCookOptions(HEU_SessionBase session)
        {
            HAPI_CookOptions options = new HAPI_CookOptions();
            HEU_SessionHAPI sessionHAPI = (HEU_SessionHAPI)session;
#if HOUDINIENGINEUNITY_ENABLED
            sessionHAPI.GetCookOptions(ref options);
#endif
            return options;
        }


        /// <summary>
        /// Waits until cooking has finished.
        /// </summary>
        /// <returns>True if cooking was successful</returns>
        public static bool ProcessHoudiniCookStatus(HEU_SessionBase session, string assetName)
        {
            bool bResult = true;
            HAPI_State statusCode = HAPI_State.HAPI_STATE_STARTING_LOAD;

            // Busy wait until cooking has finished
            while (bResult && statusCode > HAPI_State.HAPI_STATE_MAX_READY_STATE)
            {
                bResult = session.GetCookState(out statusCode);

                if (HEU_PluginSettings.WriteCookLogs)
                {
                    string cookStatus = session.GetStatusString(HAPI_StatusType.HAPI_STATUS_COOK_STATE,
                        HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_ERRORS);
                    HEU_CookLogs.Instance.AppendCookLog(cookStatus);
                }
            }

            // Check cook results for any errors
            if (statusCode == HAPI_State.HAPI_STATE_READY_WITH_COOK_ERRORS)
            {
                // We should be able to continue even with these errors, but at least notify user.
                string statusString =
                    session.GetStatusString(HAPI_StatusType.HAPI_STATUS_COOK_RESULT, HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_WARNINGS);
                HEU_Logger.LogWarning(string.Format("Houdini Engine: Cooking finished with some warnings for asset: {0}\n{1}", assetName,
                    statusString));

                if (HEU_PluginSettings.WriteCookLogs)
                {
                    HEU_CookLogs.Instance.AppendCookLog(statusString);
                }
            }
            else if (statusCode == HAPI_State.HAPI_STATE_READY_WITH_FATAL_ERRORS)
            {
                string statusString =
                    session.GetStatusString(HAPI_StatusType.HAPI_STATUS_COOK_RESULT, HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_ERRORS);
                HEU_Logger.LogError(string.Format("Houdini Engine: Cooking failed for asset: {0}\n{1}", assetName, statusString));
                if (HEU_PluginSettings.WriteCookLogs)
                {
                    HEU_CookLogs.Instance.AppendCookLog(statusString);
                }

                return false;
            }
            else
            {
                //HEU_Logger.LogFormat("Houdini Engine: Cooking result {0} for asset: {1}", (HAPI_State)statusCode, AssetName);
            }

            return true;
        }

        public static GameObject CreateNewAsset(HEU_AssetTypeWrapper assetType, string rootName = "HoudiniAsset", Transform parentTransform = null,
            HEU_SessionBase session = null, bool bBuildAsync = true, GameObject rootGO = null)
        {
            return CreateNewAsset(HEU_HoudiniAsset.AssetType_WrapperToInternal(assetType), rootName, parentTransform, session, bBuildAsync);
        }

        internal static GameObject CreateNewAsset(HEU_HoudiniAsset.HEU_AssetType assetType, string rootName = "HoudiniAsset",
            Transform parentTransform = null, HEU_SessionBase session = null, bool bBuildAsync = true, GameObject rootGO = null)
        {
            if (session == null)
            {
                session = HEU_SessionManager.GetOrCreateDefaultSession();
            }

            if (!session.IsSessionValid())
            {
                HEU_Logger.LogWarning("Invalid Houdini Engine session!");
                return null;
            }

            // This will be the root GameObject for the HDA. Adding HEU_HoudiniAssetRoot
            // allows to use a custom Inspector.
            if (rootGO == null)
            {
                rootGO = HEU_GeneralUtility.CreateNewGameObject();
            }

            // Set the game object's name to the asset's name
            rootGO.name = string.Format("{0}{1}", rootName, rootGO.GetInstanceID());
            HEU_HoudiniAssetRoot assetRoot = rootGO.AddComponent<HEU_HoudiniAssetRoot>();


            // Under the root, we'll add the HEU_HoudiniAsset onto another GameObject
            // This will be marked as EditorOnly to strip out for builds
            GameObject hdaGEO = HEU_GeneralUtility.CreateNewGameObject(HEU_PluginSettings.HDAData_Name);
            hdaGEO.transform.parent = rootGO.transform;

            // This holds all Houdini Engine data
            HEU_HoudiniAsset asset = hdaGEO.AddComponent<HEU_HoudiniAsset>();
            // Marking as EditorOnly to be excluded from builds
            if (HEU_GeneralUtility.DoesUnityTagExist(HEU_PluginSettings.EditorOnly_Tag))
            {
                hdaGEO.tag = HEU_PluginSettings.EditorOnly_Tag;
            }

            // Bind the root to the asset
            assetRoot._houdiniAsset = asset;

            // Populate asset with what we know
            asset.SetupAsset(assetType, null, rootGO, session);

            // Build it in Houdini Engine
            asset.RequestReload(bBuildAsync);

            if (parentTransform != null)
            {
                rootGO.transform.parent = parentTransform;
                rootGO.transform.localPosition = Vector3.zero;
            }
            else
            {
                rootGO.transform.position = Vector3.zero;
            }

            return rootGO;
        }

        /// <summary>
        /// Creates a new Curve asset in scene, as well as in a Houdini session.
        /// </summary>
        /// <returns>A valid curve asset gameobject or null if failed.</returns>
        public static GameObject CreateNewCurveAsset(string name = "HoudiniCurve", Transform parentTransform = null, HEU_SessionBase session = null,
            bool bBuildAsync = true, GameObject rootGO = null)
        {
            return CreateNewAsset(HEU_HoudiniAsset.HEU_AssetType.TYPE_CURVE, name, parentTransform, session, bBuildAsync, rootGO);
        }

        /// <summary>
        /// Creates a new input asset in scene, as well as in a Houdini session.
        /// </summary>
        /// <returns>A valid input asset gameobject or null if failed.</returns>
        public static GameObject CreateNewInputAsset(string name = "HoudiniInput", Transform parentTransform = null, HEU_SessionBase session = null,
            bool bBuildAsync = true, GameObject rootGO = null)
        {
            return CreateNewAsset(HEU_HoudiniAsset.HEU_AssetType.TYPE_INPUT, name, parentTransform, session, bBuildAsync, rootGO);
        }

        /// <summary>
        /// Creates a new HEU_GeoSync in scene, opens the file select panel, and starts the file load.
        /// </summary>
        /// <param name="session">A Houdini session to use, or null if to use default or create one.</param>
        /// <returns>The new gameobject with HEU_GeoSync as a component.</returns>
        public static GameObject LoadGeoWithNewGeoSync(HEU_SessionBase session = null)
        {
#if UNITY_EDITOR
            // Note: sc is necessary on Mac because they are not allowed to have two or more dots in the extension..
            string filePattern = "bgeo,bgeo.sc,sc";
            string filePath = EditorUtility.OpenFilePanel("Select Geo File To Load", "", filePattern);

            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (session == null)
            {
                session = HEU_SessionManager.GetOrCreateDefaultSession();
            }

            if (!session.IsSessionValid())
            {
                HEU_Logger.LogWarning("Invalid Houdini Engine session!");
                return null;
            }

            // This will be the root GameObject.
            GameObject rootGO = HEU_GeneralUtility.CreateNewGameObject();
            HEU_GeoSync geoSync = rootGO.AddComponent<HEU_GeoSync>();

            // Set the game object's name to the asset's name
            HEU_GeneralUtility.RenameGameObject(rootGO, string.Format("{0}{1}", "GeoSync", rootGO.GetInstanceID()));

            geoSync._filePath = filePath;
            geoSync.StartSync();

            return rootGO;
#else
			return null;
#endif
        }

        /// <summary>
        /// Destroy children of the given transform. Does not destroy inTransform itself.
        /// </summary>
        /// <param name="inTransform">Tranform whose children are to be destroyed</param>
        public static void DestroyChildren(Transform inTransform)
        {
            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in inTransform)
            {
                children.Add(child.gameObject);
            }

            foreach (GameObject child in children)
            {
                DestroyGameObject(child);
            }
        }

        /// <summary>
        /// Destroy the given game object, including its public mesh and any shared materials.
        /// </summary>
        /// <param name="gameObect">Game object to destroy</param>
        public static void DestroyGameObject(GameObject gameObect, bool bRegisterUndo = false) // TODO: remove default bRegisterUndo arg
        {
            HEU_GeneralUtility.DestroyImmediate(gameObect, bAllowDestroyingAssets: true, bRegisterUndo: bRegisterUndo);
        }

        /// <summary>
        /// Destroy child GameObjects under the given gameObject with component T.
        /// </summary>
        /// <typeparam name="T">The component to look for on the child GameObjects</typeparam>
        /// <param name="gameObject">The GameObject's children to search through</param>
        public static void DestroyChildrenWithComponent<T>(GameObject gameObject) where T : Component
        {
            Transform trans = gameObject.transform;
            List<GameObject> children = new List<GameObject>();
            foreach (Transform t in trans)
            {
                children.Add(t.gameObject);
            }

            foreach (GameObject c in children)
            {
                if (c.GetComponent<T>() != null)
                {
                    HEU_HAPIUtility.DestroyGameObject(c);
                }
            }
        }

        /// <summary>
        /// Returns true if given node id is valid in given Houdini session.
        /// </summary>
        /// <param name="session">Session to check</param>
        /// <param name="nodeID">ID of the node to check</param>
        /// <returns>True if node is valid in given session</returns>
        public static bool IsNodeValidInHoudini(HEU_SessionBase session, HAPI_NodeId nodeID)
        {
            // Without a valid asset ID, we can't really check in Houdini session
            if (nodeID != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                // Use _assetID with uniqueHoudiniNodeId to see if our asset matches up in Houdini
                HAPI_NodeInfo nodeInfo = new HAPI_NodeInfo();
                if (session.GetNodeInfo(nodeID, ref nodeInfo, false))
                {
                    return session.IsNodeValid(nodeID, nodeInfo.uniqueHoudiniNodeId);
                }
            }

            return false;
        }

        /// <summary>
        /// Get the HDA in the current Unity scene, if it exists, with the assetID.
        /// </summary>
        /// <param name="assetID">Asset with this ID will be searched for</param>
        /// <returns>The asset in the scene with the ID, or null if not found</returns>
        public static HEU_HoudiniAssetRoot GetAssetInScene(HAPI_NodeId assetID)
        {
            HEU_HoudiniAssetRoot foundAsset = null;
            HEU_HoudiniAssetRoot[] houdiniAssets = GameObject.FindObjectsOfType<HEU_HoudiniAssetRoot>();

            foreach (HEU_HoudiniAssetRoot assetRoot in houdiniAssets)
            {
                if (assetRoot._houdiniAsset != null && assetRoot._houdiniAsset.AssetID == assetID)
                {
                    foundAsset = assetRoot;
                    break;
                }
            }

            return foundAsset;
        }

        // TRANSFORMS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Apply Houdini Engine world transform to Unity's transform object.
        /// This assumes given HAPI transform is in world space.
        /// </summary>
        /// <param name="hapiTransform">Houdini Engine transform to get data from</param>
        /// <param name="unityTransform">The Unity transform to apply data to</param>
        public static void ApplyWorldTransfromFromHoudiniToUnity(ref HAPI_Transform hapiTransform, Transform unityTransform)
        {
            // Houdini uses right-handed coordinate system, while Unity uses left-handed.
            // Note: we always use global transform space when communicating with Houdini

            // Invert the X for position
            unityTransform.position = new Vector3(-hapiTransform.position[0], hapiTransform.position[1], hapiTransform.position[2]);

            // Invert Y and Z for rotation
            Quaternion quaternion = new Quaternion(hapiTransform.rotationQuaternion[0], hapiTransform.rotationQuaternion[1],
                hapiTransform.rotationQuaternion[2], hapiTransform.rotationQuaternion[3]);
            Vector3 euler = quaternion.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;
            unityTransform.rotation = Quaternion.Euler(euler);

            // No inversion required for scale
            // We can't directly set global scale in Unity, but the proper workaround is to unparent, set scale, then reparent
            Vector3 scale = new Vector3(hapiTransform.scale[0], hapiTransform.scale[1], hapiTransform.scale[2]);
            if (unityTransform.parent != null)
            {
                Transform parent = unityTransform.parent;
                unityTransform.parent = null;
                unityTransform.localScale = scale;
                unityTransform.parent = parent;
            }
            else
            {
                unityTransform.localScale = scale;
            }
        }

        /// <summary>
        /// Apply Houdini Engine local transform to Unity's transform object.
        /// This assumes given HAPI transform is in local space.
        /// </summary>
        /// <param name="hapiTransform">Houdini Engine transform to get data from</param>
        /// <param name="unityTransform">The Unity transform to apply data to</param>
        public static void ApplyLocalTransfromFromHoudiniToUnity(ref HAPI_Transform hapiTransform, Transform unityTransform)
        {
            // Houdini uses right-handed coordinate system, while Unity uses left-handed.
            // Note: we always use global transform space when communicating with Houdini

            // Invert the X for position
            unityTransform.localPosition = new Vector3(-hapiTransform.position[0], hapiTransform.position[1], hapiTransform.position[2]);

            // Invert Y and Z for rotation
            Quaternion quaternion = new Quaternion(hapiTransform.rotationQuaternion[0], hapiTransform.rotationQuaternion[1],
                hapiTransform.rotationQuaternion[2], hapiTransform.rotationQuaternion[3]);
            Vector3 euler = quaternion.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;
            unityTransform.localRotation = Quaternion.Euler(euler);

            // No inversion required for scale
            // We can't directly set global scale in Unity, but the proper workaround is to unparent, set scale, then reparent
            Vector3 scale = new Vector3(hapiTransform.scale[0], hapiTransform.scale[1], hapiTransform.scale[2]);
            unityTransform.localScale = scale;
        }

        /// <summary>
        /// Apply Houdini Engine local transform to Unity's transform for instances, which means rotation and 
        /// scale are applied in addition (or combined with) the instance's existing transform (presumably from its source).
        /// This assumes given HAPI transform is in local space.
        /// </summary>
        /// <param name="hapiTransform">Houdini Engine transform to get data from</param>
        /// <param name="unityTransform">The Unity transform to apply data to</param>
        public static void ApplyLocalTransfromFromHoudiniToUnityForInstance(ref HAPI_Transform hapiTransform, Transform unityTransform)
        {
            // Houdini uses right-handed coordinate system, while Unity uses left-handed.
            // Note: we always use global transform space when communicating with Houdini

            // Invert the X for position
            unityTransform.localPosition = new Vector3(-hapiTransform.position[0], hapiTransform.position[1], hapiTransform.position[2]);

            // Invert Y and Z for rotation
            Quaternion quaternion = new Quaternion(hapiTransform.rotationQuaternion[0], hapiTransform.rotationQuaternion[1],
                hapiTransform.rotationQuaternion[2], hapiTransform.rotationQuaternion[3]);
            Vector3 euler = quaternion.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;
            unityTransform.localRotation = Quaternion.Euler(euler) * unityTransform.localRotation;

            // No inversion required for scale
            // We can't directly set global scale in Unity, but the proper workaround is to unparent, set scale, then reparent
            Vector3 scale = new Vector3(hapiTransform.scale[0], hapiTransform.scale[1], hapiTransform.scale[2]);
            unityTransform.localScale = Vector3.Scale(unityTransform.localScale, scale);
        }

        /// <summary>
        /// Apply matrix to transform.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="transform"></param>
        public static void ApplyMatrixToLocalTransform(ref Matrix4x4 matrix, Transform transform)
        {
            transform.localPosition = GetPosition(ref matrix);
            transform.localRotation = GetQuaternion(ref matrix);
            transform.localScale = GetScale(ref matrix);
        }

        /// <summary>
        /// Returns Unity 4x4 matrix corresponding to the given HAPI_Transform.
        /// Converts from Houdini to Unity coordinate system.
        /// </summary>
        /// <param name="hapiTransform">HAPI transform to get values from</param>
        /// <returns>Matrix4x4 in Unity coordinate system</returns>
        public static Matrix4x4 GetMatrixFromHAPITransform(ref HAPI_Transform hapiTransform, bool bConvertToUnity = true)
        {
            float invert = bConvertToUnity ? -1f : 1f;

            // TODO: Refactor this so as to use a common function to get these values
            // Invert the X for position
            Vector3 position = new Vector3(invert * hapiTransform.position[0], hapiTransform.position[1], hapiTransform.position[2]);

            // Invert Y and Z for rotation
            Quaternion quaternion = new Quaternion(hapiTransform.rotationQuaternion[0], hapiTransform.rotationQuaternion[1],
                hapiTransform.rotationQuaternion[2], hapiTransform.rotationQuaternion[3]);
            Vector3 euler = quaternion.eulerAngles;
            euler.y = invert * euler.y;
            euler.z = invert * euler.z;
            Quaternion rotation = Quaternion.Euler(euler);

            // No inversion required for scale
            // We can't directly set global scale in Unity, but the proper workaround is to unparent, set scale, then reparent
            Vector3 scale = new Vector3(hapiTransform.scale[0], hapiTransform.scale[1], hapiTransform.scale[2]);

            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(position, rotation, scale);
            return matrix;
        }

        public static Quaternion GetQuaternion(ref Matrix4x4 m)
        {
            // Check to stop warning about "Look rotation viewing vector is zero" from Quaternion.LookRotation().
            if (
                Mathf.Approximately(0.0f, m.GetColumn(2).x) &&
                Mathf.Approximately(0.0f, m.GetColumn(2).y) &&
                Mathf.Approximately(0.0f, m.GetColumn(2).z) &&
                Mathf.Approximately(0.0f, m.GetColumn(2).w) &&
                Mathf.Approximately(0.0f, m.GetColumn(1).x) &&
                Mathf.Approximately(0.0f, m.GetColumn(1).y) &&
                Mathf.Approximately(0.0f, m.GetColumn(1).z) &&
                Mathf.Approximately(0.0f, m.GetColumn(1).w))
            {
                return new Quaternion();
            }
            else
            {
                return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
            }
        }

        public static Vector3 GetPosition(ref Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        public static void SetMatrixPosition(ref Matrix4x4 m, ref Vector3 position)
        {
            m.SetColumn(3, position);
        }

        public static Vector3 GetScale(ref Matrix4x4 m)
        {
            var x = Mathf.Sqrt(m.m00 * m.m00 + m.m10 * m.m10 + m.m20 * m.m20);
            var y = Mathf.Sqrt(m.m01 * m.m01 + m.m11 * m.m11 + m.m21 * m.m21);
            var z = Mathf.Sqrt(m.m02 * m.m02 + m.m12 * m.m12 + m.m22 * m.m22);

            return new Vector3(x, y, z);
        }

        public static HAPI_TransformEuler GetHAPITransformFromMatrix(ref Matrix4x4 mat)
        {
            Quaternion q = GetQuaternion(ref mat);
            Vector3 r = q.eulerAngles;

            Vector3 p = GetPosition(ref mat);
            Vector3 s = GetScale(ref mat);

            HAPI_TransformEuler transform = new HAPI_TransformEuler(true);

            transform.position[0] = -p[0];
            transform.position[1] = p[1];
            transform.position[2] = p[2];

            transform.rotationEuler[0] = r[0];
            transform.rotationEuler[1] = -r[1];
            transform.rotationEuler[2] = -r[2];

            transform.scale[0] = s[0];
            transform.scale[1] = s[1];
            transform.scale[2] = s[2];

            transform.rotationOrder = HAPI_XYZOrder.HAPI_ZXY;
            transform.rstOrder = HAPI_RSTOrder.HAPI_SRT;

            return transform;
        }

        public static HAPI_TransformEuler GetHAPITransform(ref Vector3 p, ref Vector3 r, ref Vector3 s)
        {
            HAPI_TransformEuler transform = new HAPI_TransformEuler(true);

            transform.position[0] = -p[0];
            transform.position[1] = p[1];
            transform.position[2] = p[2];

            transform.rotationEuler[0] = r[0];
            transform.rotationEuler[1] = -r[1];
            transform.rotationEuler[2] = -r[2];

            transform.scale[0] = s[0];
            transform.scale[1] = s[1];
            transform.scale[2] = s[2];

            transform.rotationOrder = HAPI_XYZOrder.HAPI_ZXY;
            transform.rstOrder = HAPI_RSTOrder.HAPI_SRT;

            return transform;
        }

        public static HAPI_Transform GetHAPITransformQuatFromMatrix(ref Matrix4x4 mat)
        {
            Quaternion q = GetQuaternion(ref mat);
            Vector3 r = q.eulerAngles;

            Vector3 p = GetPosition(ref mat);
            Vector3 s = GetScale(ref mat);

            HAPI_Transform transform = new HAPI_Transform(true);

            transform.position[0] = -p[0];
            transform.position[1] = p[1];
            transform.position[2] = p[2];

            Quaternion rotation = mat.rotation;
            Vector3 euler = rotation.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;
            rotation = Quaternion.Euler(euler);

            transform.rotationQuaternion[0] = rotation.x;
            transform.rotationQuaternion[1] = rotation.y;
            transform.rotationQuaternion[2] = rotation.z;
            transform.rotationQuaternion[3] = rotation.w;

            transform.scale[0] = s[0];
            transform.scale[1] = s[1];
            transform.scale[2] = s[2];

            transform.rstOrder = HAPI_RSTOrder.HAPI_SRT;

            return transform;
        }

        public static Matrix4x4 GetMatrix4x4(ref Vector3 p, ref Vector3 r, ref Vector3 s)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(p, Quaternion.Euler(r.x, r.y, r.z), s);
            return matrix;
        }

        public static bool IsSameTransform(ref Matrix4x4 transformMatrix, ref Vector3 p, ref Vector3 r, ref Vector3 s)
        {
            // TODO: optimize this
            return (transformMatrix == GetMatrix4x4(ref p, ref r, ref s));
        }

        public static bool IsEqualTol(float a, float b, float t = 0.00001f)
        {
            return Mathf.Abs(a - b) <= t;
        }

        public static bool IsTransformEqual(ref HAPI_Transform transA, ref HAPI_Transform transB)
        {
            int length = transA.position.Length;
            for (int n = 0; n < length; n++)
            {
                if (!IsEqualTol(transA.position[n], transB.position[n]))
                {
                    return false;
                }
            }

            length = transA.rotationQuaternion.Length;
            for (int n = 0; n < length; n++)
            {
                if (!IsEqualTol(transA.rotationQuaternion[n], transB.rotationQuaternion[n]))
                {
                    return false;
                }
            }

            length = transA.scale.Length;
            for (int n = 0; n < length; n++)
            {
                if (!IsEqualTol(transA.scale[n], transB.scale[n]))
                {
                    return false;
                }
            }

            length = transA.shear.Length;
            for (int n = 0; n < length; n++)
            {
                if (!IsEqualTol(transA.shear[n], transB.shear[n]))
                {
                    return false;
                }
            }

            return transA.rstOrder == transB.rstOrder;
        }

        public static bool IsViewportEqual(ref HAPI_Viewport viewA, ref HAPI_Viewport viewB)
        {
            int length = viewA.position.Length;
            for (int n = 0; n < length; n++)
            {
                if (!IsEqualTol(viewA.position[n], viewB.position[n]))
                {
                    return false;
                }
            }

            length = viewA.rotationQuaternion.Length;
            for (int n = 0; n < length; n++)
            {
                if (!IsEqualTol(viewA.rotationQuaternion[n], viewB.rotationQuaternion[n]))
                {
                    return false;
                }
            }

            return viewA.offset == viewB.offset;
        }

        public static bool IsSessionSyncEqual(ref HAPI_SessionSyncInfo syncA, ref HAPI_SessionSyncInfo syncB)
        {
            return syncA.cookUsingHoudiniTime == syncB.cookUsingHoudiniTime
                   && syncA.syncViewport == syncB.syncViewport;
        }

        public static bool DoesGeoPartHaveAttribute(HEU_SessionBase session, HAPI_NodeId geoID, HAPI_PartId partID, string attrName,
            HAPI_AttributeOwner owner, ref HAPI_AttributeInfo attributeInfo)
        {
            if (session.GetAttributeInfo(geoID, partID, attrName, owner, ref attributeInfo))
            {
                return attributeInfo.exists;
                //HEU_Logger.LogFormat("Attr {0} exists={1}, with count={2}, type={3}, storage={4}, tuple={5}", "Cd", colorAttrInfo.exists, colorAttrInfo.count, colorAttrInfo.typeInfo, colorAttrInfo.storage, colorAttrInfo.tupleSize);
            }

            return false;
        }

#if UNITY_EDITOR
        public static int TangentModeToHoudiniRampInterpolation(AnimationUtility.TangentMode tangentMode)
        {
            if (tangentMode == AnimationUtility.TangentMode.Constant)
            {
                return 0;
            }
            else if (tangentMode == AnimationUtility.TangentMode.Linear)
            {
                return 1;
            }

            // Use Catmull-Rom for all smooth interpolation
            return 2;
        }

        public static AnimationUtility.TangentMode HoudiniRampInterpolationToTangentMode(int interpolation)
        {
            // interpolation == 0 -> Constant		=> TangentMode.Constant
            // interpolation == 1 -> Linear			=> TangentMode.Linear
            // interpolation == 2 -> Catmull-Rom	=> TangentMode.Free

            if (interpolation == 0)
            {
                return AnimationUtility.TangentMode.Constant;
            }
            else if (interpolation == 1)
            {
                return AnimationUtility.TangentMode.Linear;
            }

            // Use Free for all smooth interpolation
            return AnimationUtility.TangentMode.Free;
        }

        public static int GradientModeToHoudiniColorRampInterpolation(GradientMode gradientMode)
        {
            return (gradientMode == GradientMode.Blend) ? 1 : 0;
        }
#endif

        public static void SetAnimationCurveTangentModes(AnimationCurve animCurve, List<int> tangentValues)
        {
#if UNITY_EDITOR
            try
            {
                AnimationUtility.TangentMode leftTangent = AnimationUtility.TangentMode.Free;
                AnimationUtility.TangentMode rightTangent = AnimationUtility.TangentMode.Free;
                for (int i = 0; i < tangentValues.Count; ++i)
                {
                    if (i > 0)
                    {
                        leftTangent = rightTangent;
                    }

                    rightTangent = HEU_HAPIUtility.HoudiniRampInterpolationToTangentMode(tangentValues[i]);

                    AnimationUtility.SetKeyLeftTangentMode(animCurve, i, leftTangent);
                    AnimationUtility.SetKeyRightTangentMode(animCurve, i, rightTangent);
                }
            }
            catch (System.Exception ex)
            {
                // Setting above key tangent modes can throw error which aborts the entire UI
                // drawing. Instead just print the error and let UI drawing continue.
                HEU_Logger.LogError(ex);
            }
#endif
        }

        /// <summary>
        /// Returns true if this plugin supports the given partType.
        /// Support means can convert one of Unity's native geometry.
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        public static bool IsSupportedPolygonType(HAPI_PartType partType)
        {
            return partType == HAPI_PartType.HAPI_PARTTYPE_MESH || partType == HAPI_PartType.HAPI_PARTTYPE_BOX ||
                   partType == HAPI_PartType.HAPI_PARTTYPE_SPHERE;
        }

        /// <summary>
        /// Returns the parent node's ID of the given node.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public static HAPI_NodeId GetParentNodeID(HEU_SessionBase session, HAPI_NodeId nodeID)
        {
            HAPI_NodeId parentNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            if (nodeID != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                HAPI_NodeInfo nodeInfo = new HAPI_NodeInfo();
                if (session.GetNodeInfo(nodeID, ref nodeInfo))
                {
                    parentNodeID = nodeInfo.parentId;
                }
            }

            return parentNodeID;
        }

        /// <summary>
        /// Gets the object infos and transforms for given asset.
        /// </summary>
        /// <param name="assetID">ID of the asset</param>
        /// <param name="nodeInfo">HAPI_NodeInfo of the asset</param>
        /// <param name="objectInfos">Array of retrieved object infos</param>
        /// <param name="objectTransforms">Array of retrieved object transforms</param>
        /// <returns>True if succesfully retrieved object infos and transforms</returns>
        public static bool GetObjectInfos(HEU_SessionBase session, HAPI_NodeId assetID, ref HAPI_NodeInfo nodeInfo, out HAPI_ObjectInfo[] objectInfos,
            out HAPI_Transform[] objectTransforms)
        {
            objectInfos = new HAPI_ObjectInfo[0];
            objectTransforms = new HAPI_Transform[0];

            if (nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP)
            {
                // For SOP assets, we use the parent IDs to get the object info and geo info

                objectInfos = new HAPI_ObjectInfo[1];
                if (!session.GetObjectInfo(nodeInfo.parentId, ref objectInfos[0]))
                {
                    return false;
                }

                // Identity transform will be used for SOP assets, so not querying transform
                objectTransforms = new HAPI_Transform[1];
                objectTransforms[0] = new HAPI_Transform(true);
            }
            else if (nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ)
            {
                int objectCount = 0;
                if (!session.ComposeObjectList(assetID, out objectCount))
                {
                    return false;
                }

                if (objectCount <= 0)
                {
                    // Since this asset is an object type and has 0 object as children, we use the object itself

                    objectInfos = new HAPI_ObjectInfo[1];
                    if (!session.GetObjectInfo(assetID, ref objectInfos[0]))
                    {
                        return false;
                    }

                    // Identity transform will be used for single object assets, so not querying transform
                    objectTransforms = new HAPI_Transform[1];
                    objectTransforms[0] = new HAPI_Transform(true);
                }
                else
                {
                    // This object has children, so use GetComposedObjectList to get list of HAPI_ObjectInfos

                    int immediateSOP = 0;
                    session.ComposeChildNodeList(nodeInfo.id, (int)HAPI_NodeType.HAPI_NODETYPE_SOP, (int)HAPI_NodeFlags.HAPI_NODEFLAGS_DISPLAY, false,
                        ref immediateSOP);
                    bool addSelf = immediateSOP > 0;

                    if (!session.ComposeObjectList(assetID, out objectCount))
                    {
                        return false;
                    }

                    if (!addSelf)
                    {
                        objectInfos = new HAPI_ObjectInfo[objectCount];
                        objectTransforms = new HAPI_Transform[objectCount];
                    }
                    else
                    {
                        objectInfos = new HAPI_ObjectInfo[objectCount + 1];
                        objectTransforms = new HAPI_Transform[objectCount + 1];
                    }

                    if (addSelf && !session.GetObjectInfo(nodeInfo.id, ref objectInfos[objectCount]))
                    {
                        return false;
                    }


                    if (!HEU_SessionManager.GetComposedObjectListMemorySafe(session, nodeInfo.parentId, objectInfos, 0, objectCount))
                    {
                        return false;
                    }

                    // Now get the object transforms
                    if (!HEU_SessionManager.GetComposedObjectTransformsMemorySafe(session, nodeInfo.parentId, HAPI_RSTOrder.HAPI_SRT,
                            objectTransforms, 0, objectCount))
                    {
                        return false;
                    }

                    if (addSelf)
                    {
                        objectTransforms[objectCount] = new HAPI_Transform(true);
                    }
                }
            }
            else
            {
                HEU_Logger.LogWarningFormat(HEU_Defines.HEU_NAME + ": Unsupported node type {0}", nodeInfo.type);
                return false;
            }

            return true;
        }

        public static bool ContainsSopNodes(HEU_SessionBase session, HAPI_NodeId nodeId)
        {
            int childCount = 0;
            if (!session.ComposeChildNodeList(nodeId, (int)HAPI_NodeType.HAPI_NODETYPE_SOP, (int)HAPI_NodeFlags.HAPI_NODEFLAGS_ANY, false,
                    ref childCount)) return false;

            return childCount > 0;
        }

        public static bool IsObjNodeFullyVisible(HEU_SessionBase session, HashSet<HAPI_NodeId> allObjectIds, HAPI_NodeId inRootNodeId,
            HAPI_NodeId inChildNodeId)
        {
            // Walk up the hierarchy from child to r oot
            // If any node in that hierarchy is not in the "AllObjectIds" set, the OBJ node is considered to
            // be hidden.

            if (inChildNodeId == inRootNodeId) return true;

            HAPI_NodeId childNodeId = inChildNodeId;

            HAPI_ObjectInfo childObjInfo = new HAPI_ObjectInfo();
            HAPI_NodeInfo childNodeInfo = new HAPI_NodeInfo();

            do
            {
                if (!session.GetObjectInfo(childNodeId, ref childObjInfo)) return false;
                if (!childObjInfo.isVisible || childObjInfo.nodeId < 0) return false;

                if (childNodeId != inChildNodeId)
                {
                    // Only perform this check for 'parents' of the incoming child node
                    if (!allObjectIds.Contains(childNodeId))
                    {
                        // There is a non-object node in the hierarchy between the child and asset root, rendering the
                        // child object node invisible.
                        return false;
                    }
                }

                if (!session.GetNodeInfo(childNodeId, ref childNodeInfo)) return false;

                childNodeId = childNodeInfo.parentId;
            } while (childNodeId != inRootNodeId && childNodeId >= 0);

            return true;
        }

        public static bool GetOutputIndex(HEU_SessionBase session, HAPI_NodeId nodeId, ref int outputIndex)
        {
            int tempValue = -1;

            if (session.GetParamIntValue(nodeId, "outputidx", 0, out tempValue))
            {
                outputIndex = tempValue;
                return true;
            }

            return false;
        }

        static internal void GatherAllAssetGeoInfos(HEU_SessionBase session, HAPI_AssetInfo assetInfo, HAPI_ObjectInfo objectInfo,
            bool bUseOutputNodes, ref List<HAPI_GeoInfo> outGeoInfos)
        {
            if (outGeoInfos == null) outGeoInfos = new List<HAPI_GeoInfo>();

            if (objectInfo.nodeId < 0)
            {
                return;
            }

            if (assetInfo.nodeId < 0)
            {
                return;
            }

            bool bOutputTemplatedGeos = false; // TODO: Add this option in HoudiniAssetComponent

            // Get the Asset NodeInfo
            HAPI_NodeInfo assetNodeInfo = new HAPI_NodeInfo();
            if (!session.GetNodeInfo(assetInfo.nodeId, ref assetNodeInfo)) return;

            // In certain cases, such as PDG output processing we might end up with a SOP node instead of a
            // container. In that case, don't try to run child queries on this node. They will fail.
            bool bAssetHasChildren = !(assetNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP && assetNodeInfo.childNodeCount == 0);

            List<HAPI_GeoInfo> editableGeoInfos = new List<HAPI_GeoInfo>();

            // Get editable nodes, cook em, then create geo nodes for them
            HAPI_NodeId[] editableNodes = null;
            HEU_SessionManager.GetComposedChildNodeList(session, assetInfo.nodeId, (int)HAPI_NodeType.HAPI_NODETYPE_SOP,
                (int)HAPI_NodeFlags.HAPI_NODEFLAGS_EDITABLE, true, out editableNodes, false);
            if (editableNodes != null)
            {
                foreach (HAPI_NodeId editNodeID in editableNodes)
                {
                    HAPI_GeoInfo editGeoInfo = new HAPI_GeoInfo();
                    if (session.GetGeoInfo(editNodeID, ref editGeoInfo))
                    {
                        // Do not process the main display geo twice!
                        if (editGeoInfo.isDisplayGeo) continue;

                        session.CookNode(editNodeID, HEU_PluginSettings.CookTemplatedGeos);

                        // Get updated geo info after cooking
                        if (session.GetGeoInfo(editNodeID, ref editGeoInfo))
                        {
                            // Add this geo to the geo info array
                            editableGeoInfos.Add(editGeoInfo);
                        }
                    }
                }
            }

            bool bIsSopAsset = assetInfo.nodeId != assetInfo.objectNodeId;
            bool bUseOutputFromSubnets = true;

            if (bAssetHasChildren)
            {
                if (HEU_HAPIUtility.ContainsSopNodes(session, assetInfo.nodeId))
                {
                    // This HDA contains immediate SOP nodes. Don't look for subnets to output.
                    bUseOutputFromSubnets = false;
                }
                else
                {
                    // Assume we're using a subnet-based HDA
                    bUseOutputFromSubnets = true;
                }
            }
            else
            {
                // This asset doesn't have any children. Don'y try to find subnets
                bUseOutputFromSubnets = false;
            }

            // Need to get all object Ids just to determine visiblity
            HashSet<HAPI_NodeId> allObjectIds = new HashSet<HAPI_NodeId>();

            if (bUseOutputFromSubnets)
            {
                HAPI_NodeId[] objectIds = null;
                HEU_SessionManager.GetComposedChildNodeList(session, assetInfo.nodeId, (int)HAPI_NodeType.HAPI_NODETYPE_OBJ,
                    (int)HAPI_NodeFlags.HAPI_NODEFLAGS_OBJ_SUBNET, true, out objectIds);

                foreach (HAPI_NodeId objId in objectIds)
                {
                    allObjectIds.Add(objId);
                }
            }
            else
            {
                allObjectIds.Add(assetInfo.objectNodeId);
            }

            // Check visibility
            bool bObjectIsVisible = false;
            HAPI_NodeId gatherOutputsNodeId = -1;
            if (!bAssetHasChildren)
            {
                bObjectIsVisible = true;
                gatherOutputsNodeId = assetNodeInfo.parentId;
            }
            else if (bIsSopAsset && objectInfo.nodeId == assetInfo.objectNodeId)
            {
                bObjectIsVisible = true;
                gatherOutputsNodeId = assetInfo.nodeId;
            }
            else
            {
                bObjectIsVisible = HEU_HAPIUtility.IsObjNodeFullyVisible(session, allObjectIds, assetInfo.nodeId, objectInfo.nodeId);
                gatherOutputsNodeId = objectInfo.nodeId;
            }

            // Actually gather the geoInfo
            outGeoInfos.AddRange(editableGeoInfos);
            if (bObjectIsVisible)
            {
                GatherAllAssetOutputs(session, gatherOutputsNodeId, bUseOutputNodes, bOutputTemplatedGeos, ref outGeoInfos);
            }
        }

        // This is the version we use in Unreal
        static internal void GatherAllObjectGeoInfos(HEU_SessionBase session, HAPI_NodeId assetId, bool bUseOutputNodes,
            ref List<HAPI_GeoInfo> outGeoInfos)
        {
            if (assetId < 0)
            {
                return;
            }

            bool bOutputTemplatedGeos = false; // TODO: Add this option in HoudiniAssetComponent


            // Get the AssetInfo
            HAPI_AssetInfo assetInfo = new HAPI_AssetInfo();
            if (!session.GetAssetInfo(assetId, ref assetInfo)) return;

            // Get the Asset NodeInfo
            HAPI_NodeInfo assetNodeInfo = new HAPI_NodeInfo();
            if (!session.GetNodeInfo(assetId, ref assetNodeInfo)) return;

            // In certain cases, such as PDG output processing we might end up with a SOP node instead of a
            // container. In that case, don't try to run child queries on this node. They will fail.
            bool bAssetHasChildren = !(assetNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP && assetNodeInfo.childNodeCount == 0);

            HAPI_ObjectInfo[] objectInfos;
            HAPI_Transform[] objectTransforms;

            if (!HEU_HAPIUtility.GetObjectInfos(session, assetId, ref assetNodeInfo, out objectInfos, out objectTransforms)) return;

            List<HAPI_GeoInfo> editableGeoInfos = new List<HAPI_GeoInfo>();

            // Get editable nodes, cook em, then create geo nodes for them
            HAPI_NodeId[] editableNodes = null;
            HEU_SessionManager.GetComposedChildNodeList(session, assetId, (int)HAPI_NodeType.HAPI_NODETYPE_SOP,
                (int)HAPI_NodeFlags.HAPI_NODEFLAGS_EDITABLE, true, out editableNodes);
            if (editableNodes != null)
            {
                foreach (HAPI_NodeId editNodeID in editableNodes)
                {
                    HAPI_GeoInfo editGeoInfo = new HAPI_GeoInfo();
                    if (session.GetGeoInfo(editNodeID, ref editGeoInfo))
                    {
                        // Do not process the main display geo twice!
                        if (editGeoInfo.isDisplayGeo) continue;

                        // We only handle editable curves for now
                        if (editGeoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_CURVE) continue;

                        //session.CookNode(editNodeID, HEU_PluginSettings.CookTemplatedGeos);

                        // Add this geo to the geo info array
                        editableGeoInfos.Add(editGeoInfo);
                    }
                }
            }

            bool bIsSopAsset = assetInfo.nodeId != assetInfo.objectNodeId;
            bool bUseOutputFromSubnets = true;

            if (bAssetHasChildren)
            {
                if (HEU_HAPIUtility.ContainsSopNodes(session, assetInfo.nodeId))
                {
                    // This HDA contains immediate SOP nodes. Don't look for subnets to output.
                    bUseOutputFromSubnets = false;
                }
                else
                {
                    // Assume we're using a subnet-based HDA
                    bUseOutputFromSubnets = true;
                }
            }
            else
            {
                // This asset doesn't have any children. Don'y try to find subnets
                bUseOutputFromSubnets = false;
            }

            // Before we can perform visibility checks on the Object nodes, we have
            // to build a set of all the Object node ids. The 'AllObjectIds' act
            // as a visibility filter. If an Object node is not present in this
            // list, the content of that node will not be displayed (display / output / templated nodes).
            // NOTE that if the HDA contains immediate SOP nodes we will ignore
            // all subnets and only use the data outputs directly from the HDA.

            HashSet<HAPI_NodeId> allObjectIds = new HashSet<HAPI_NodeId>();

            if (bUseOutputFromSubnets)
            {
                HAPI_NodeId[] objectIds = null;
                HEU_SessionManager.GetComposedChildNodeList(session, assetId, (int)HAPI_NodeType.HAPI_NODETYPE_OBJ,
                    (int)HAPI_NodeFlags.HAPI_NODEFLAGS_OBJ_SUBNET, true, out objectIds);

                foreach (HAPI_NodeId objId in objectIds)
                {
                    allObjectIds.Add(objId);
                }
            }
            else
            {
                allObjectIds.Add(assetInfo.objectNodeId);
            }

            for (int objectIdx = 0; objectIdx < objectInfos.Length; objectIdx++)
            {
                HAPI_ObjectInfo currentObjInfo = objectInfos[objectIdx];
                bool bObjectIsVisible = false;
                HAPI_NodeId gatherOutputsNodeId = -1;
                if (!bAssetHasChildren)
                {
                    bObjectIsVisible = true;
                    gatherOutputsNodeId = assetNodeInfo.parentId;
                }
                else if (bIsSopAsset && currentObjInfo.nodeId == assetInfo.objectNodeId)
                {
                    bObjectIsVisible = true;
                    gatherOutputsNodeId = assetInfo.nodeId;
                }
                else
                {
                    bObjectIsVisible = HEU_HAPIUtility.IsObjNodeFullyVisible(session, allObjectIds, assetId, currentObjInfo.nodeId);
                    gatherOutputsNodeId = currentObjInfo.nodeId;
                }

                List<HAPI_GeoInfo> geoInfos = new List<HAPI_GeoInfo>(editableGeoInfos);
                if (bObjectIsVisible)
                {
                    GatherAllAssetOutputs(session, gatherOutputsNodeId, bUseOutputNodes, bOutputTemplatedGeos, ref outGeoInfos);
                }
            }
        }

        static private void GatherAllAssetOutputs(HEU_SessionBase session, HAPI_NodeId nodeId, bool bUseOutputNodes, bool bOutputTemplatedGeos,
            ref List<HAPI_GeoInfo> outGeoInfos)
        {
            HashSet<HAPI_NodeId> gatheredNodeIds = new HashSet<HAPI_NodeId>();
            // NOTE: This function assumes that the incoming node is a Geometry container that contains immediate
            // outputs / display nodes / template nodes.

            // First we look for (immediate) output nodes (if bUseOutputNodes have been enabled).
            // If we didn't find an output node, we'll look for a display node.

            List<HAPI_GeoInfo> geoInfos = new List<HAPI_GeoInfo>();
            bool bHasOutputs = false;

            if (bUseOutputNodes)
            {
                int numOutputs = -1;
                session.GetOutputGeoCount(nodeId, out numOutputs);
                if (numOutputs > 0)
                {
                    bHasOutputs = true;
                    HAPI_GeoInfo[] outputGeoInfos = new HAPI_GeoInfo[numOutputs];
                    if (session.GetOutputGeoInfos(nodeId, ref outputGeoInfos, numOutputs))
                    {
                        foreach (HAPI_GeoInfo outputGeoInfo in outputGeoInfos)
                        {
                            // Gather all output nodes, regardless of index.
                            //    int outputIndex = -1;

                            //    if (HEU_HAPIUtility.GetOutputIndex(session, outputGeoInfo.nodeId, ref outputIndex) && outputIndex == 0)
                            //    {
                            if (!gatheredNodeIds.Contains(outputGeoInfo.nodeId))
                            {
                                geoInfos.Add(outputGeoInfo);
                                gatheredNodeIds.Add(outputGeoInfo.nodeId);
                            }

                            //    }
                        }
                    }
                }
            }

            if (!bHasOutputs)
            {
                HAPI_NodeId[] displayNodeIds = null;
                if (HEU_SessionManager.GetComposedChildNodeList(session, nodeId, (int)HAPI_NodeType.HAPI_NODETYPE_SOP,
                        (int)HAPI_NodeFlags.HAPI_NODEFLAGS_DISPLAY, false, out displayNodeIds))
                {
                    foreach (HAPI_NodeId displayNodeId in displayNodeIds)
                    {
                        if (gatheredNodeIds.Contains(displayNodeId)) continue;

                        HAPI_GeoInfo geoInfo = new HAPI_GeoInfo();
                        if (session.GetGeoInfo(displayNodeId, ref geoInfo))
                        {
                            geoInfos.Add(geoInfo);
                            gatheredNodeIds.Add(geoInfo.nodeId);
                        }
                    }
                }
            }

            if (bOutputTemplatedGeos)
            {
                HAPI_NodeId[] templatedNodeIds = null;
                if (HEU_SessionManager.GetComposedChildNodeList(session, nodeId, (int)HAPI_NodeType.HAPI_NODETYPE_SOP,
                        (int)HAPI_NodeFlags.HAPI_NODEFLAGS_TEMPLATED, false, out templatedNodeIds))
                {
                    foreach (HAPI_NodeId templateNodeId in templatedNodeIds)
                    {
                        if (gatheredNodeIds.Contains(templateNodeId)) continue;

                        HAPI_GeoInfo geoInfo = new HAPI_GeoInfo();
                        if (session.GetGeoInfo(templateNodeId, ref geoInfo) && geoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_INVALID)
                        {
                            geoInfos.Add(geoInfo);
                            gatheredNodeIds.Add(geoInfo.nodeId);
                        }
                    }
                }
            }

            foreach (HAPI_GeoInfo info in geoInfos)
            {
                bool requiresCook = info.hasGeoChanged;

                if (info.partCount <= 0) requiresCook = true;

                // Not sure if this is necessary. TODO: Remove if not needed
                //if (!requiresCook)
                //{
                //    // Recook assets with invalid parts
                //    int numParts = info.partCount;
                //    for (int i = 0; i < numParts; ++i)
                //    {
                //        HAPI_PartInfo partInfo = new HAPI_PartInfo();
                //        if (!session.GetPartInfo(info.nodeId, i, ref partInfo))
                //        {
                //            continue;
                //        }
                //	if (partInfo.id < 0 || partInfo.type == HAPI_PartType.HAPI_PARTTYPE_INVALID)
                //	{
                //	    requiresCook = true;
                //	    break;
                //	}
                //    }
                //}

                if (requiresCook)
                {
                    session.CookNode(info.nodeId, HEU_PluginSettings.CookTemplatedGeos);
                    HAPI_GeoInfo geoInfo = new HAPI_GeoInfo();
                    if (session.GetGeoInfo(info.nodeId, ref geoInfo) && geoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_INVALID)
                    {
                        outGeoInfos.Add(geoInfo);
                    }
                }
                else
                {
                    outGeoInfos.Add(info);
                }
            }
        }

        // Replaces invalid characters with _ to make a hapi variable name
        public static string ToHapiVariableName(string name)
        {
            char[] charArray = name.ToCharArray();

            for (int i = 0; i < charArray.Length; i++)
            {
                if (charArray[i] == ';' ||
                    charArray[i] == ';' ||
                    charArray[i] == '<' ||
                    charArray[i] == '>' ||
                    charArray[i] == '?' ||
                    charArray[i] == '|' ||
                    charArray[i] == ' ')
                {
                    charArray[i] = '_';
                }
            }

            return new string(charArray);
        }

        // Conversion helpers
        // Scale is same as Houdini
        public static void ConvertPositionUnityToHoudini(ref Vector3 position)
        {
            position.x = -position.x;
        }

        public static void ConvertPositionUnityToHoudini(Vector3 position, out float outputX, out float outputY, out float outputZ)
        {
            outputX = -position.x;
            outputY = position.y;
            outputZ = position.z;
        }

        public static Vector3 ConvertPositionUnityToHoudini(float inputX, float inputY, float inputZ)
        {
            return new Vector3(-inputX, inputY, inputZ);
        }

        public static Vector3 ConvertPositionUnityToHoudini(Vector3 inputVec)
        {
            inputVec.x = -inputVec.x;
            return inputVec;
        }

        public static void ConvertPositionUnityToHoudini(float inputX, float inputY, float inputZ, ref Vector3 outputVec)
        {
            outputVec.x = -inputX;
            outputVec.y = inputY;
            outputVec.z = inputZ;
        }

        public static void ConvertRotationUnityToHoudini(ref Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;
            rotation = Quaternion.Euler(euler);
        }

        public static void ConvertRotationUnityToHoudini(Quaternion rotation, out float outputX, out float outputY, out float outputZ,
            out float outputW)
        {
            Vector3 euler = rotation.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;
            rotation = Quaternion.Euler(euler);
            outputX = rotation[0];
            outputY = rotation[1];
            outputZ = rotation[2];
            outputW = rotation[3];
        }

        public static Quaternion ConvertRotationUnityToHoudini(float inputX, float inputY, float inputZ, float inputW)
        {
            Quaternion quat = new Quaternion(inputX, inputY, inputZ, inputW);
            Vector3 euler = quat.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;

            return Quaternion.Euler(euler);
        }

        public static Quaternion ConvertRotationUnityToHoudini(Quaternion inputQuat)
        {
            Vector3 euler = inputQuat.eulerAngles;
            euler.y = -euler.y;
            euler.z = -euler.z;
            return Quaternion.Euler(euler);
        }

        public static void ConvertScaleUnityToHoudini(ref Vector3 position)
        {
            // Scale is the same!
        }

        public static void ConvertScaleUnityToHoudini(Vector3 position, out float outputX, out float outputY, out float outputZ)
        {
            // Scale is the same!
            outputX = position.x;
            outputY = position.y;
            outputZ = position.z;
        }

        public static Vector3 ConvertScaleUnityToHoudini(float inputX, float inputY, float inputZ)
        {
            return new Vector3(inputX, inputY, inputZ);
        }

        public static Vector3 ConvertScaleUnityToHoudini(Vector3 inputVec)
        {
            return inputVec;
        }
    }
} // HoudiniEngineUnity