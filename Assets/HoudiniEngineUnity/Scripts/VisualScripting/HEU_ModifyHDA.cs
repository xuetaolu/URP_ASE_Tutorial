#if UNITY_2021_1_OR_NEWER
using UnityEngine;
using Unity.VisualScripting;

namespace HoudiniEngineUnity
{
    [UnitCategory("Events/Editor/Houdini Engine")]
    [UnitShortTitle("Modify HDA")]
    [UnitTitle("Houdini Engine Modify HDA")]
    public class HEU_ModifyHDA : ManualEventUnit<EmptyEventArgs>
    {
        protected override string hookName
        {
            get { return "ModifyHDAEvent"; }
        }

        [UnitHeaderInspectable] [HEU_UnitButtonAttribute("TriggerButton", "Modify HDA", 50)]
        public HEU_UnitButton triggerButton;

        // Input
        [DoNotSerialize] public ValueInput inputHDA;

        // Output
        [DoNotSerialize] public ValueOutput outputHDAAsset;

        // Data
        private HEU_HoudiniAsset hdaAsset;

        protected override void Definition()
        {
            base.Definition();
            // Input
            inputHDA = ValueInput<HEU_HoudiniAssetRoot>("HDA", null);

            // Output
            outputHDAAsset = ValueOutput<HEU_HoudiniAsset>("Output HDA", (flow) => { return hdaAsset; });
        }

        public void TriggerButton(GraphReference reference)
        {
            Flow flow = Flow.New(reference);
            HEU_HoudiniAssetRoot hdaAssetRoot = flow.GetValue<HEU_HoudiniAssetRoot>(inputHDA);

            if (hdaAssetRoot != null && hdaAssetRoot.HoudiniAsset != null)
            {
                hdaAsset = hdaAssetRoot.HoudiniAsset;
            }

            System.Action ContinueFlow = () => { flow.Invoke(trigger); };

            Debug.Log("Triggered");

            ContinueFlow();
        }
    }
}

#endif