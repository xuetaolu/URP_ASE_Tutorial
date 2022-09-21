// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
#if UNITY_2018_3_OR_NEWER
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace AmplifyShaderEditor
{
	public enum ASESRPVersions
	{
		ASE_SRP_3_0_0 =		030000,
		ASE_SRP_3_1_0 =		030100,
		ASE_SRP_3_3_0 =		030300,
		ASE_SRP_4_1_0 =		040100,
		ASE_SRP_4_2_0 =		040200,
		ASE_SRP_4_3_0 =		040300,
		ASE_SRP_4_6_0 =		040600,
		ASE_SRP_4_8_0 =		040800,
		ASE_SRP_4_9_0 =		040900,
		ASE_SRP_4_10_0 =	041000,
		ASE_SRP_5_7_2 =		050702,
		ASE_SRP_5_8_2 =		050802,
		ASE_SRP_5_9_0 =		050900,
		ASE_SRP_5_10_0 =	051000,
		ASE_SRP_5_13_0 =	051300,
		ASE_SRP_5_16_1 =	051601,
		ASE_SRP_6_9_0 =		060900,
		ASE_SRP_6_9_1 =		060901,
		ASE_SRP_6_9_2 =		060902,
		ASE_SRP_7_0_1 =		070001,
		ASE_SRP_7_1_1 =		070101,
		ASE_SRP_7_1_2 =		070102,
		ASE_SRP_7_1_5 =		070105,
		ASE_SRP_7_1_6 =		070106,
		ASE_SRP_7_1_7 =		070107,
		ASE_SRP_7_1_8 =		070108,
		ASE_SRP_7_2_0 =		070200,
		ASE_SRP_7_2_1 =		070201,
		ASE_SRP_7_3_1 =		070301,
		ASE_SRP_7_4_1 =		070401,
		ASE_SRP_7_4_2 =		070402,
		ASE_SRP_7_4_3 =		070403,
		ASE_SRP_7_5_1 =		070501,
		ASE_SRP_7_5_2 =		070502,
		ASE_SRP_7_5_3 =		070503,
		ASE_SRP_7_6_0 =		070600,
		ASE_SRP_7_7_1 =		070701,
		ASE_SRP_8_2_0 =		080200,
		ASE_SRP_8_3_1 =		080301,
		ASE_SRP_9_0_0 =		090000,
		ASE_SRP_10_0_0 =	100000,
		ASE_SRP_10_1_0 =	100100,
		ASE_SRP_10_2_2 =	100202,
		ASE_SRP_10_3_1 =	100301,
		ASE_SRP_10_3_2 =	100302,
		ASE_SRP_10_4_0 =	100400,
		ASE_SRP_10_5_0 =	100500,
		ASE_SRP_10_5_1 =	100501,
		ASE_SRP_10_6_0 =	100600,
		ASE_SRP_10_7_0 =	100700,
		ASE_SRP_10_8_0 =	100800,
		ASE_SRP_10_8_1 =	100801,
		ASE_SRP_11_0_0 =	110000,
		ASE_SRP_12_0_0 =	120000,
		ASE_SRP_12_1_0 =	120100,
		ASE_SRP_12_1_1 =	120101,
		ASE_SRP_12_1_2 =	120102,
		ASE_SRP_RECENT =	999999
	}

	public enum ASEImportState
	{
		None,
		Lightweight,
		HD,
		Both
	}

	public static class AssetDatabaseEX
	{
		private static System.Type type = null;
		public static System.Type Type { get { return ( type == null ) ? type = System.Type.GetType( "UnityEditor.AssetDatabase, UnityEditor" ) : type; } }

		public static void ImportPackageImmediately( string packagePath )
		{
			AssetDatabaseEX.Type.InvokeMember( "ImportPackageImmediately", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[] { packagePath } );
		}
	}


	[Serializable]
	public static class ASEPackageManagerHelper
	{
		private static string URPNewVersionDetected =	"A new Universal RP version was detected and new templates are being imported.\n" +
																"Please hit the Update button on your ASE canvas to recompile your shader under the newest version.";

		private static string HDNewVersionDetected =	"A new HD RP version was detected and new templates are being imported.\n" +
														"Please hit the Update button on your ASE canvas to recompile your shader under the newest version.";

		private static string HDPackageId = "com.unity.render-pipelines.high-definition";
		private static string LWPackageId = "com.unity.render-pipelines.lightweight";
		private static string UniversalPackageId = "com.unity.render-pipelines.universal";
		private static string HDEditorPrefsId = "ASEHDEditorPrefsId";
		private static string LWEditorPrefsId = "ASELightweigthEditorPrefsId ";

		private static string URPTemplateVersion = "ASEURPtemplate" + Application.productName;
		private static string HDRPTemplateVersion = "ASEHDRPtemplate" + Application.productName;

		private static string SPKeywordFormat = "ASE_SRP_VERSION {0}";
		private static ListRequest m_packageListRequest = null;
		private static UnityEditor.PackageManager.PackageInfo m_lwPackageInfo;
		private static UnityEditor.PackageManager.PackageInfo m_hdPackageInfo;

		// V4.8.0 and bellow
		// HD 
		private static readonly string[] GetNormalWSFunc =
		{
			"inline void GetNormalWS( FragInputs input, float3 normalTS, out float3 normalWS, float3 doubleSidedConstants )\n",
			"{\n",
			"\tGetNormalWS( input, normalTS, normalWS );\n",
			"}\n"
		};

		// v4.6.0 and below
		private static readonly string[] BuildWordTangentFunc =
		{
			"float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)\n",
			"{\n",
			"\tfloat3 unnormalizedNormalWS = normalWS;\n",
			"\tfloat renormFactor = 1.0 / length(unnormalizedNormalWS);\n",
			"\tfloat3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);\n",
			"\tworldToTangent[0] = worldToTangent[0] * renormFactor;\n",
			"\tworldToTangent[1] = worldToTangent[1] * renormFactor;\n",
			"\tworldToTangent[2] = worldToTangent[2] * renormFactor;\n",
			"\treturn worldToTangent;\n",
			"}\n"
		};

		private static bool m_lateImport = false;
		private static string m_latePackageToImport;

		private static bool m_requireUpdateList = false;
		private static ASEImportState m_importingPackage = ASEImportState.None;


		private static ASESRPVersions m_currentHDVersion = ASESRPVersions.ASE_SRP_RECENT;
		private static ASESRPVersions m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;

		private static int m_urpTemplateVersion = 26;
		private static int m_hdrpTemplateVersion = 20;

		private static Dictionary<string, ASESRPVersions> m_srpVersionConverter = new Dictionary<string, ASESRPVersions>()
		{
			{"3.0.0-preview",		ASESRPVersions.ASE_SRP_3_0_0},
			{"3.1.0-preview",		ASESRPVersions.ASE_SRP_3_1_0},
			{"3.3.0-preview",		ASESRPVersions.ASE_SRP_3_3_0},
			{"4.1.0-preview",		ASESRPVersions.ASE_SRP_4_1_0},
			{"4.2.0-preview",		ASESRPVersions.ASE_SRP_4_2_0},
			{"4.3.0-preview",		ASESRPVersions.ASE_SRP_4_3_0},
			{"4.6.0-preview",		ASESRPVersions.ASE_SRP_4_6_0},
			{"4.8.0-preview",		ASESRPVersions.ASE_SRP_4_8_0},
			{"4.9.0-preview",		ASESRPVersions.ASE_SRP_4_9_0},
			{"4.10.0-preview",		ASESRPVersions.ASE_SRP_4_10_0},
			{"5.7.2-preview",		ASESRPVersions.ASE_SRP_5_7_2},
			{"5.7.2",				ASESRPVersions.ASE_SRP_5_7_2},
			{"5.8.2-preview",		ASESRPVersions.ASE_SRP_5_8_2},
			{"5.8.2",				ASESRPVersions.ASE_SRP_5_8_2},
			{"5.9.0-preview",		ASESRPVersions.ASE_SRP_5_9_0},
			{"5.9.0",				ASESRPVersions.ASE_SRP_5_9_0},
			{"5.10.0-preview",		ASESRPVersions.ASE_SRP_5_10_0},
			{"5.10.0",				ASESRPVersions.ASE_SRP_5_10_0},
			{"5.13.0-preview",		ASESRPVersions.ASE_SRP_5_13_0},
			{"5.13.0",				ASESRPVersions.ASE_SRP_5_13_0},
			{"5.16.1-preview",		ASESRPVersions.ASE_SRP_5_16_1},
			{"5.16.1",				ASESRPVersions.ASE_SRP_5_16_1},
			{"6.9.0",				ASESRPVersions.ASE_SRP_6_9_0},
			{"6.9.0-preview",		ASESRPVersions.ASE_SRP_6_9_0},
			{"6.9.1",				ASESRPVersions.ASE_SRP_6_9_1},
			{"6.9.1-preview",		ASESRPVersions.ASE_SRP_6_9_1},
			{"6.9.2",				ASESRPVersions.ASE_SRP_6_9_2},
			{"6.9.2-preview",		ASESRPVersions.ASE_SRP_6_9_2},
			{"7.0.1",				ASESRPVersions.ASE_SRP_7_0_1},
			{"7.0.1-preview",		ASESRPVersions.ASE_SRP_7_0_1},
			{"7.1.1",				ASESRPVersions.ASE_SRP_7_1_1},
			{"7.1.1-preview",		ASESRPVersions.ASE_SRP_7_1_1},
			{"7.1.2",				ASESRPVersions.ASE_SRP_7_1_2},
			{"7.1.2-preview",		ASESRPVersions.ASE_SRP_7_1_2},
			{"7.1.5",				ASESRPVersions.ASE_SRP_7_1_5},
			{"7.1.5-preview",		ASESRPVersions.ASE_SRP_7_1_5},
			{"7.1.6",				ASESRPVersions.ASE_SRP_7_1_6},
			{"7.1.6-preview",		ASESRPVersions.ASE_SRP_7_1_6},
			{"7.1.7",				ASESRPVersions.ASE_SRP_7_1_7},
			{"7.1.7-preview",		ASESRPVersions.ASE_SRP_7_1_7},
			{"7.1.8",				ASESRPVersions.ASE_SRP_7_1_8},
			{"7.1.8-preview",		ASESRPVersions.ASE_SRP_7_1_8},
			{"7.2.0",				ASESRPVersions.ASE_SRP_7_2_0},
			{"7.2.0-preview",		ASESRPVersions.ASE_SRP_7_2_0},
			{"7.2.1",				ASESRPVersions.ASE_SRP_7_2_1},
			{"7.2.1-preview",		ASESRPVersions.ASE_SRP_7_2_1},
			{"7.3.1",				ASESRPVersions.ASE_SRP_7_3_1},
			{"7.3.1-preview",		ASESRPVersions.ASE_SRP_7_3_1},
			{"7.4.1",				ASESRPVersions.ASE_SRP_7_4_1},
			{"7.4.1-preview",		ASESRPVersions.ASE_SRP_7_4_1},
			{"7.4.2",				ASESRPVersions.ASE_SRP_7_4_2},
			{"7.4.2-preview",		ASESRPVersions.ASE_SRP_7_4_2},
			{"7.4.3",				ASESRPVersions.ASE_SRP_7_4_3},
			{"7.4.3-preview",		ASESRPVersions.ASE_SRP_7_4_3},
			{"7.5.1",               ASESRPVersions.ASE_SRP_7_5_1},
			{"7.5.1-preview",       ASESRPVersions.ASE_SRP_7_5_1},
			{"7.5.2",               ASESRPVersions.ASE_SRP_7_5_2},
			{"7.5.2-preview",       ASESRPVersions.ASE_SRP_7_5_2},
			{"7.5.3",               ASESRPVersions.ASE_SRP_7_5_3},
			{"7.5.3-preview",       ASESRPVersions.ASE_SRP_7_5_3},
			{"7.6.0",               ASESRPVersions.ASE_SRP_7_6_0},
			{"7.6.0-preview",       ASESRPVersions.ASE_SRP_7_6_0},
			{"7.7.1",               ASESRPVersions.ASE_SRP_7_7_1},
			{"7.7.1-preview",       ASESRPVersions.ASE_SRP_7_7_1},
			{"8.2.0",				ASESRPVersions.ASE_SRP_8_2_0},
			{"8.2.0-preview",		ASESRPVersions.ASE_SRP_8_2_0},
			{"8.3.1",               ASESRPVersions.ASE_SRP_8_3_1},
			{"8.3.1-preview",       ASESRPVersions.ASE_SRP_8_3_1},
			{"9.0.0",				ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.13",    ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.14",	ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.33",    ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.35",	ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.54",    ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.55",	ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.71",    ASESRPVersions.ASE_SRP_9_0_0},
			{"9.0.0-preview.72",    ASESRPVersions.ASE_SRP_9_0_0},
			{"10.0.0-preview.26",   ASESRPVersions.ASE_SRP_10_0_0},
			{"10.0.0-preview.27",	ASESRPVersions.ASE_SRP_10_0_0},
			{"10.1.0",				ASESRPVersions.ASE_SRP_10_1_0},
			{"10.2.2",              ASESRPVersions.ASE_SRP_10_2_2},
			{"10.3.1",              ASESRPVersions.ASE_SRP_10_3_1},
			{"10.3.2",              ASESRPVersions.ASE_SRP_10_3_2},
			{"10.4.0",              ASESRPVersions.ASE_SRP_10_4_0},
			{"10.5.0",              ASESRPVersions.ASE_SRP_10_5_0},
			{"10.5.1",              ASESRPVersions.ASE_SRP_10_5_1},
			{"10.6.0",              ASESRPVersions.ASE_SRP_10_6_0},
			{"10.7.0",              ASESRPVersions.ASE_SRP_10_7_0},
			{"10.8.0",              ASESRPVersions.ASE_SRP_10_8_0},
			{"10.8.1",              ASESRPVersions.ASE_SRP_10_8_1},
			{"11.0.0",              ASESRPVersions.ASE_SRP_11_0_0},
			{"12.0.0",              ASESRPVersions.ASE_SRP_12_0_0},
			{"12.1.0",              ASESRPVersions.ASE_SRP_12_1_0},
			{"12.1.1",              ASESRPVersions.ASE_SRP_12_1_1},
			{"12.1.2",              ASESRPVersions.ASE_SRP_12_1_2}
		};

		private static Dictionary<ASESRPVersions, string> m_srpToASEPackageLW = new Dictionary<ASESRPVersions, string>()
		{
			{ASESRPVersions.ASE_SRP_3_0_0,	"b53d2f3b156ff104f90d4d7693d769c8"},
			{ASESRPVersions.ASE_SRP_3_1_0,	"b53d2f3b156ff104f90d4d7693d769c8"},
			{ASESRPVersions.ASE_SRP_3_3_0,	"b53d2f3b156ff104f90d4d7693d769c8"},
			{ASESRPVersions.ASE_SRP_4_1_0,	"3e8eabcfae1e5aa4397de89fedeb48db"},
			{ASESRPVersions.ASE_SRP_4_2_0,	"3e8eabcfae1e5aa4397de89fedeb48db"},
			{ASESRPVersions.ASE_SRP_4_3_0,	"3e8eabcfae1e5aa4397de89fedeb48db"},
			{ASESRPVersions.ASE_SRP_4_6_0,	"3e8eabcfae1e5aa4397de89fedeb48db"},
			{ASESRPVersions.ASE_SRP_4_8_0,	"3e8eabcfae1e5aa4397de89fedeb48db"},
			{ASESRPVersions.ASE_SRP_4_9_0,	"3e8eabcfae1e5aa4397de89fedeb48db"},
			{ASESRPVersions.ASE_SRP_4_10_0,	"3e8eabcfae1e5aa4397de89fedeb48db"},
			{ASESRPVersions.ASE_SRP_5_7_2,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_5_8_2,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_5_9_0,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_5_10_0,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_5_13_0,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_5_16_1,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_6_9_0,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_6_9_1,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_6_9_2,	"4c816894a3147d343891060451241bfe"},
			{ASESRPVersions.ASE_SRP_7_0_1,  "f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_1_1,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_1_2,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_1_5,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_1_6,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_1_7,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_1_8,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_2_0,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_2_1,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_3_1,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_4_1,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_4_2,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_4_3,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_5_1,  "f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_5_2,  "f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_5_3,  "f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_6_0,  "f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_7_7_1,  "f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_8_2_0,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_8_3_1,  "f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_9_0_0,	"f54faaaf4faf8784183ede7f87dfeb23"},
			{ASESRPVersions.ASE_SRP_10_0_0,	"b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_1_0,	"b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_2_2, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_3_1, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_3_2, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_4_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_5_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_5_1, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_6_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_7_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_8_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_8_1, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_11_0_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_12_0_0, "57fcea0ed8b5eb347923c4c21fa31b57"},
			{ASESRPVersions.ASE_SRP_12_1_0, "57fcea0ed8b5eb347923c4c21fa31b57"},
			{ASESRPVersions.ASE_SRP_12_1_1, "57fcea0ed8b5eb347923c4c21fa31b57"},
			{ASESRPVersions.ASE_SRP_12_1_2, "57fcea0ed8b5eb347923c4c21fa31b57"},
			{ASESRPVersions.ASE_SRP_RECENT,	"57fcea0ed8b5eb347923c4c21fa31b57"}
		};

		private static Dictionary<ASESRPVersions, string> m_srpToASEPackageHD = new Dictionary<ASESRPVersions, string>()
		{
			{ASESRPVersions.ASE_SRP_3_0_0,	"4dc1afbcc68875c4780502f5e6b80158"},
			{ASESRPVersions.ASE_SRP_3_1_0,	"4dc1afbcc68875c4780502f5e6b80158"},
			{ASESRPVersions.ASE_SRP_3_3_0,	"4dc1afbcc68875c4780502f5e6b80158"},
			{ASESRPVersions.ASE_SRP_4_1_0,	"5d615bf612f33364e96fb9fd2959ae9c"},
			{ASESRPVersions.ASE_SRP_4_2_0,	"5d615bf612f33364e96fb9fd2959ae9c"},
			{ASESRPVersions.ASE_SRP_4_3_0,	"5d615bf612f33364e96fb9fd2959ae9c"},
			{ASESRPVersions.ASE_SRP_4_6_0,	"5d615bf612f33364e96fb9fd2959ae9c"},
			{ASESRPVersions.ASE_SRP_4_8_0,	"5d615bf612f33364e96fb9fd2959ae9c"},
			{ASESRPVersions.ASE_SRP_4_9_0,	"5d615bf612f33364e96fb9fd2959ae9c"},
			{ASESRPVersions.ASE_SRP_4_10_0,	"5d615bf612f33364e96fb9fd2959ae9c"},
			{ASESRPVersions.ASE_SRP_5_7_2,	"f51b7b861facbc3429fcc5f1f6f91183"},
			{ASESRPVersions.ASE_SRP_5_8_2,	"2d7fe4f7c19e90f41b893bc01fc17230"},
			{ASESRPVersions.ASE_SRP_5_9_0,	"2d7fe4f7c19e90f41b893bc01fc17230"},
			{ASESRPVersions.ASE_SRP_5_10_0,	"2d7fe4f7c19e90f41b893bc01fc17230"},
			{ASESRPVersions.ASE_SRP_5_13_0,	"2d7fe4f7c19e90f41b893bc01fc17230"},
			{ASESRPVersions.ASE_SRP_5_16_1,	"2d7fe4f7c19e90f41b893bc01fc17230"},
			{ASESRPVersions.ASE_SRP_6_9_0,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_6_9_1,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_6_9_2,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_7_0_1,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_7_1_1,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_7_1_2,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_7_1_5,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_7_1_6,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_7_1_7,	"e137dba02f4d0f542ab09dcedea27314"},
			{ASESRPVersions.ASE_SRP_7_1_8,  "38b4bfd7c725505409f8f4d944961db4"},
			{ASESRPVersions.ASE_SRP_7_2_0,  "38b4bfd7c725505409f8f4d944961db4"},
			{ASESRPVersions.ASE_SRP_7_2_1,  "38b4bfd7c725505409f8f4d944961db4"},
			{ASESRPVersions.ASE_SRP_7_3_1,  "38b4bfd7c725505409f8f4d944961db4"},
			{ASESRPVersions.ASE_SRP_7_4_1,  "38b4bfd7c725505409f8f4d944961db4"},
			{ASESRPVersions.ASE_SRP_7_4_2,  "38b4bfd7c725505409f8f4d944961db4"},
			{ASESRPVersions.ASE_SRP_7_4_3,  "38b4bfd7c725505409f8f4d944961db4"},
			{ASESRPVersions.ASE_SRP_7_5_1,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_7_5_2,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_7_5_3,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_7_6_0,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_7_7_1,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_8_2_0,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_8_3_1,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_9_0_0,  "3aeabe705b70b154ea99893f91351100"},
			{ASESRPVersions.ASE_SRP_10_0_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_1_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_2_2, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_3_1, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_3_2, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_4_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_5_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_5_1, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_6_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_7_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_8_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_8_1, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_11_0_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_12_0_0, "9a5e61a8b3421b944863d0946e32da0a"},
			{ASESRPVersions.ASE_SRP_12_1_0, "9a5e61a8b3421b944863d0946e32da0a"},
			{ASESRPVersions.ASE_SRP_12_1_1, "9a5e61a8b3421b944863d0946e32da0a"},
			{ASESRPVersions.ASE_SRP_12_1_2, "9a5e61a8b3421b944863d0946e32da0a"},
			{ASESRPVersions.ASE_SRP_RECENT, "9a5e61a8b3421b944863d0946e32da0a"}

		};

		private static Shader m_lateShader;
		private static Material m_lateMaterial;
		private static AmplifyShaderFunction m_lateShaderFunction;


		public static void RequestInfo()
		{
			if( !m_requireUpdateList && m_importingPackage == ASEImportState.None )
			{
				m_requireUpdateList = true;
				m_packageListRequest = UnityEditor.PackageManager.Client.List( true );
			}
		}

		static void FailedPackageImport( string packageName, string errorMessage )
		{
			FinishImporter();
		}

		static void CancelledPackageImport( string packageName )
		{
			FinishImporter();
		}

		static void CompletedPackageImport( string packageName )
		{
			FinishImporter();
		}

		public static void CheckLatePackageImport()
		{
			if( !Application.isPlaying && m_lateImport && !string.IsNullOrEmpty( m_latePackageToImport ) )
			{
				m_lateImport = false;
				StartImporting( m_latePackageToImport );
				m_latePackageToImport = string.Empty;	
			}
		}

		public static void StartImporting( string packagePath )
		{
			if( !Preferences.GlobalAutoSRP )
			{
				m_importingPackage = ASEImportState.None;
				return;
			}

			if( Application.isPlaying )
			{
				if( !m_lateImport )
				{
					m_lateImport = true;
					m_latePackageToImport = packagePath;
					Debug.LogWarning( "Amplify Shader Editor requires the \""+ packagePath +"\" package to be installed in order to continue. Please exit Play mode to proceed." );
				}
				return;
			}

			AssetDatabase.importPackageCancelled += CancelledPackageImport;
			AssetDatabase.importPackageCompleted += CompletedPackageImport;
			AssetDatabase.importPackageFailed += FailedPackageImport;
			AssetDatabase.ImportPackage( packagePath, false );
			//AssetDatabaseEX.ImportPackageImmediately( packagePath );
		}

		public static void FinishImporter()
		{
			m_importingPackage = ASEImportState.None;
			AssetDatabase.importPackageCancelled -= CancelledPackageImport;
			AssetDatabase.importPackageCompleted -= CompletedPackageImport;
			AssetDatabase.importPackageFailed -= FailedPackageImport;
		}

		public static void SetupLateShader( Shader shader )
		{
			if( shader == null )
				return;

			//If a previous delayed object is pending discard it and register the new one
			// So the last selection will be the choice of opening
			//This can happen when trying to open an ASE canvas while importing templates or in play mode
			if( m_lateShader != null )
			{
				EditorApplication.delayCall -= LateShaderOpener;
			}

			RequestInfo();
			m_lateShader = shader;
			EditorApplication.delayCall += LateShaderOpener;
		}

		public static void LateShaderOpener()
		{
			Preferences.LoadDefaults();
			Update();
			if( IsProcessing )
			{
				EditorApplication.delayCall += LateShaderOpener;
			}
			else
			{
				AmplifyShaderEditorWindow.ConvertShaderToASE( m_lateShader );
				m_lateShader = null;
			}
		}

		public static void SetupLateMaterial( Material material )
		{
			if( material == null )
				return;
			
			//If a previous delayed object is pending discard it and register the new one
			// So the last selection will be the choice of opening
			//This can happen when trying to open an ASE canvas while importing templates or in play mode
			if( m_lateMaterial != null )
			{
				EditorApplication.delayCall -= LateMaterialOpener;
			}

			RequestInfo();
			m_lateMaterial = material;
			EditorApplication.delayCall += LateMaterialOpener;
		}

		public static void LateMaterialOpener()
		{
			Preferences.LoadDefaults();
			Update();
			if( IsProcessing )
			{
				EditorApplication.delayCall += LateMaterialOpener;
			}
			else
			{
				AmplifyShaderEditorWindow.LoadMaterialToASE( m_lateMaterial );
				m_lateMaterial = null;
			}
		}

		public static void SetupLateShaderFunction( AmplifyShaderFunction shaderFunction )
		{
			if( shaderFunction == null )
				return;

			//If a previous delayed object is pending discard it and register the new one
			// So the last selection will be the choice of opening
			//This can happen when trying to open an ASE canvas while importing templates or in play mode
			if( m_lateShaderFunction != null )
			{
				EditorApplication.delayCall -= LateShaderFunctionOpener;
			}

			RequestInfo();
			m_lateShaderFunction = shaderFunction;
			EditorApplication.delayCall += LateShaderFunctionOpener;
		}

		public static void LateShaderFunctionOpener()
		{
			Preferences.LoadDefaults();
			Update();
			if( IsProcessing )
			{
				EditorApplication.delayCall += LateShaderFunctionOpener;
			}
			else
			{
				AmplifyShaderEditorWindow.LoadShaderFunctionToASE( m_lateShaderFunction, false );
				m_lateShaderFunction = null;
			}
		}

		public static void Update()
		{
			//if( Application.isPlaying )
			//	return;

			CheckLatePackageImport();
			//if( m_lwPackageInfo != null )
			//{
			//	if( m_srpVersionConverter[ m_lwPackageInfo.version ] != m_currentLWVersion )
			//	{
			//		m_currentLWVersion = m_srpVersionConverter[ m_lwPackageInfo.version ];
			//		EditorPrefs.SetInt( LWEditorPrefsId, (int)m_currentLWVersion );
			//		m_importingPackage = ASEImportState.Lightweight;
			//		string packagePath = AssetDatabase.GUIDToAssetPath( m_srpToASEPackageLW[ m_currentLWVersion ] );
			//		StartImporting( packagePath );
			//	}
			//}

			//if( m_hdPackageInfo != null )
			//{
			//	if( m_srpVersionConverter[ m_hdPackageInfo.version ] != m_currentHDVersion )
			//	{
			//		m_currentHDVersion = m_srpVersionConverter[ m_hdPackageInfo.version ];
			//		EditorPrefs.SetInt( HDEditorPrefsId, (int)m_currentHDVersion );
			//		m_importingPackage = ASEImportState.HD;
			//		string packagePath = AssetDatabase.GUIDToAssetPath( m_srpToASEPackageHD[ m_currentHDVersion ] );
			//		StartImporting( packagePath );
			//	}
			//}

			if( m_requireUpdateList && m_importingPackage == ASEImportState.None )
			{
				if( m_packageListRequest != null && m_packageListRequest.IsCompleted )
				{
					m_requireUpdateList = false;
					foreach( UnityEditor.PackageManager.PackageInfo pi in m_packageListRequest.Result )
					{
						if( pi.name.Equals( LWPackageId ) )
						{
							m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;
							m_lwPackageInfo = pi;
							ASESRPVersions oldVersion = (ASESRPVersions)EditorPrefs.GetInt( LWEditorPrefsId );
							if( m_srpVersionConverter.ContainsKey( pi.version ) )
							{
								m_currentLWVersion = m_srpVersionConverter[ pi.version ];
							}
							else
							{
								m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;
							}

							EditorPrefs.SetInt( LWEditorPrefsId, (int)m_currentLWVersion );
							bool foundNewVersion = oldVersion != m_currentLWVersion;
							if( !File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.LightweigthPBRGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.LightweigthUnlitGUID ) ) ||
								foundNewVersion
								)
							{
								if( foundNewVersion )
									Debug.Log( URPNewVersionDetected );

								m_importingPackage = ASEImportState.Lightweight;
								string guid = m_srpToASEPackageLW.ContainsKey( m_currentLWVersion ) ? m_srpToASEPackageLW[ m_currentLWVersion ] : m_srpToASEPackageLW[ ASESRPVersions.ASE_SRP_RECENT ];
								string packagePath = AssetDatabase.GUIDToAssetPath( guid );
								StartImporting( packagePath );
							}
						}

						if( pi.name.Equals( UniversalPackageId ) )
						{
							m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;
							m_lwPackageInfo = pi;
							ASESRPVersions oldVersion = (ASESRPVersions)EditorPrefs.GetInt( LWEditorPrefsId );
							if( m_srpVersionConverter.ContainsKey( pi.version ) )
							{
								m_currentLWVersion = m_srpVersionConverter[ pi.version ];
							}
							else
							{
								m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;
							}

							EditorPrefs.SetInt( LWEditorPrefsId, (int)m_currentLWVersion );
							bool foundNewVersion = oldVersion != m_currentLWVersion;

							int urpVersion = EditorPrefs.GetInt( URPTemplateVersion, m_urpTemplateVersion );
							if( urpVersion < m_urpTemplateVersion )
								foundNewVersion = true;
							EditorPrefs.SetInt( URPTemplateVersion, m_urpTemplateVersion );

							if( !File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.UniversalPBRGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.UniversalUnlitGUID ) ) ||
								foundNewVersion
								)
							{
								if( foundNewVersion )
									Debug.Log( URPNewVersionDetected );

								m_importingPackage = ASEImportState.Lightweight;
								string guid = m_srpToASEPackageLW.ContainsKey( m_currentLWVersion ) ? m_srpToASEPackageLW[ m_currentLWVersion ] : m_srpToASEPackageLW[ ASESRPVersions.ASE_SRP_RECENT ];
								string packagePath = AssetDatabase.GUIDToAssetPath( guid );
								StartImporting( packagePath );
							}
							
						}

						if( pi.name.Equals( HDPackageId ) )
						{
							m_currentHDVersion = ASESRPVersions.ASE_SRP_RECENT;
							m_hdPackageInfo = pi;
							ASESRPVersions oldVersion = (ASESRPVersions)EditorPrefs.GetInt( HDEditorPrefsId );
							if( m_srpVersionConverter.ContainsKey( pi.version ) )
							{
								m_currentHDVersion = m_srpVersionConverter[ pi.version ];
							}
							else
							{
								m_currentHDVersion = ASESRPVersions.ASE_SRP_RECENT;
							}

							EditorPrefs.SetInt( HDEditorPrefsId, (int)m_currentHDVersion );
							bool foundNewVersion = oldVersion != m_currentHDVersion;

							int hdrpVersion = EditorPrefs.GetInt( HDRPTemplateVersion, m_hdrpTemplateVersion );
							if( hdrpVersion < m_hdrpTemplateVersion )
								foundNewVersion = true;
							EditorPrefs.SetInt( HDRPTemplateVersion, m_hdrpTemplateVersion );

#if UNITY_2019_3_OR_NEWER
							if( !File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDNewLitGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDNewPBRGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDNewUnlitGUID ) ) ||
#else
							if( !File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDLitGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDPBRGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDUnlitGUID ) ) ||
#endif
								foundNewVersion
								)
							{
								if( foundNewVersion )
									Debug.Log( HDNewVersionDetected );

								m_importingPackage = m_importingPackage == ASEImportState.Lightweight ? ASEImportState.Both : ASEImportState.HD;
								string guid = m_srpToASEPackageHD.ContainsKey( m_currentHDVersion ) ? m_srpToASEPackageHD[ m_currentHDVersion ] : m_srpToASEPackageHD[ ASESRPVersions.ASE_SRP_RECENT ];
								string packagePath = AssetDatabase.GUIDToAssetPath( guid );
								StartImporting( packagePath );
							}
							
						}
					}
				}
			}
		}

		public static void SetSRPInfoOnDataCollector( ref MasterNodeDataCollector dataCollector )
		{
			Preferences.LoadDefaults();
			if( m_requireUpdateList )
				Update();

			if( dataCollector.CurrentSRPType == TemplateSRPType.HD )
			{
				dataCollector.AddToDirectives( string.Format( SPKeywordFormat, (int)m_currentHDVersion ) ,-1, AdditionalLineType.Define );
				if( m_currentHDVersion < ASESRPVersions.ASE_SRP_4_9_0 )
				{
					dataCollector.AddFunction( GetNormalWSFunc[ 0 ], GetNormalWSFunc, false );
				}

				if( m_currentHDVersion < ASESRPVersions.ASE_SRP_4_8_0 )
				{
					dataCollector.AddFunction( BuildWordTangentFunc[ 0 ], BuildWordTangentFunc, false );
				}
			}

			if( dataCollector.CurrentSRPType == TemplateSRPType.Lightweight )
				dataCollector.AddToDirectives( string.Format( SPKeywordFormat, (int)m_currentLWVersion ), -1, AdditionalLineType.Define );
		}
		public static ASESRPVersions CurrentHDVersion { get { return m_currentHDVersion; } }
		public static ASESRPVersions CurrentLWVersion { get { return m_currentLWVersion; } }

		public static bool FoundHDVersion { get { return m_hdPackageInfo != null; } }
		public static bool FoundLWVersion { get { return m_lwPackageInfo != null; } }

		public static bool CheckImporter { get { return m_importingPackage != ASEImportState.None; } }
		public static bool IsProcessing { get { return m_requireUpdateList && m_importingPackage == ASEImportState.None; } }
	}
}
#endif
