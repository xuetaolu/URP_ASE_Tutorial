#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BrainFailProductions.PolyFew
{
    public class PolyFewResetter 
    {
        public static void ResetToInitialState()
        {
            //PolyFew Doesn't show up for multiple objects so don't do anything
            if(UtilityServices.dataContainer == null) { return; }
            //if (Selection.gameObjects != null && Selection.gameObjects.Length > 1) { return; }
            //if (Selection.activeGameObject == null) { return; }

            UtilityServices.RestoreMeshesFromPairs(UtilityServices.dataContainer.objectMeshPairs);
            UtilityServices.dataContainer.reductionPending = false;
            UtilityServices.dataContainer.reductionStrength = 0;

            if(Selection.activeGameObject != null)
            {
                UtilityServices.dataContainer.triangleCount = UtilityServices.CountTriangles(UtilityServices.dataContainer.considerChildren, UtilityServices.dataContainer.objectMeshPairs, Selection.activeGameObject);
            }
        }


        public static void RefreshObjectMeshPairs(GameObject forObject)
        {
            //PolyFew Doesn't show up for multiple objects so don't do anything
            if (UtilityServices.dataContainer == null) { return; }
            //if (Selection.gameObjects != null && Selection.gameObjects.Length > 1) { return; }
            //forObject = Selection.activeGameObject;

            if (forObject == null) { return; }

            UtilityServices.dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(forObject, true, true);
            UtilityServices.dataContainer.triangleCount = UtilityServices.CountTriangles(UtilityServices.dataContainer.considerChildren, UtilityServices.dataContainer.objectMeshPairs, forObject);

            bool found = UtilityServices.dataContainer.objectsHistory != null;

            // Delete the Undo Redo history as they contain older meshes and have become invalid
            if (found) { UtilityServices.dataContainer.objectsHistory.Destruct(); }
        }

    }
}

#endif