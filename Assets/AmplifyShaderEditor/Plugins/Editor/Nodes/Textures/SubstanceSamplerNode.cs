// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
#if !UNITY_2018_1_OR_NEWER
namespace AmplifyShaderEditor
{
	// Disabling Substance Deprecated warning
#pragma warning disable 0618
	[Serializable]
	[NodeAttributes( "Substance Sample", "Textures", "Samples a procedural material", KeyCode.None, true, 0, int.MaxValue, typeof( SubstanceArchive ), typeof( ProceduralMaterial ) )]
	public sealed class SubstanceSamplerNode : PropertyNode
	{
		private const string GlobalVarDecStr = "uniform sampler2D {0};";
		private const string PropertyDecStr = "{0}(\"{0}\", 2D) = \"white\"";

		private const string AutoNormalStr = "Auto-Normal";
		private const string SubstanceStr = "Substance";

		private float TexturePreviewSizeX = 128;
		private float TexturePreviewSizeY = 128;

		private float PickerPreviewWidthAdjust = 18;

		private bool m_editing;

		private CacheNodeConnections m_cacheNodeConnections;

		[SerializeField]
		private int m_firstOutputConnected = 0;

		[SerializeField]
		private ProceduralMaterial m_proceduralMaterial;

		[SerializeField]
		private int m_textureCoordSet = 0;

		[SerializeField]
		private ProceduralOutputType[] m_textureTypes;

		[SerializeField]
		private bool m_autoNormal = true;

		private System.Type m_type;

		private Texture[] m_textures = new Texture[] { };
		
		private List<int> m_outputConns = new List<int>();

		private Rect m_previewArea;

