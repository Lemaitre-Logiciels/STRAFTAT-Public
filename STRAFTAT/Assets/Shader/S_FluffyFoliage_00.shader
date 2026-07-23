// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_FluffyFoliage_00"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.78
		_fluffyscale("fluffyscale", Float) = 0
		_sss_Strength("sss_Strength", Float) = 1
		_SSSColor("SSSColor", Color) = (0,0,0,0)
		_Tex("Tex", 2D) = "white" {}
		_noisesize("noise size", Float) = 0
		_windNoise("windNoise", 2D) = "white" {}
		_WindSpeed("WindSpeed", Float) = 0
		_NoisePower("NoisePower", Float) = 0
		_NoiseIntensity("NoiseIntensity", Float) = 0
		_WindJitter("Wind Jitter", Float) = 1
		[Toggle(_USESSS_ON)] _UseSSS("UseSSS", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USESSS_ON
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _fluffyscale;
		uniform sampler2D _windNoise;
		uniform float _WindSpeed;
		uniform float _noisesize;
		uniform float _NoisePower;
		uniform float _NoiseIntensity;
		uniform float _WindJitter;
		uniform sampler2D _Tex;
		uniform float4 _Tex_ST;
		uniform float _sss_Strength;
		uniform float4 _SSSColor;
		uniform float _Cutoff = 0.78;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 temp_cast_0 = (-0.5).xx;
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
			half tangentSign = v.tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
			float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * tangentSign;
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float3 tangentTobjectDir7 = normalize( mul( unity_WorldToObject, float4( mul( ase_tangentToWorldFast, float3( (temp_cast_0 + (v.texcoord.xy - float2( 0,0 )) * (float2( 1,1 ) - temp_cast_0) / (float2( 1,1 ) - float2( 0,0 ))) ,  0.0 ) ), 0 ) ).xyz );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult39 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 temp_output_40_0 = ( appendResult39 * _noisesize );
			float2 panner43 = ( ( _WindSpeed * _Time.y ) * float2( 1,1 ) + temp_output_40_0);
			float4 temp_cast_4 = (_NoisePower).xxxx;
			float2 panner60 = ( ( _Time.y * _WindJitter ) * float2( 1,1 ) + ( temp_output_40_0 * 2.0 ));
			float4 WindNoise47 = ( ( pow( tex2Dlod( _windNoise, float4( panner43, 0, 0.0) ) , temp_cast_4 ) * _NoiseIntensity ) * tex2Dlod( _windNoise, float4( panner60, 0, 0.0) ).r );
			float4 QuadScatter8 = ( float4( ( tangentTobjectDir7 * _fluffyscale ) , 0.0 ) + ( float4( ase_vertexNormal , 0.0 ) * WindNoise47 ) );
			v.vertex.xyz += QuadScatter8.rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Tex = i.uv_texcoord * _Tex_ST.xy + _Tex_ST.zw;
			float4 tex2DNode28 = tex2D( _Tex, uv_Tex );
			float4 BC31 = tex2DNode28;
			o.Albedo = BC31.rgb;
			float4 temp_cast_1 = 0;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir23 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float dotResult15 = dot( ase_worldViewDir , -( ase_worldlightDir + ( objToWorldDir23 * _sss_Strength ) ) );
			float4 blendOpSrc30 = _SSSColor;
			float4 blendOpDest30 = BC31;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 sss17 = ( saturate( dotResult15 ) * ( ( saturate( (( blendOpDest30 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest30 ) * ( 1.0 - blendOpSrc30 ) ) : ( 2.0 * blendOpDest30 * blendOpSrc30 ) ) )) * ase_lightColor ) );
			#ifdef _USESSS_ON
				float4 staticSwitch68 = sss17;
			#else
				float4 staticSwitch68 = temp_cast_1;
			#endif
			o.Emission = staticSwitch68.rgb;
			float temp_output_57_0 = 0.0;
			o.Metallic = temp_output_57_0;
			o.Smoothness = temp_output_57_0;
			float opacity66 = tex2DNode28.a;
			float temp_output_67_0 = opacity66;
			o.Alpha = temp_output_67_0;
			clip( temp_output_67_0 - _Cutoff );
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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
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
Node;AmplifyShaderEditor.CommentaryNode;37;-3253.603,-41.38728;Inherit;False;1897.588;863.0237;Comment;17;16;26;15;13;19;22;14;20;21;24;23;30;27;34;35;36;17;sss;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;9;-1857.517,1048.077;Inherit;False;1901.812;511.6927;QuadScatter;10;48;8;6;1;3;2;7;5;50;51;;1,1,1,1;0;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;284.4131,-71.25485;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_FluffyFoliage_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.78;True;True;0;True;TransparentCutout;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-893.6041,1226.185;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;7;-1269.436,1223.255;Inherit;False;Tangent;Object;True;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;2;-1559.412,1224.232;Inherit;True;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;0,0;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1750.752,1378.767;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-1807.517,1152.373;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-962.4116,1369.366;Inherit;False;Property;_fluffyscale;fluffyscale;1;0;Create;True;0;0;0;False;0;False;0;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;32;42.71999,-142.9055;Inherit;False;31;BC;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;16;-1933.473,138.6127;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-1772.608,141.7425;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;15;-2083.473,128.6127;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;13;-2384.473,8.612717;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NegateNode;19;-2284.362,310.9417;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-2441.802,313.3109;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;14;-2723.729,268.8005;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-2657.101,484.2396;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-2913.801,580.6393;Inherit;False;Property;_sss_Strength;sss_Strength;2;0;Create;True;0;0;0;False;0;False;1;-1.65;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;24;-3203.603,379.6109;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;23;-2934.5,382.2112;Inherit;False;Object;World;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BlendOpsNode;30;-2139.129,533.35;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;27;-2426.923,463.9326;Inherit;False;Property;_SSSColor;SSSColor;3;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.2924528,0.2924528,0.2924528,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;34;-2371.135,663.5396;Inherit;False;31;BC;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1923.762,539.9161;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;36;-2100.772,662.6364;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;17;-1580.016,132.751;Inherit;False;sss;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;28;-1866.258,-824.067;Inherit;True;Property;_Tex;Tex;4;0;Create;True;0;0;0;False;0;False;-1;None;bda75e2a29e76b94ba57bbd167b8d0ab;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-1520.977,-815.3484;Inherit;False;BC;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;39;-1535.861,2218.118;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-1367.861,2218.118;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;38;-1764.282,2179.538;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;41;-1542.861,2334.118;Inherit;False;Property;_noisesize;noise size;5;0;Create;True;0;0;0;False;0;False;0;0.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;43;-1203.861,2217.118;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-1383.861,2505.118;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1609.861,2480.118;Inherit;True;Property;_WindSpeed;WindSpeed;7;0;Create;True;0;0;0;False;0;False;0;0.09;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;46;-1607.861,2701.118;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-423.0936,1352.051;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-704.6231,1469.673;Inherit;False;47;WindNoise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;50;-702.0936,1325.051;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;8;-153.2045,1228.384;Inherit;False;QuadScatter;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-274.0936,1236.051;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;52;-643.7058,2197.953;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-818.5333,2471.277;Inherit;False;Property;_NoisePower;NoisePower;8;0;Create;True;0;0;0;False;0;False;0;2.91;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-299.9517,2208.903;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-488.4528,2472.805;Inherit;False;Property;_NoiseIntensity;NoiseIntensity;9;0;Create;True;0;0;0;False;0;False;0;0.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;116.5594,74.3912;Inherit;False;Constant;_Float1;Float 1;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-1084.569,2672.84;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-1240.57,2778.14;Inherit;False;Constant;_Float2;Float 2;11;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;60;-905.4379,2662.449;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;42;-977.8606,2186.118;Inherit;True;Property;_windNoise;windNoise;6;0;Create;True;0;0;0;False;0;False;-1;None;9d446fdc79d86274895f7d9352297658;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;61;-688.6758,2645.898;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;42;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-1299.654,2971.15;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-1554.454,3069.95;Inherit;False;Property;_WindJitter;Wind Jitter;10;0;Create;True;0;0;0;False;0;False;1;0.29;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;209.8087,2206.119;Inherit;False;WindNoise;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;47.45935,2215.685;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;66;-1516.863,-651.8977;Inherit;False;opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;10;-39.37053,304.0988;Inherit;False;8;QuadScatter;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-62.70874,177.5316;Inherit;False;66;opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;18;-235.5743,51.19548;Inherit;False;17;sss;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.IntNode;69;-193.9113,-53.57465;Inherit;False;Constant;_Int0;Int 0;12;0;Create;True;0;0;0;False;0;False;0;0;False;0;1;INT;0
Node;AmplifyShaderEditor.StaticSwitch;68;-7.911255,-44.57465;Inherit;False;Property;_UseSSS;UseSSS;11;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
WireConnection;0;0;32;0
WireConnection;0;2;68;0
WireConnection;0;3;57;0
WireConnection;0;4;57;0
WireConnection;0;9;67;0
WireConnection;0;10;67;0
WireConnection;0;11;10;0
WireConnection;5;0;7;0
WireConnection;5;1;6;0
WireConnection;7;0;2;0
WireConnection;2;0;1;0
WireConnection;2;3;3;0
WireConnection;16;0;15;0
WireConnection;26;0;16;0
WireConnection;26;1;35;0
WireConnection;15;0;13;0
WireConnection;15;1;19;0
WireConnection;19;0;22;0
WireConnection;22;0;14;0
WireConnection;22;1;20;0
WireConnection;20;0;23;0
WireConnection;20;1;21;0
WireConnection;23;0;24;0
WireConnection;30;0;27;0
WireConnection;30;1;34;0
WireConnection;35;0;30;0
WireConnection;35;1;36;0
WireConnection;17;0;26;0
WireConnection;31;0;28;0
WireConnection;39;0;38;1
WireConnection;39;1;38;3
WireConnection;40;0;39;0
WireConnection;40;1;41;0
WireConnection;43;0;40;0
WireConnection;43;1;45;0
WireConnection;45;0;44;0
WireConnection;45;1;46;0
WireConnection;49;0;50;0
WireConnection;49;1;48;0
WireConnection;8;0;51;0
WireConnection;51;0;5;0
WireConnection;51;1;49;0
WireConnection;52;0;42;0
WireConnection;52;1;53;0
WireConnection;54;0;52;0
WireConnection;54;1;55;0
WireConnection;58;0;40;0
WireConnection;58;1;59;0
WireConnection;60;0;58;0
WireConnection;60;1;62;0
WireConnection;42;1;43;0
WireConnection;61;1;60;0
WireConnection;62;0;46;0
WireConnection;62;1;63;0
WireConnection;47;0;65;0
WireConnection;65;0;54;0
WireConnection;65;1;61;1
WireConnection;66;0;28;4
WireConnection;68;1;69;0
WireConnection;68;0;18;0
ASEEND*/
//CHKSM=39C8A046789FB265EF25FE2C1289BF2498DFD794