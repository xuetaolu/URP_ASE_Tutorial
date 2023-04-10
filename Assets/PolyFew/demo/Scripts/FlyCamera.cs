using UnityEngine;


namespace BrainFailProductions.PolyFewRuntime
{

    public class FlyCamera : MonoBehaviour
    {


        public Transform target;
        public float distance = 5.0f;
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;
        public float panSpeed = 0.05f;

        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;

        public float distanceMin = .5f;
        public float distanceMax = 15f;

#pragma warning disable
        private Rigidbody rigidbody;

        float x = 0.0f;
        float y = 0.0f;


        public static bool deactivated = false;

        // Use this for initialization
        void Start()
        {

            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;

            rigidbody = GetComponent<Rigidbody>();

            // Make the rigid body not change rotation
            if (rigidbody != null)
            {
                rigidbody.freezeRotation = true;
            }

        }

        void Update()
        {

            if (deactivated) { return; }

            if (target)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                if (!Input.GetMouseButton(0))
                {
                    mouseX = 0;
                    mouseY = 0;
                }

                x += mouseX * xSpeed * distance * 0.02f;
                y -= mouseY * ySpeed * 0.02f;



                y = ClampAngle(y, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

                RaycastHit hit;

                if (Physics.Linecast(target.position, transform.position, out hit))
                {
                    distance -= hit.distance;
                }

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + target.position;


                transform.position = position;
                transform.rotation = rotation;

            }
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }

    }

}