		private Rect m_pickerArea;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT2, false, "UV" );
			AddOutputPort( WirePortDataType.COLOR, Constants.EmptyPortValue );
			m_insideSize.Set( TexturePreviewSizeX + PickerPreviewWidthAdjust, TexturePreviewSizeY + 10 );
			m_type = typeof( ProceduralMaterial );
			m_currentParameterType = PropertyType.Property;
			m_freeType = false;
			m_freeName = false;
			m_autoWrapProperties = true;
			m_customPrefix = "Substance Sample ";
			m_drawPrecisionUI = false;
			m_showPreview = true;
			m_drawPreviewExpander = false;
			m_selectedLocation = PreviewLocation.TopCenter;
			m_cacheNodeConnections = new CacheNodeConnections();
			m_previewShaderGUID = "6f322c1da33f1e744941aafcb0ad1a2d";
			m_showAutoRegisterUI = false;
		}

		public override void RenderNodePreview()
		{
			//Runs at least one time
			if( !m_initialized )
			{
				// nodes with no preview don't update at all
				PreviewIsDirty = false;
				return;
			}

			if( !PreviewIsDirty )
				return;

			SetPreviewInputs();

			PreviewMaterial.SetInt( "_CustomUVs", m_inputPorts[ 0 ].IsConnected ? 1 : 0 );

			if( m_proceduralMaterial == null )
				return;

			if( !Preferences.GlobalDisablePreviews )
			{
				Texture[] texs = m_proceduralMaterial.GetGeneratedTextures();
				int count = m_outputPorts.Count;
				for( int i = 0 ; i < count ; i++ )
				{
					RenderTexture temp = RenderTexture.active;
					RenderTexture.active = m_outputPorts[ i ].OutputPreviewTexture;

					PreviewMaterial.SetTexture( "_GenTex" , texs[ i ] );

					if( m_autoNormal && m_textureTypes[ i ] == ProceduralOutputType.Normal )
						Graphics.Blit( null , m_outputPorts[ i ].OutputPreviewTexture , PreviewMaterial , 1 );
					else
						Graphics.Blit( null , m_outputPorts[ i ].OutputPreviewTexture , PreviewMaterial , 0 );
					RenderTexture.active = temp;
				}
			}

			PreviewIsDirty = m_continuousPreviewRefresh;
		}

		public override void OnOutputPortConnected( int portId, int otherNodeId, int otherPortId )
		{
			base.OnOutputPortConnected( portId, otherNodeId, otherPortId );
			m_firstOutputConnected = -1;
		}

		public override void OnOutputPortDisconnected( int portId )
		{
			base.OnOutputPortDisconnected( portId );
			m_firstOutputConnected = -1;
		}

		void CalculateFirstOutputConnected()
		{
			m_outputConns.Clear();
			int count = m_outputPorts.Count;
			bool connectionsAvailable = false;
			for( int i = 0; i < count; i++ )
			{
				if( m_outputPorts[ i ].IsConnected )
				{
					connectionsAvailable = true;
				}
			}

			for( int i = 0; i < count; i++ )
			{
				if( connectionsAvailable )
				{
					if( m_outputPorts[ i ].IsConnected )
					{
						if( m_firstOutputConnected < 0 )
							m_firstOutputConnected = i;

						m_outputConns.Add( i );
					}
				}
				else
				{
					m_outputConns.Add( i );
				}
			}

			if( m_firstOutputConnected < 0 )
				m_firstOutputConnected = 0;
		}

		public override void OnNodeLayout( DrawInfo drawInfo )
		{
			base.OnNodeLayout( drawInfo );

			m_previewArea = m_remainingBox;
			m_previewArea.width = TexturePreviewSizeX * drawInfo.InvertedZoom;
			m_previewArea.height = TexturePreviewSizeY * drawInfo.InvertedZoom;
			m_previewArea.x += 0.5f * m_remainingBox.width - 0.5f * m_previewArea.width;
			m_pickerArea = m_previewArea;
			m_pickerArea.width = 40 * drawInfo.InvertedZoom;
			m_pickerArea.x = m_previewArea.xMax - m_pickerArea.width - 2;
			m_pickerArea.height = 14 * drawInfo.InvertedZoom;
			m_pickerArea.y = m_previewArea.yMax - m_pickerArea.height - 2;
		}

		public override void DrawGUIControls( DrawInfo drawInfo )
		{
			base.DrawGUIControls( drawInfo );

			if( !( drawInfo.CurrentEventType == EventType.MouseDown || drawInfo.CurrentEventType == EventType.MouseUp || drawInfo.CurrentEventType == EventType.ExecuteCommand || drawInfo.CurrentEventType == EventType.DragPerform ) )
				return;

			bool insideBox = m_previewArea.Contains( drawInfo.MousePosition );

			if( insideBox )
			{
				m_editing = true;
			}
			else if( m_editing && !insideBox && drawInfo.CurrentEventType != EventType.ExecuteCommand )
			{
				GUI.FocusControl( null );
				m_editing = false;
			}
		}

		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );

			if( m_editing )
			{
				m_textures = m_proceduralMaterial != null ? m_proceduralMaterial.GetGeneratedTextures() : null;
				if( GUI.Button( m_pickerArea, string.Empty, GUIStyle.none ) )
				{
					int controlID = EditorGUIUtility.GetControlID( FocusType.Passive );
					EditorGUIUtility.ShowObjectPicker<ProceduralMaterial>( m_proceduralMaterial, false, "", controlID );
				}

				string commandName = Event.current.commandName;
				UnityEngine.Object newValue = null;
				if( commandName == "ObjectSelectorUpdated" )
				{
					newValue = EditorGUIUtility.GetObjectPickerObject();
					if( newValue != (UnityEngine.Object)m_proceduralMaterial )
					{
						PreviewIsDirty = true;
						UndoRecordObject( "Changing value EditorGUIObjectField on node Substance Sample" );

						m_proceduralMaterial = newValue != null ? (ProceduralMaterial)newValue : null;
						m_textures = m_proceduralMaterial != null ? m_proceduralMaterial.GetGeneratedTextures() : null;
						OnNewSubstanceSelected( m_textures );
					}
				}
				else if( commandName == "ObjectSelectorClosed" )
				{
					newValue = EditorGUIUtility.GetObjectPickerObject();
					if( newValue != (UnityEngine.Object)m_proceduralMaterial )
					{
						PreviewIsDirty = true;
						UndoRecordObject( "Changing value EditorGUIObjectField on node Substance Sample" );

						m_proceduralMaterial = newValue != null ? (ProceduralMaterial)newValue : null;
						m_textures = m_proceduralMaterial != null ? m_proceduralMaterial.GetGeneratedTextures() : null;
						OnNewSubstanceSelected( m_textures );
					}
					m_editing = false;
				}

				if( GUI.Button( m_previewArea, string.Empty, GUIStyle.none ) )
				{
					if( m_proceduralMaterial != null )
					{
						Selection.activeObject = m_proceduralMaterial;
						EditorGUIUtility.PingObject( Selection.activeObject );
					}
					m_editing = false;
				}
			}

			if( drawInfo.CurrentEventType == EventType.Repaint )
			{
				if( !m_editing )
					m_textures = m_proceduralMaterial != null ? m_proceduralMaterial.GetGeneratedTextures() : null;

				if( m_textures != null )
				{
					if( m_firstOutputConnected < 0 )
					{
						CalculateFirstOutputConnected();
					}
					else if( m_textures.Length != m_textureTypes.Length )
					{
						OnNewSubstanceSelected( m_textures );
					}

					int texCount = m_outputConns.Count;
					Rect individuals = m_previewArea;
					individuals.height /= texCount;

					for( int i = 0; i < texCount; i++ )
					{
						EditorGUI.DrawPreviewTexture( individuals, m_textures[ m_outputConns[ i ] ], null, ScaleMode.ScaleAndCrop );
						individuals.y += individuals.height;
					}
				}
				else
				{
					GUI.Label( m_previewArea, string.Empty, UIUtils.ObjectFieldThumb );
				}

				if( ContainerGraph.LodLevel <= ParentGraph.NodeLOD.LOD2 )
				{
					Rect smallButton = m_previewArea;
					smallButton.height = 14 * drawInfo.InvertedZoom;
					smallButton.y = m_previewArea.yMax - smallButton.height - 2;
					smallButton.width = 40 * drawInfo.InvertedZoom;
					smallButton.x = m_previewArea.xMax - smallButton.width - 2;
					if( m_textures == null )
					{
						GUI.Label( m_previewArea, "None (Procedural Material)", UIUtils.ObjectFieldThumbOverlay );
					}
					GUI.Label( m_pickerArea, "Select", UIUtils.GetCustomStyle( CustomStyle.SamplerButton ) );
				}

				GUI.Label( m_previewArea, string.Empty, UIUtils.GetCustomStyle( CustomStyle.SamplerFrame ) );
			}
		}

		void OnNewSubstanceSelected( Texture[] textures )
		{
			CacheCurrentSettings();
			ConfigPortsFromMaterial( true, textures );
			ConnectFromCache();
			m_requireMaterialUpdate = true;
			CalculateFirstOutputConnected();
			ContainerGraph.ParentWindow.RequestRepaint();
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUI.BeginChangeCheck();
			m_proceduralMaterial = EditorGUILayoutObjectField( SubstanceStr, m_proceduralMaterial, m_type, false ) as ProceduralMaterial;
			if( EditorGUI.EndChangeCheck() )
			{
				Texture[] textures = m_proceduralMaterial != null ? m_proceduralMaterial.GetGeneratedTextures() : null;
				if( textures != null )
				{
					OnNewSubstanceSelected( textures );
				}
			}

			m_textureCoordSet = EditorGUILayoutIntPopup( Constants.AvailableUVSetsLabel, m_textureCoordSet, Constants.AvailableUVSetsStr, Constants.AvailableUVSets );
			EditorGUI.BeginChangeCheck();
			m_autoNormal = EditorGUILayoutToggle( AutoNormalStr, m_autoNormal );
			if( EditorGUI.EndChangeCheck() )
			{
				for( int i = 0; i < m_textureTypes.Length; i++ )
				{
					WirePortDataType portType = ( m_autoNormal && m_textureTypes[ i ] == ProceduralOutputType.Normal ) ? WirePortDataType.FLOAT3 : WirePortDataType.COLOR;
					if( m_outputPorts[ i ].DataType != portType )
					{
						m_outputPorts[ i ].ChangeType( portType, false );
					}
				}
			}
		}

		private void CacheCurrentSettings()
		{
			m_cacheNodeConnections.Clear();
			for( int portId = 0; portId < m_outputPorts.Count; portId++ )
			{
				if( m_outputPorts[ portId ].IsConnected )
				{
					int connCount = m_outputPorts[ portId ].ConnectionCount;
					for( int connIdx = 0; connIdx < connCount; connIdx++ )
					{
						WireReference connection = m_outputPorts[ portId ].GetConnection( connIdx );
						m_cacheNodeConnections.Add( m_outputPorts[ portId ].Name, new NodeCache( connection.NodeId, connection.PortId ) );
					}
				}
			}
		}

		private void ConnectFromCache()
		{
			for( int i = 0; i < m_outputPorts.Count; i++ )
			{
				List<NodeCache> connections = m_cacheNodeConnections.GetList( m_outputPorts[ i ].Name );
				if( connections != null )
				{
					int count = connections.Count;
					for( int connIdx = 0; connIdx < count; connIdx++ )
					{
						UIUtils.SetConnection( connections[ connIdx ].TargetNodeId, connections[ connIdx ].TargetPortId, UniqueId, i );
					}
				}
			}
		}


		private void ConfigPortsFromMaterial( bool invalidateConnections = false, Texture[] newTextures = null )
		{
			SetAdditonalTitleText( ( m_proceduralMaterial != null ) ? string.Format( Constants.PropertyValueLabel, m_proceduralMaterial.name ) : "Value( <None> )" );

			Texture[] textures = newTextures != null ? newTextures : ( ( m_proceduralMaterial != null ) ? m_proceduralMaterial.GetGeneratedTextures() : null );
			if( textures != null )
			{
				m_firstOutputConnected = -1;
				string nameToRemove = m_proceduralMaterial.name + "_";
				m_textureTypes = new ProceduralOutputType[ textures.Length ];
				for( int i = 0; i < textures.Length; i++ )
				{
					ProceduralTexture procTex = textures[ i ] as ProceduralTexture;
					m_textureTypes[ i ] = procTex.GetProceduralOutputType();

					WirePortDataType portType = ( m_autoNormal && m_textureTypes[ i ] == ProceduralOutputType.Normal ) ? WirePortDataType.FLOAT3 : WirePortDataType.COLOR;
					string newName = textures[ i ].name.Replace( nameToRemove, string.Empty );
					char firstLetter = Char.ToUpper( newName[ 0 ] );
					newName = firstLetter.ToString() + newName.Substring( 1 );
					if( i < m_outputPorts.Count )
					{
						m_outputPorts[ i ].ChangeProperties( newName, portType, false );
						if( invalidateConnections )
						{
							m_outputPorts[ i ].FullDeleteConnections();
						}
					}
					else
					{
						AddOutputPort( portType, newName );
					}
				}

				if( textures.Length < m_outputPorts.Count )
				{
					int itemsToRemove = m_outputPorts.Count - textures.Length;
					for( int i = 0; i < itemsToRemove; i++ )
					{
						int idx = m_outputPorts.Count - 1;
						if( m_outputPorts[ idx ].IsConnected )
						{
							m_outputPorts[ idx ].ForceClearConnection();
						}
						RemoveOutputPort( idx );
					}
				}
			}
			else
			{
				int itemsToRemove = m_outputPorts.Count - 1;
				m_outputPorts[ 0 ].ChangeProperties( Constants.EmptyPortValue, WirePortDataType.COLOR, false );
				m_outputPorts[ 0 ].ForceClearConnection();

				for( int i = 0; i < itemsToRemove; i++ )
				{
					int idx = m_outputPorts.Count - 1;
					if( m_outputPorts[ idx ].IsConnected )
					{
						m_outputPorts[ idx ].ForceClearConnection();
					}
					RemoveOutputPort( idx );
				}
			}

			m_sizeIsDirty = true;
			m_isDirty = true;
		}

		private void ConfigFromObject( UnityEngine.Object obj )
		{
			ProceduralMaterial newMat = AssetDatabase.LoadAssetAtPath<ProceduralMaterial>( AssetDatabase.GetAssetPath( obj ) );
			if( newMat != null )
			{
				m_proceduralMaterial = newMat;
				ConfigPortsFromMaterial();
			}
		}

		public override void OnObjectDropped( UnityEngine.Object obj )
		{
			ConfigFromObject( obj );
		}

		public override void SetupFromCastObject( UnityEngine.Object obj )
		{
			ConfigFromObject( obj );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_proceduralMaterial == null )
			{
				return "(0).xxxx";
			}

			if( m_outputPorts[ outputId ].IsLocalValue( dataCollector.PortCategory ) )
			{
				return m_outputPorts[ outputId ].LocalValue( dataCollector.PortCategory );
			}

			Texture[] textures = m_proceduralMaterial.GetGeneratedTextures();

			string uvPropertyName = string.Empty;
			for( int i = 0; i < m_outputPorts.Count; i++ )
			{
				if( m_outputPorts[ i ].HasConnectedNode )
				{
					uvPropertyName = textures[ i ].name;
					break;
				}
			}

			string name = textures[ outputId ].name + OutputId;
			dataCollector.AddToUniforms( UniqueId, string.Format( GlobalVarDecStr, textures[ outputId ].name ) );
			dataCollector.AddToProperties( UniqueId, string.Format( PropertyDecStr, textures[ outputId ].name ) + "{}", -1 );
			bool isVertex = ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation );
			string value = string.Format( "tex2D{0}({1}, {2})", ( isVertex ? "lod" : string.Empty ), textures[ outputId ].name, GetUVCoords( ref dataCollector, ignoreLocalvar, uvPropertyName ) );
			if( m_autoNormal && m_textureTypes[ outputId ] == ProceduralOutputType.Normal )
			{
				value = GeneratorUtils.GenerateUnpackNormalStr( ref dataCollector, CurrentPrecisionType, UniqueId, OutputId, value, false, "1.0" , UnpackInputMode.Tangent );
			}

			dataCollector.AddPropertyNode( this );
			RegisterLocalVariable( outputId, value, ref dataCollector, name );

			return m_outputPorts[ outputId ].LocalValue( dataCollector.PortCategory );
		}

		public string GetUVCoords( ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar, string propertyName )
		{
			bool isVertex = ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation );
			if( m_inputPorts[ 0 ].IsConnected )
			{
				return m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, isVertex ? WirePortDataType.FLOAT4 : WirePortDataType.FLOAT2, ignoreLocalVar, true );
			}
			else
			{
				string uvChannelName = IOUtils.GetUVChannelName( propertyName, m_textureCoordSet );

				if( dataCollector.IsTemplate )
				{
					string propertyHelperVar = propertyName + "_ST";
					dataCollector.AddToUniforms( UniqueId, "float4", propertyHelperVar, dataCollector.IsSRP );
					string uvName = string.Empty;
					if( dataCollector.TemplateDataCollectorInstance.HasUV( m_textureCoordSet ) )
					{
						uvName = dataCollector.TemplateDataCollectorInstance.GetUVName( m_textureCoordSet );
					}
					else
					{
						uvName = dataCollector.TemplateDataCollectorInstance.RegisterUV( m_textureCoordSet );
					}

					uvChannelName = "uv" + propertyName;
					if( isVertex )
					{
						string value = string.Format( Constants.TilingOffsetFormat, uvName, propertyHelperVar + ".xy", propertyHelperVar + ".zw" );
						string lodLevel = "0";

						value = "float4( " + value + ", 0 , " + lodLevel + " )";
						dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT4, uvChannelName, value );
					}
					else
					{
						dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT2, uvChannelName, string.Format( Constants.TilingOffsetFormat, uvName, propertyHelperVar + ".xy", propertyHelperVar + ".zw" ) );
					}
				}
				else
				{
					string vertexCoords = Constants.VertexShaderInputStr + ".texcoord";
					if( m_textureCoordSet > 0 )
					{
						vertexCoords += m_textureCoordSet.ToString();
					}


					string dummyPropUV = "_texcoord" + ( m_textureCoordSet > 0 ? ( m_textureCoordSet + 1 ).ToString() : "" );
					string dummyUV = "uv" + ( m_textureCoordSet > 0 ? ( m_textureCoordSet + 1 ).ToString() : "" ) + dummyPropUV;

					dataCollector.AddToUniforms( UniqueId, "uniform float4 " + propertyName + "_ST;" );
					dataCollector.AddToProperties( UniqueId, "[HideInInspector] " + dummyPropUV + "( \"\", 2D ) = \"white\" {}", 100 );
					dataCollector.AddToInput( UniqueId, dummyUV, WirePortDataType.FLOAT2 );

					if( isVertex )
					{
						dataCollector.AddToVertexLocalVariables( UniqueId, "float4 " + uvChannelName + " = float4(" + vertexCoords + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw, 0 ,0);" );
						return uvChannelName;
					}
					else
						dataCollector.AddToLocalVariables( UniqueId, PrecisionType.Float, WirePortDataType.FLOAT2, uvChannelName, Constants.InputVarStr + "." + dummyUV + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw" );

				}

				return uvChannelName;
			}
		}

		public override void UpdateMaterial( Material mat )
		{
			base.UpdateMaterial( mat );
			if( m_proceduralMaterial != null )
			{
				Texture[] textures = m_proceduralMaterial.GetGeneratedTextures();
				for( int i = 0; i < textures.Length; i++ )
				{
					if( mat.HasProperty( textures[ i ].name ) && !InsideShaderFunction )
					{
						mat.SetTexture( textures[ i ].name, textures[ i ] );
					}
				}
			}
		}

		public override bool UpdateShaderDefaults( ref Shader shader, ref TextureDefaultsDataColector defaultCol )
		{
			if( m_proceduralMaterial != null )
			{
				Texture[] textures = m_proceduralMaterial.GetGeneratedTextures();
				for( int i = 0; i < textures.Length; i++ )
				{
					defaultCol.AddValue( textures[ i ].name, textures[ i ] );
				}
			}
			return true;
		}

		public override void Destroy()
		{
			base.Destroy();
			m_textures = null;
			m_proceduralMaterial = null;
			m_cacheNodeConnections.Clear();
			m_cacheNodeConnections = null;
			m_outputConns.Clear();
			m_outputConns = null;
		}

		public override string GetPropertyValStr()
		{
			return m_proceduralMaterial ? m_proceduralMaterial.name : string.Empty;
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			string guid = GetCurrentParam( ref nodeParams );
			m_textureCoordSet = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_autoNormal = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			if( guid.Length > 1 )
			{
				m_proceduralMaterial = AssetDatabase.LoadAssetAtPath<ProceduralMaterial>( AssetDatabase.GUIDToAssetPath( guid ) );
				if( m_proceduralMaterial != null )
				{
					ConfigPortsFromMaterial();
				}
				else
				{
					UIUtils.ShowMessage( UniqueId, "Substance not found ", MessageSeverity.Error );
				}
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			string guid = ( m_proceduralMaterial != null ) ? AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath( m_proceduralMaterial ) ) : "0";
			IOUtils.AddFieldValueToString( ref nodeInfo, guid );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_textureCoordSet );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_autoNormal );
		}

	}
