#if UNITY_2021_1_OR_NEWER
using UnityEngine;
using Unity.VisualScripting;

namespace HoudiniEngineUnity
{
    [UnitCategory("Events/Editor/Houdini Engine")]
    [UnitShortTitle("Instantiate HDA")]
    [UnitTitle("Houdini Engine Instantiate HDA")]
    public class HEU_InstantiateHDA : ManualEventUnit<EmptyEventArgs>
    {
        protected override string hookName
        {
            get { return "InstantiateHDAEvent"; }
        }

        [UnitHeaderInspectable] [HEU_UnitButtonAttribute("TriggerButton", "Instantiate HDA", 100)]
        public HEU_UnitButton triggerButton;

        // Input
        [DoNotSerialize] public ValueInput inputPath;

        [DoNotSerialize] public ValueInput inputAsync;

        [DoNotSerialize] public ValueInput inputPosition;

        // Output
        [DoNotSerialize] public ValueOutput outputHDARoot;

        [DoNotSerialize] public ValueOutput outputHDAAsset;

        [DoNotSerialize] public ValueOutput outputSuccess;

        // Data
        private HEU_HoudiniAssetRoot hdaRoot;

        private HEU_HoudiniAsset hdaAsset;
        private bool bSuccess;

        protected override void Definition()
        {
            base.Definition();
            // Input
            inputPath = ValueInput<string>("HDA Path", "Assets/Plugins/HoudiniEngineUnity/HDAs/EverGreen.otl");
            inputAsync = ValueInput<bool>("Cook Async", false);
            inputPosition = ValueInput<Vector3>("Instantiation Position", Vector3.zero);

            // Output
            outputHDARoot = ValueOutput<HEU_HoudiniAssetRoot>("Output HDA Root", (flow) => { return hdaRoot; });
            outputHDAAsset = ValueOutput<HEU_HoudiniAsset>("Output HDA", (flow) => { return hdaAsset; });
            outputSuccess = ValueOutput<bool>("Success", (flow) => { return bSuccess; });
        }

        public void TriggerButton(GraphReference reference)
        {
            Flow flow = Flow.New(reference);
            string hdaPath = flow.GetValue<string>(inputPath);
            bool hdaAsync = flow.GetValue<bool>(inputAsync);
            Vector3 hdaPosition = flow.GetValue<Vector3>(inputPosition);

            System.Action ContinueFlow = () => { flow.Invoke(trigger); };

            bool hasErrored = false;

            try
            {
                HEU_SessionBase session = HEU_SessionManager.GetOrCreateDefaultSession(true);
                if (session != null)
                {
                    GameObject go = HEU_HAPIUtility.InstantiateHDA(hdaPath, hdaPosition, session, bBuildAsync: hdaAsync);

                    hdaRoot = go.GetComponent<HEU_HoudiniAssetRoot>();

                    if (hdaRoot != null)
                    {
                        hdaAsset = hdaRoot.HoudiniAsset;
                        if (hdaAsync)
                        {
                            hdaAsset.ReloadDataEvent.AddListener((HEU_ReloadEventData data) =>
                            {
                                bSuccess = data.CookSuccess;
                                ContinueFlow();
                            });
                        }
                        else
                        {
                            bSuccess = hdaAsset.LastCookResult != HEU_AssetCookResultWrapper.ERRORED;
                        }
                    }
                    else
                    {
                        hasErrored = true;
                    }
                }
                else
                {
                    hasErrored = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                hasErrored = true;
            }


            if (hasErrored)
            {
                bSuccess = false;
                ContinueFlow();
            }
            else if (!hdaAsync)
            {
                ContinueFlow();
            }
        }
    }
}

#endif