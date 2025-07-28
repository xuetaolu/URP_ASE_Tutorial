#if UNITY_2021_1_OR_NEWER
using Unity.VisualScripting;

namespace HoudiniEngineUnity
{
    [Inspectable]
    public class HEU_UnitButton
    {
        public System.Action action;
    }
}
#endif