#pragma warning restore 0618
}
#elif SUBSTANCE_PLUGIN_ENABLED

using Substance.Game;

namespace AmplifyShaderEditor
{
	public enum ASEProceduralOutputType
	{
		Color,
		Normal,
	}
	// Disabling Substance Deprecated warning
#pragma warning disable 0618
	[Serializable]
	[NodeAttributes( "Substance Sample", "Textures", "Samples a procedural material", KeyCode.None, true, 0, int.MaxValue, typeof( SubstanceGraph ), typeof( Substance.Game.Substance ) )]
	public sealed class SubstanceSamplerNode : PropertyNode
	{
		private const string NormalMapCheck = "normal";
		private const string GlobalVarDecStr = "uniform sampler2D {0};";
		private const string PropertyDecStr = "{0}(\"{1}\", 2D) = \"white\"";

		private const string AutoNormalStr = "Auto-Normal";
		private const string SubstanceStr = "Substance";

		private float TexturePreviewSizeX = 128;
		private float TexturePreviewSizeY = 128;

		private float PickerPreviewWidthAdjust = 18;

		private bool m_editing;

		private CacheNodeConnections m_cacheNodeConnections;

		[SerializeField]
		private int m_firstOutputConnected = 0;

		[SerializeField]
		private Substance.Game.SubstanceGraph m_substanceGraph;
		[SerializeField]
		private string m_substanceGUID = string.Empty;

