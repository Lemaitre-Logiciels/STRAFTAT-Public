// Upgrade NOTE: upgraded instancing buffer 'S_Foliage03_BCA' to new syntax.

// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_Foliage03_BCA"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_InstanceID("Instance ID", Float) = 0
		_sss_Strength("sss_Strength", Float) = 1
		_MainTex("MainTex", 2D) = "white" {}
		_SSSColor("SSSColor", Color) = (0,0,0,0)
		_DisplacementStrength("DisplacementStrength", Float) = 1
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "../CorgiFoliagePainter/Runtime/Shaders/Include/FoliageDisplacement.hlsl"
		#include "../CorgiFoliagePainter/Runtime/Shaders/Include/FoliageInstancingData.hlsl"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#include "Assets/CorgiFoliagePainter/Runtime/Shaders/Include/FoliageDisplacement.hlsl"
		#include "Assets/CorgiFoliagePainter/Runtime/Shaders/Include/FoliageInstancingData.hlsl"
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 uv2_texcoord2;
			float3 viewDir;
		};

		uniform float _DisplacementStrength;
		uniform sampler2D _MainTex;
		uniform float _sss_Strength;
		uniform float4 _SSSColor;
		uniform float _Cutoff = 0.5;

		UNITY_INSTANCING_BUFFER_START(S_Foliage03_BCA)
			UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
#define _MainTex_ST_arr S_Foliage03_BCA
			UNITY_DEFINE_INSTANCED_PROP(float, _InstanceID)
