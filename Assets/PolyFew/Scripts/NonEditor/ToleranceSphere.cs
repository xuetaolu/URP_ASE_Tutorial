using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrainFailProductions.PolyFew
{

    [System.Serializable]
    public class ToleranceSphere : ScriptableObject
    {

        public Vector3 worldPosition;
        public float diameter;
        public Color color;
        public float preservationStrength;
        public bool isHidden;

        public ToleranceSphere(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
        {
            this.worldPosition = worldPosition;
            this.diameter = diameter;
            this.color = color;
            this.preservationStrength = preservationStrength;
            this.isHidden = isHidden;
        }


        public void SetProperties(ToleranceSphereJson tSphereJson)
        {
            worldPosition = tSphereJson.worldPosition;
            diameter = tSphereJson.diameter;
            color = tSphereJson.color;
            preservationStrength = tSphereJson.preservationStrength;
            isHidden = tSphereJson.isHidden;
        }


        public void SetProperties(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
        {
            this.worldPosition = worldPosition;
            this.diameter = diameter;
            this.color = color;
            this.preservationStrength = preservationStrength;
            this.isHidden = isHidden;
        }
    }


    [System.Serializable]
    public class ToleranceSphereJson
    {

        public Vector3 worldPosition;
        public float diameter;
        public Color color;
        public float preservationStrength;
        public bool isHidden;

        public ToleranceSphereJson(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
        {
            this.worldPosition = worldPosition;
            this.diameter = diameter;
            this.color = color;
            this.preservationStrength = preservationStrength;
            this.isHidden = isHidden;
        }

        public ToleranceSphereJson(ToleranceSphere toleranceSphere)
        {
            if (toleranceSphere == null) { return; }

            DumpFromToleranceSphere(toleranceSphere);
        }


        public void SetProperties(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
        {
            this.worldPosition = worldPosition;
            this.diameter = diameter;
            this.color = color;
            this.preservationStrength = preservationStrength;
            this.isHidden = isHidden;
        }


        public void DumpFromToleranceSphere(ToleranceSphere toleranceSphere)
        {
            if (toleranceSphere == null) { return; }

            worldPosition = toleranceSphere.worldPosition;
            diameter = toleranceSphere.diameter;
            color = toleranceSphere.color;
            preservationStrength = toleranceSphere.preservationStrength;
            isHidden = toleranceSphere.isHidden;
        }


        public void DumpToToleranceSphere(ref ToleranceSphere toleranceSphere)
        {
            if (toleranceSphere == null) { return; }

            toleranceSphere.worldPosition = worldPosition;
            toleranceSphere.diameter = diameter;
            toleranceSphere.color = color;
            toleranceSphere.preservationStrength = preservationStrength;
            toleranceSphere.isHidden = isHidden;
        }
    }

}