		[SerializeField]
		private int m_textureCoordSet = 0;

		[SerializeField]
		private ASEProceduralOutputType[] m_textureTypes;

		[SerializeField]
		private bool m_autoNormal = true;

		private System.Type m_type;

		private List<Texture2D> m_textures = new List<Texture2D>();

		private List<int> m_outputConns = new List<int>();

		private Rect m_previewArea;

		private Rect m_pickerArea;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT2, false, "UV" );
			AddOutputPort( WirePortDataType.COLOR, Constants.EmptyPortValue );
			m_insideSize.Set( TexturePreviewSizeX + PickerPreviewWidthAdjust, TexturePreviewSizeY + 10 );
			m_type = typeof( Substance.Game.Substance );
			m_currentParameterType = PropertyType.Property;
			m_freeType = false;
			m_freeName = false;
			m_autoWrapProperties = true;
			m_customPrefix = "Substance Sample ";
			m_drawPrecisionUI = false;
			m_showPreview = true;
			m_drawPreviewExpander = false;
			m_selectedLocation = PreviewLocation.TopCenter;
			m_cacheNodeConnections = new CacheNodeConnections();
			m_previewShaderGUID = "6f322c1da33f1e744941aafcb0ad1a2d";
			m_showAutoRegisterUI = false;
		}

		public override void RenderNodePreview()
		{
			if( !m_initialized )
				return;

			SetPreviewInputs();
			PreviewMaterial.SetInt( "_CustomUVs", m_inputPorts[ 0 ].IsConnected ? 1 : 0 );

			if( m_substanceGraph == null )
				return;

			List<Texture2D> texs = m_substanceGraph.generatedTextures;
			int count = m_outputPorts.Count;
			for( int i = 0; i < count; i++ )
			{
				RenderTexture temp = RenderTexture.active;
				RenderTexture.active = m_outputPorts[ i ].OutputPreviewTexture;

				PreviewMaterial.SetTexture( "_GenTex", texs[ i ] );

				if( m_autoNormal && m_textureTypes[ i ] == ASEProceduralOutputType.Normal )
					Graphics.Blit( null, m_outputPorts[ i ].OutputPreviewTexture, PreviewMaterial, 1 );
				else
					Graphics.Blit( null, m_outputPorts[ i ].OutputPreviewTexture, PreviewMaterial, 0 );
				RenderTexture.active = temp;
			}
		}

		public override void OnOutputPortConnected( int portId, int otherNodeId, int otherPortId )
		{
			base.OnOutputPortConnected( portId, otherNodeId, otherPortId );
			m_firstOutputConnected = -1;
		}

		public override void OnOutputPortDisconnected( int portId )
		{
			base.OnOutputPortDisconnected( portId );
			m_firstOutputConnected = -1;
		}

		void CalculateFirstOutputConnected()
		{
			m_outputConns.Clear();
			int count = m_outputPorts.Count;
			bool connectionsAvailable = false;
			for( int i = 0; i < count; i++ )
			{
				if( m_outputPorts[ i ].IsConnected )
				{
					connectionsAvailable = true;
				}
			}

			for( int i = 0; i < count; i++ )
			{
				if( connectionsAvailable )
				{
					if( m_outputPorts[ i ].IsConnected )
					{
						if( m_firstOutputConnected < 0 )
							m_firstOutputConnected = i;

						m_outputConns.Add( i );
					}
				}
				else
				{
					m_outputConns.Add( i );
				}
			}

			if( m_firstOutputConnected < 0 )
				m_firstOutputConnected = 0;
		}

		public override void OnNodeLayout( DrawInfo drawInfo )
		{
			base.OnNodeLayout( drawInfo );

			m_previewArea = m_remainingBox;
			m_previewArea.width = TexturePreviewSizeX * drawInfo.InvertedZoom;
			m_previewArea.height = TexturePreviewSizeY * drawInfo.InvertedZoom;
			m_previewArea.x += 0.5f * m_remainingBox.width - 0.5f * m_previewArea.width;
			m_pickerArea = m_previewArea;
			m_pickerArea.width = 40 * drawInfo.InvertedZoom;
			m_pickerArea.x = m_previewArea.xMax - m_pickerArea.width - 2;
			m_pickerArea.height = 14 * drawInfo.InvertedZoom;
			m_pickerArea.y = m_previewArea.yMax - m_pickerArea.height - 2;
		}

		public override void DrawGUIControls( DrawInfo drawInfo )
		{
			base.DrawGUIControls( drawInfo );

			if( !( drawInfo.CurrentEventType == EventType.MouseDown || drawInfo.CurrentEventType == EventType.MouseUp || drawInfo.CurrentEventType == EventType.ExecuteCommand || drawInfo.CurrentEventType == EventType.DragPerform ) )
				return;

			bool insideBox = m_previewArea.Contains( drawInfo.MousePosition );

			if( insideBox )
			{
				m_editing = true;
			}
			else if( m_editing && !insideBox && drawInfo.CurrentEventType != EventType.ExecuteCommand )
			{
				GUI.FocusControl( null );
				m_editing = false;
			}
		}


		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );

			if( m_editing )
			{
				m_textures = m_substanceGraph != null ? m_substanceGraph.generatedTextures : null;
				if( GUI.Button( m_pickerArea, string.Empty, GUIStyle.none ) )
				{
					int controlID = EditorGUIUtility.GetControlID( FocusType.Passive );
					EditorGUIUtility.ShowObjectPicker<SubstanceGraph>( m_substanceGraph, false, "", controlID );
				}

				string commandName = Event.current.commandName;
				UnityEngine.Object newValue = null;
				if( commandName == "ObjectSelectorUpdated" )
				{
					newValue = EditorGUIUtility.GetObjectPickerObject();
					if( newValue != (UnityEngine.Object)m_substanceGraph )
					{
						UndoRecordObject( "Changing value EditorGUIObjectField on node Substance Sample" );

						SubstanceGraph = newValue != null ? (SubstanceGraph)newValue : null;
						m_textures = m_substanceGraph != null ? m_substanceGraph.generatedTextures : null;
						OnNewSubstanceSelected( m_textures );
					}
				}
				else if( commandName == "ObjectSelectorClosed" )
				{
					newValue = EditorGUIUtility.GetObjectPickerObject();
					if( newValue != (UnityEngine.Object)m_substanceGraph )
					{
						UndoRecordObject( "Changing value EditorGUIObjectField on node Substance Sample" );

						SubstanceGraph = newValue != null ? (SubstanceGraph)newValue : null;
						m_textures = m_substanceGraph != null ? m_substanceGraph.generatedTextures : null;
						OnNewSubstanceSelected( m_textures );
					}
					m_editing = false;
				}

				if( GUI.Button( m_previewArea, string.Empty, GUIStyle.none ) )
				{
					if( m_substanceGraph != null )
					{
						Selection.activeObject = m_substanceGraph;
						EditorGUIUtility.PingObject( Selection.activeObject );
					}
					m_editing = false;
				}
			}

			if( drawInfo.CurrentEventType == EventType.Repaint )
			{
				if( !m_editing )
					m_textures = m_substanceGraph != null ? m_substanceGraph.generatedTextures : null;

				if( m_textures != null )
				{
					if( m_firstOutputConnected < 0 )
					{
						CalculateFirstOutputConnected();
					}
					else if( m_textures.Count != m_textureTypes.Length )
					{
						OnNewSubstanceSelected( m_textures );
					}

					int texCount = m_outputConns.Count;
					Rect individuals = m_previewArea;
					individuals.height /= texCount;

					for( int i = 0; i < texCount; i++ )
					{
						
						//EditorGUI.DrawPreviewTexture( individuals, m_textures[ m_outputConns[ i ] ], null, ScaleMode.ScaleAndCrop );
						EditorGUI.DrawPreviewTexture( individuals, m_outputPorts[ m_outputConns[ i ] ].OutputPreviewTexture, null, ScaleMode.ScaleAndCrop );
						individuals.y += individuals.height;
					}
				}
				else
				{
					GUI.Label( m_previewArea, string.Empty, UIUtils.ObjectFieldThumb );
				}

				if( ContainerGraph.LodLevel <= ParentGraph.NodeLOD.LOD2 )
				{
					Rect smallButton = m_previewArea;
					smallButton.height = 14 * drawInfo.InvertedZoom;
					smallButton.y = m_previewArea.yMax - smallButton.height - 2;
					smallButton.width = 40 * drawInfo.InvertedZoom;
					smallButton.x = m_previewArea.xMax - smallButton.width - 2;
					if( m_textures == null )
					{
						GUI.Label( m_previewArea, "None (Procedural Material)", UIUtils.ObjectFieldThumbOverlay );
					}
					GUI.Label( m_pickerArea, "Select", UIUtils.GetCustomStyle( CustomStyle.SamplerButton ) );
				}

				GUI.Label( m_previewArea, string.Empty, UIUtils.GetCustomStyle( CustomStyle.SamplerFrame ) );
			}
		}

		void OnNewSubstanceSelected( List<Texture2D> textures )
		{
			CacheCurrentSettings();
			ConfigPortsFromMaterial( true, textures );
			ConnectFromCache();
			m_requireMaterialUpdate = true;
			CalculateFirstOutputConnected();
			ContainerGraph.ParentWindow.RequestRepaint();
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUI.BeginChangeCheck();
			SubstanceGraph = EditorGUILayoutObjectField( SubstanceStr, m_substanceGraph, m_type, false ) as SubstanceGraph;
			if( EditorGUI.EndChangeCheck() )
			{
				List<Texture2D> textures = m_substanceGraph != null ? m_substanceGraph.generatedTextures : null;
				if( textures != null )
				{
					OnNewSubstanceSelected( textures );
				}
			}

			m_textureCoordSet = EditorGUILayoutIntPopup( Constants.AvailableUVSetsLabel, m_textureCoordSet, Constants.AvailableUVSetsStr, Constants.AvailableUVSets );
			EditorGUI.BeginChangeCheck();
			m_autoNormal = EditorGUILayoutToggle( AutoNormalStr, m_autoNormal );
			if( EditorGUI.EndChangeCheck() )
			{
				for( int i = 0; i < m_textureTypes.Length; i++ )
				{
					WirePortDataType portType = ( m_autoNormal && m_textureTypes[ i ] == ASEProceduralOutputType.Normal ) ? WirePortDataType.FLOAT3 : WirePortDataType.COLOR;
					if( m_outputPorts[ i ].DataType != portType )
					{
						m_outputPorts[ i ].ChangeType( portType, false );
					}
				}
			}
		}

		private void CacheCurrentSettings()
		{
			m_cacheNodeConnections.Clear();
			for( int portId = 0; portId < m_outputPorts.Count; portId++ )
			{
				if( m_outputPorts[ portId ].IsConnected )
				{
					int connCount = m_outputPorts[ portId ].ConnectionCount;
					for( int connIdx = 0; connIdx < connCount; connIdx++ )
					{
						WireReference connection = m_outputPorts[ portId ].GetConnection( connIdx );
						m_cacheNodeConnections.Add( m_outputPorts[ portId ].Name, new NodeCache( connection.NodeId, connection.PortId ) );
					}
				}
			}
		}

		private void ConnectFromCache()
		{
			for( int i = 0; i < m_outputPorts.Count; i++ )
			{
				List<NodeCache> connections = m_cacheNodeConnections.GetList( m_outputPorts[ i ].Name );
				if( connections != null )
				{
					int count = connections.Count;
					for( int connIdx = 0; connIdx < count; connIdx++ )
					{
						UIUtils.SetConnection( connections[ connIdx ].TargetNodeId, connections[ connIdx ].TargetPortId, UniqueId, i );
					}
				}
			}
		}


		private void ConfigPortsFromMaterial( bool invalidateConnections = false, List<Texture2D> newTextures = null )
		{
			SetAdditonalTitleText( ( m_substanceGraph != null ) ? string.Format( Constants.PropertyValueLabel, m_substanceGraph.name ) : "Value( <None> )" );

			List<Texture2D> textures = newTextures != null ? newTextures : ( ( m_substanceGraph != null ) ? m_substanceGraph.generatedTextures : null );
			if( textures != null )
			{
				m_firstOutputConnected = -1;
				string nameToRemove = m_substanceGraph.graphLabel + "_";
				m_textureTypes = new ASEProceduralOutputType[ textures.Count ];
				for( int i = 0; i < textures.Count; i++ )
				{
					//TODO: Replace for a more efficient test as soon as Laurent gives more infos
					bool isNormal = textures[ i ].format == TextureFormat.BC5 || textures[ i ].name.EndsWith( NormalMapCheck );
					m_textureTypes[ i ] = isNormal?ASEProceduralOutputType.Normal:ASEProceduralOutputType.Color;

					WirePortDataType portType = ( m_autoNormal && m_textureTypes[ i ] == ASEProceduralOutputType.Normal ) ? WirePortDataType.FLOAT3 : WirePortDataType.COLOR;
					string newName = textures[ i ].name.Replace( nameToRemove, string.Empty );
					char firstLetter = Char.ToUpper( newName[ 0 ] );
					newName = firstLetter.ToString() + newName.Substring( 1 );
					if( i < m_outputPorts.Count )
					{
						m_outputPorts[ i ].ChangeProperties( newName, portType, false );
						if( invalidateConnections )
						{
							m_outputPorts[ i ].FullDeleteConnections();
						}
					}
					else
					{
						AddOutputPort( portType, newName );
					}
				}

				if( textures.Count < m_outputPorts.Count )
				{
					int itemsToRemove = m_outputPorts.Count - textures.Count;
					for( int i = 0; i < itemsToRemove; i++ )
					{
						int idx = m_outputPorts.Count - 1;
						if( m_outputPorts[ idx ].IsConnected )
						{
							m_outputPorts[ idx ].ForceClearConnection();
						}
						RemoveOutputPort( idx );
					}
				}
			}
			else
			{
				int itemsToRemove = m_outputPorts.Count - 1;
				m_outputPorts[ 0 ].ChangeProperties( Constants.EmptyPortValue, WirePortDataType.COLOR, false );
				m_outputPorts[ 0 ].ForceClearConnection();

				for( int i = 0; i < itemsToRemove; i++ )
				{
					int idx = m_outputPorts.Count - 1;
					if( m_outputPorts[ idx ].IsConnected )
					{
						m_outputPorts[ idx ].ForceClearConnection();
					}
					RemoveOutputPort( idx );
				}
			}

			m_sizeIsDirty = true;
			m_isDirty = true;
		}

		private void ConfigFromObject( UnityEngine.Object obj )
		{
			SubstanceGraph newGraph = obj as SubstanceGraph;// AssetDatabase.LoadAssetAtPath<SubstanceGraph>( AssetDatabase.GetAssetPath( obj ) );
			if( newGraph != null )
			{
				SubstanceGraph = newGraph;
				ConfigPortsFromMaterial();
			}

			Substance.Game.Substance newSubstance = obj as Substance.Game.Substance;// AssetDatabase.LoadAssetAtPath<SubstanceGraph>( AssetDatabase.GetAssetPath( obj ) );
			if( newSubstance != null && newSubstance.graphs.Count > 0 )
			{
				SubstanceGraph = newSubstance.graphs[0];
				ConfigPortsFromMaterial();
			}

		}

		public override void OnObjectDropped( UnityEngine.Object obj )
		{
			ConfigFromObject( obj );
		}

		public override void SetupFromCastObject( UnityEngine.Object obj )
		{
			ConfigFromObject( obj );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_substanceGraph == null )
			{
				return "(0).xxxx";
			}

			if( m_outputPorts[ outputId ].IsLocalValue( dataCollector.PortCategory ) )
			{
				return m_outputPorts[ outputId ].LocalValue( dataCollector.PortCategory );
			}

			List<Texture2D> textures = m_substanceGraph.generatedTextures;

			string uvPropertyName = string.Empty;
			for( int i = 0; i < m_outputPorts.Count; i++ )
			{
				if( m_outputPorts[ i ].HasConnectedNode  )
				{
					uvPropertyName = UIUtils.GeneratePropertyName( textures[ i ].name , PropertyType.Property );
					break;
				}
			}

			string propertyName = UIUtils.GeneratePropertyName( textures[ outputId ].name, PropertyType.Property );
			string name = propertyName + OutputId;
			dataCollector.AddToUniforms( UniqueId, string.Format( GlobalVarDecStr, propertyName ) );
			dataCollector.AddToProperties( UniqueId, string.Format( PropertyDecStr, propertyName, textures[ outputId ].name ) + "{}", -1 );
			bool isVertex = ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation );
			string value = string.Format( "tex2D{0}({1}, {2})", ( isVertex ? "lod" : string.Empty ), propertyName, GetUVCoords( ref dataCollector, ignoreLocalvar, uvPropertyName ) );
			if( m_autoNormal && m_textureTypes[ outputId ] == ASEProceduralOutputType.Normal )
			{
				value = GeneratorUtils.GenerateUnpackNormalStr( ref dataCollector, CurrentPrecisionType, UniqueId, OutputId, value, false, "1.0",UnpackInputMode.Tangent );
			}

			dataCollector.AddPropertyNode( this );
			RegisterLocalVariable( outputId, value, ref dataCollector, name );

			return m_outputPorts[ outputId ].LocalValue( dataCollector.PortCategory );
		}

		public string GetUVCoords( ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar, string propertyName )
		{
			bool isVertex = ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation );
			if( m_inputPorts[ 0 ].IsConnected )
			{
				return m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, isVertex ? WirePortDataType.FLOAT4 : WirePortDataType.FLOAT2, ignoreLocalVar, true );
			}
			else
			{
				string uvChannelName = IOUtils.GetUVChannelName( propertyName, m_textureCoordSet );

				if( dataCollector.IsTemplate )
				{
					string propertyHelperVar = propertyName + "_ST";
					dataCollector.AddToUniforms( UniqueId, "float4", propertyHelperVar, dataCollector.IsSRP );
					string uvName = string.Empty;
					string result = string.Empty;
					if( dataCollector.TemplateDataCollectorInstance.GetCustomInterpolatedData( TemplateHelperFunctions.IntToUVChannelInfo[ m_textureCoordSet ], WirePortDataType.FLOAT4, PrecisionType.Float, ref result, false, dataCollector.PortCategory ) )
					{
						if( m_inputPorts[ 0 ].DataType != WirePortDataType.FLOAT4 )
							result += UIUtils.GetAutoSwizzle( m_inputPorts[ 0 ].DataType );
						uvName = result;
					}
					else
					if( dataCollector.TemplateDataCollectorInstance.HasUV( m_textureCoordSet ) )
					{
						uvName = dataCollector.TemplateDataCollectorInstance.GetUVName( m_textureCoordSet );
					}
					else
					{
						uvName = dataCollector.TemplateDataCollectorInstance.RegisterUV( m_textureCoordSet );
					}

					uvChannelName = "uv" + propertyName;
					if( isVertex )
					{
						string value = string.Format( Constants.TilingOffsetFormat, uvName, propertyHelperVar + ".xy", propertyHelperVar + ".zw" );
						string lodLevel = "0";

						value = "float4( " + value + ", 0 , " + lodLevel + " )";
						dataCollector.AddLocalVariable( UniqueId, m_currentPrecisionType, WirePortDataType.FLOAT4, uvChannelName, value );
					}
					else
					{
						dataCollector.AddLocalVariable( UniqueId, m_currentPrecisionType, WirePortDataType.FLOAT2, uvChannelName, string.Format( Constants.TilingOffsetFormat, uvName, propertyHelperVar + ".xy", propertyHelperVar + ".zw" ) );
					}
				}
				else
				{
					string vertexCoords = Constants.VertexShaderInputStr + ".texcoord";
					if( m_textureCoordSet > 0 )
					{
						vertexCoords += m_textureCoordSet.ToString();
					}


					string dummyPropUV = "_texcoord" + ( m_textureCoordSet > 0 ? ( m_textureCoordSet + 1 ).ToString() : "" );
					string dummyUV = "uv" + ( m_textureCoordSet > 0 ? ( m_textureCoordSet + 1 ).ToString() : "" ) + dummyPropUV;

					dataCollector.AddToUniforms( UniqueId, "uniform float4 " + propertyName + "_ST;" );
					dataCollector.AddToProperties( UniqueId, "[HideInInspector] " + dummyPropUV + "( \"\", 2D ) = \"white\" {}", 100 );
					dataCollector.AddToInput( UniqueId, dummyUV, WirePortDataType.FLOAT2 );

					if( isVertex )
					{
						dataCollector.AddToVertexLocalVariables( UniqueId, "float4 " + uvChannelName + " = float4(" + vertexCoords + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw, 0 ,0);" );
						return uvChannelName;
					}
					else
						dataCollector.AddToLocalVariables( UniqueId, PrecisionType.Float, WirePortDataType.FLOAT2, uvChannelName, Constants.InputVarStr + "." + dummyUV + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw" );

				}

				return uvChannelName;
			}
		}

		public override void UpdateMaterial( Material mat )
		{
			base.UpdateMaterial( mat );
			if( m_substanceGraph != null )
			{
				List<Texture2D> textures = m_substanceGraph.generatedTextures;
				for( int i = 0; i < textures.Count; i++ )
				{
					string textureName = UIUtils.GeneratePropertyName( textures[ i ].name, PropertyType.Property );
					if( mat.HasProperty( textureName ) && !InsideShaderFunction )
					{
						mat.SetTexture( textureName, textures[ i ] );
					}
				}
			}
		}

		public override bool UpdateShaderDefaults( ref Shader shader, ref TextureDefaultsDataColector defaultCol )
		{
			if( m_substanceGraph != null )
			{
				List<Texture2D> textures = m_substanceGraph.generatedTextures;
				for( int i = 0; i < textures.Count; i++ )
				{
					defaultCol.AddValue( UIUtils.GeneratePropertyName( textures[ i ].name, PropertyType.Property ), textures[ i ] );
				}
			}
			return true;
		}

		public override void OnNodeLogicUpdate( DrawInfo drawInfo )
		{
			base.OnNodeLogicUpdate( drawInfo );
			if( m_substanceGraph == null && !string.IsNullOrEmpty( m_substanceGUID ) )
			{
				SubstanceGraph = AssetDatabase.LoadAssetAtPath<SubstanceGraph>( AssetDatabase.GUIDToAssetPath( m_substanceGUID ) );
				if( m_substanceGraph == null )
				{
					m_substanceGUID = string.Empty;
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			m_textures = null;
			m_substanceGraph = null;
			m_substanceGUID = string.Empty;
			m_cacheNodeConnections.Clear();
			m_cacheNodeConnections = null;
			m_outputConns.Clear();
			m_outputConns = null;
		}

		public override string GetPropertyValStr()
		{
			return m_substanceGraph ? m_substanceGraph.name : string.Empty;
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			string guid = GetCurrentParam( ref nodeParams );
			m_textureCoordSet = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_autoNormal = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			if( guid.Length > 1 )
			{
				SubstanceGraph = AssetDatabase.LoadAssetAtPath<SubstanceGraph>( AssetDatabase.GUIDToAssetPath( guid ) );
				if( m_substanceGraph != null )
				{
					ConfigPortsFromMaterial();
				}
				else
				{
					UIUtils.ShowMessage( "Substance not found ", MessageSeverity.Error );
				}
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			string guid = ( m_substanceGraph != null ) ? AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath( m_substanceGraph ) ) : "0";
			IOUtils.AddFieldValueToString( ref nodeInfo, guid );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_textureCoordSet );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_autoNormal );
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			if( m_substanceGraph != null )
			{
				int count = m_outputPorts.Count;
				for( int i = 0; i < count; i++ )
				{
					if( m_autoNormal && m_textureTypes[ i ] == ASEProceduralOutputType.Normal )
						m_outputPorts[ i ].ChangeType( WirePortDataType.FLOAT3, false );
					else
						m_outputPorts[ i ].ChangeType( WirePortDataType.FLOAT4, false );
				}
			}
		}

		public SubstanceGraph SubstanceGraph
		{
			set
			{
				m_substanceGraph = value;
				if( value != null )
				{
					m_substanceGUID = AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath( value ) );
				}
				else
				{
					m_substanceGUID = string.Empty;
				}
			}
			get	{ return m_substanceGraph; }
		}
	}
#pragma warning restore 0618
}
#endif