#define _InstanceID_arr S_Foliage03_BCA
		UNITY_INSTANCING_BUFFER_END(S_Foliage03_BCA)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float foliageDisplacementAmount1_g6 = _DisplacementStrength;
			float3 objToWorld6_g6 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float3 objectCenterPosition1_g6 = objToWorld6_g6;
			float localInjectPragmas10_g6 = ( 0.0 );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 In010_g6 = ase_vertex3Pos;
			{
			#pragma instancing_options assumeuniformscaling procedural:foliageSetup
			#pragma editor_sync_compilation
			Out = In;
			}
			float3 temp_cast_0 = (localInjectPragmas10_g6).xxx;
			float3 vertexPosition1_g6 = temp_cast_0;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 worldPosition1_g6 = ase_worldPos;
			float3 newWorldPosition1_g6 = float3( 0,0,0 );
			float offsetDistance1_g6 = 0.0;
			int localFoliageDisplacement1_g6 = FoliageDisplacement( foliageDisplacementAmount1_g6 , objectCenterPosition1_g6 , vertexPosition1_g6 , worldPosition1_g6 , newWorldPosition1_g6 , offsetDistance1_g6 );
			float3 worldToObj12_g6 = mul( unity_WorldToObject, float4( newWorldPosition1_g6, 1 ) ).xyz;
			float lerpResult75 = lerp( 0.0 , offsetDistance1_g6 , worldToObj12_g6.x);
			float3 temp_cast_2 = (lerpResult75).xxx;
			v.vertex.xyz += temp_cast_2;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_MainTex_ST_arr, _MainTex_ST);
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
			float4 tex2DNode2 = tex2D( _MainTex, uv_MainTex );
			float4 uvData1_g7 = i.uv2_texcoord2;
			float _InstanceID_Instance = UNITY_ACCESS_INSTANCED_PROP(_InstanceID_arr, _InstanceID);
			float instance_index1_g7 = _InstanceID_Instance;
			float4 localGetFoliageColor1_g7 = GetFoliageColor( uvData1_g7 , instance_index1_g7 );
			o.Albedo = ( tex2DNode2 * localGetFoliageColor1_g7 ).rgb;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir65 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float dotResult57 = dot( i.viewDir , -( ase_worldlightDir + ( objToWorldDir65 * _sss_Strength ) ) );
			float4 BC70 = tex2DNode2;
			float4 blendOpSrc66 = _SSSColor;
			float4 blendOpDest66 = BC70;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 sss69 = ( saturate( dotResult57 ) * ( ( saturate( (( blendOpDest66 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest66 ) * ( 1.0 - blendOpSrc66 ) ) : ( 2.0 * blendOpDest66 * blendOpSrc66 ) ) )) * ase_lightColor ) );
			o.Emission = sss69.rgb;
			float temp_output_3_0 = 0.0;
			o.Metallic = temp_output_3_0;
			o.Smoothness = temp_output_3_0;
			o.Alpha = 1;
			clip( tex2DNode2.a - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyzw = customInputData.uv2_texcoord2;
				o.customPack2.xyzw = v.texcoord1;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv2_texcoord2 = IN.customPack2.xyzw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;375.6093,-54.9;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_Foliage03_BCA;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;2;Include;;True;8c0679fb068c4e644b762e349fb9047f;Custom;False;0;0;;Include;;True;f98055ed079449a47b13681a612583e6;Custom;False;0;0;;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;3;87.58451,39.46099;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;31;-1964.042,1456.147;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1796.042,1456.147;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;33;-2192.464,1417.567;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;34;-1971.042,1572.147;Inherit;False;Property;_noisesize;noise size;7;0;Create;True;0;0;0;False;0;False;0;0.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;35;-1632.042,1455.147;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1812.042,1743.147;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2038.042,1718.147;Inherit;True;Property;_WindSpeed;WindSpeed;9;0;Create;True;0;0;0;False;0;False;0;0.09;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;38;-2036.042,1939.147;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;39;-1071.886,1435.982;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1246.714,1709.306;Inherit;False;Property;_NoisePower;NoisePower;10;0;Create;True;0;0;0;False;0;False;0;2.91;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-728.1324,1446.932;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-916.6334,1710.834;Inherit;False;Property;_NoiseIntensity;NoiseIntensity;11;0;Create;True;0;0;0;False;0;False;0;0.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-1512.75,1910.869;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1668.751,2016.169;Inherit;False;Constant;_Float2;Float 2;11;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;45;-1333.619,1900.478;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-1727.835,2209.179;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-1982.635,2307.979;Inherit;False;Property;_WindJitter;Wind Jitter;12;0;Create;True;0;0;0;False;0;False;1;0.29;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-380.7214,1453.714;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;50;-218.3718,1444.148;Inherit;False;WindNoise;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;21;79.9272,462.0179;Inherit;False;Property;_UseWind;UseWind;6;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;54;-1588.617,-642.9796;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SaturateNode;55;-1421.318,-1167.003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-1260.453,-1163.874;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;57;-1571.318,-1177.003;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;58;-1872.318,-1297.003;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NegateNode;59;-1772.207,-994.6743;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-1929.647,-992.3051;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;61;-2211.574,-1036.815;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-2144.946,-821.3763;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-2401.646,-724.9767;Inherit;False;Property;_sss_Strength;sss_Strength;3;0;Create;True;0;0;0;False;0;False;1;2.42;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;64;-2691.448,-926.0051;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;65;-2422.345,-923.4048;Inherit;False;Object;World;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BlendOpsNode;66;-1626.974,-772.266;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;67;-1914.768,-841.6833;Inherit;False;Property;_SSSColor;SSSColor;5;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5283019,0.5283019,0.5283019,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-1411.607,-765.6999;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;69;-1067.861,-1172.865;Inherit;False;sss;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-564.9016,-70.90528;Inherit;True;Property;_MainTex;MainTex;4;0;Create;True;0;0;0;False;0;False;-1;None;cd3bda10b0933804ea394d2ab7e20661;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-135.7619,-248.3186;Inherit;False;BC;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;-1858.98,-642.0764;Inherit;False;70;BC;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;-79.86129,-22.11944;Inherit;False;69;sss;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;47;-1116.857,1883.927;Inherit;True;Property;_TextureSample1;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;46;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;46;-1406.042,1424.147;Inherit;True;Property;_windNoise;windNoise;8;0;Create;True;0;0;0;False;0;False;-1;9d446fdc79d86274895f7d9352297658;9d446fdc79d86274895f7d9352297658;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;52;-116.9688,474.893;Inherit;False;50;WindNoise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;75;112.6072,267.8952;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-666.4662,286.8667;Inherit;False;Property;_DisplacementStrength;DisplacementStrength;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;79;-905.5889,152.6608;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-197.3462,-76.53165;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;82;-374.4473,259.8968;Inherit;False;CorgiDisplacement;-1;;6;3a0a36a95c1fd45419340f22e631abcd;0;1;13;FLOAT;0;False;2;FLOAT;2;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;83;-405.0914,148.7607;Inherit;False;GetFoliageColor;1;;7;9f2d85894a998c34aa514bb70feb6b90;0;1;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
WireConnection;0;0;80;0
WireConnection;0;2;71;0
WireConnection;0;3;3;0
WireConnection;0;4;3;0
WireConnection;0;10;2;4
WireConnection;0;11;75;0
WireConnection;31;0;33;1
WireConnection;31;1;33;3
WireConnection;32;0;31;0
WireConnection;32;1;34;0
WireConnection;35;0;32;0
WireConnection;35;1;36;0
WireConnection;36;0;37;0
WireConnection;36;1;38;0
WireConnection;39;0;46;0
WireConnection;39;1;40;0
WireConnection;41;0;39;0
WireConnection;41;1;42;0
WireConnection;43;0;32;0
WireConnection;43;1;44;0
WireConnection;45;0;43;0
WireConnection;45;1;48;0
WireConnection;48;0;38;0
WireConnection;48;1;49;0
WireConnection;51;0;41;0
WireConnection;51;1;47;1
WireConnection;50;0;51;0
WireConnection;21;0;52;0
WireConnection;55;0;57;0
WireConnection;56;0;55;0
WireConnection;56;1;68;0
WireConnection;57;0;58;0
WireConnection;57;1;59;0
WireConnection;59;0;60;0
WireConnection;60;0;61;0
WireConnection;60;1;62;0
WireConnection;62;0;65;0
WireConnection;62;1;63;0
WireConnection;65;0;64;0
WireConnection;66;0;67;0
WireConnection;66;1;53;0
WireConnection;68;0;66;0
WireConnection;68;1;54;0
WireConnection;69;0;56;0
WireConnection;70;0;2;0
WireConnection;47;1;45;0
WireConnection;46;1;35;0
WireConnection;75;1;82;2
WireConnection;75;2;82;0
WireConnection;80;0;2;0
WireConnection;80;1;83;0
WireConnection;82;13;76;0
WireConnection;83;2;79;0
ASEEND*/
//CHKSM=4BF8D32FED65E845D29F5444401FDCEA2AD135FA