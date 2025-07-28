#if UNITY_2021_1_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoudiniEngineUnity
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HEU_UnitButtonAttribute : Attribute
    {
        public string functionName;
        public string buttonLabel;
        public int buttonWidth;

        public HEU_UnitButtonAttribute(string fnName, string btnLabel, int btnWidth)
        {
            this.functionName = fnName;
            this.buttonLabel = btnLabel;
            this.buttonWidth = btnWidth;
        }
    }
}
#endif