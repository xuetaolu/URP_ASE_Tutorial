using UnityEngine;

namespace BrainFailProductions.PolyFew
{
    [ExecuteInEditMode]
    public class PolyFew : MonoBehaviour
    {
#if UNITY_EDITOR

        [HideInInspector]
#if UNITY_2019_3_OR_NEWER
        [SerializeReference]
#else
        [SerializeField]
#endif
        public DataContainer dataContainer;

        private void OnEnable()
        {
            if (dataContainer == null)
            {
                dataContainer = new DataContainer();
            }
        }

#endif
    }
}
