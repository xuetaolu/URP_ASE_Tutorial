using UnityEngine;


namespace UnityMeshSimplifier
{

    [System.Serializable]
    public class ToleranceSphere
    {

        public Vector3 worldPosition;
        public Matrix4x4 localToWorldMatrix;
        public float diameter;
        public GameObject targetObject;
        public int initialEnclosedTrianglesCount { private set; get; }
        public int currentEnclosedTrianglesCount;
        public int leastTrianglesCount { private set; get; }
        public float preservationStrength = 100;

        public void SetInitialEnclosedTrianglesCount(int initialCount)
        {
            initialEnclosedTrianglesCount = initialCount;
            currentEnclosedTrianglesCount = initialCount;

            leastTrianglesCount = Mathf.RoundToInt((preservationStrength / 100f) * initialCount);
        }


        public ToleranceSphere()
        {
        }

        public ToleranceSphere(Vector3 worldPosition, Matrix4x4 localToWorldMatrix, float diameter, GameObject targetObject, float preservationStrength)
        {
            this.worldPosition = worldPosition;
            this.localToWorldMatrix = localToWorldMatrix;
            this.diameter = diameter;
            this.targetObject = targetObject;
            this.preservationStrength = preservationStrength;
        }
    }

}
