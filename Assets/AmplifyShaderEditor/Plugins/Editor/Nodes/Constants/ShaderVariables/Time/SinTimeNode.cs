// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEditor;
using UnityEngine;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Sin Time", "Time", "Unity sin time" )]
	public sealed class SinTimeNode : ConstVecShaderVariable
	{
		//double m_time;
#if UNITY_2018_3_OR_NEWER
		private readonly string[] SRPTime =
		{
			"sin( _TimeParameters.x * 0.125 )",
			"sin( _TimeParameters.x * 0.25 )",
			"sin( _TimeParameters.x * 0.5 )",
			"_TimeParameters.y",
		};
#endif
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, "t/8" );
			ChangeOutputName( 2, "t/4" );
			ChangeOutputName( 3, "t/2" );
			ChangeOutputName( 4, "t" );
			m_value = "_SinTime";
			m_previewShaderGUID = "e4ba809e0badeb94994170b2cbbbba10";
			m_continuousPreviewRefresh = true;
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			if( !m_outputPorts[ 0 ].IsConnected )
			{
				m_outputPorts[ 0 ].Visible = false;
				m_sizeIsDirty = true;
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
#if UNITY_2018_3_OR_NEWER
			if( outputId > 0 && dataCollector.IsTemplate )
			{
				if( ( dataCollector.TemplateDataCollectorInstance.IsHDRP && ASEPackageManagerHelper.CurrentHDVersion > ASESRPVersions.ASE_SRP_5_16_1 ) ||
					( dataCollector.TemplateDataCollectorInstance.IsLWRP && ASEPackageManagerHelper.CurrentLWVersion > ASESRPVersions.ASE_SRP_5_16_1 ) )
					return SRPTime[ outputId - 1 ];
			}
#endif
			return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
		}
	}
}
