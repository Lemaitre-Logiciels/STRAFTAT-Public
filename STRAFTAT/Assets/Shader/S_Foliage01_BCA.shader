// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_Foliage01_BCA"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_VertexPosgradient("VertexPos gradient", Float) = 1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[Toggle(_USEWIND_ON)] _UseWind("UseWind", Float) = 0
		_WindStrenght("WindStrenght", Float) = 0
		_NoiseTiling("NoiseTiling", Float) = 0.01
		_pannerspeed("panner speed", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEWIND_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample0;
		uniform float2 _pannerspeed;
		uniform float _NoiseTiling;
		uniform float _VertexPosgradient;
		uniform float _WindStrenght;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 panner22 = ( 1.0 * _Time.y * _pannerspeed + ( ase_worldPos * _NoiseTiling ).xy);
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
			float3x3 tangentToWorld = CreateTangentToWorldPerVertex( ase_worldNormal, ase_worldTangent, v.tangent.w );
			float3 tangentNormal19 = ase_vertexNormal;
			float3 modWorldNormal19 = (tangentToWorld[0] * tangentNormal19.x + tangentToWorld[1] * tangentNormal19.y + tangentToWorld[2] * tangentNormal19.z);
			float3 appendResult14 = (float3(modWorldNormal19.x , 0.0 , modWorldNormal19.z));
			float3 ase_vertex3Pos = v.vertex.xyz;
			#ifdef _USEWIND_ON
				float4 staticSwitch21 = ( ( ( tex2Dlod( _TextureSample0, float4( panner22, 0, 0.0) ) * float4( appendResult14 , 0.0 ) ) * ( ase_vertex3Pos.y * _VertexPosgradient ) ) * _WindStrenght );
			#else
				float4 staticSwitch21 = float4( 0,0,0,0 );
			#endif
			v.vertex.xyz += staticSwitch21.rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode2 = tex2D( _MainTex, uv_MainTex );
			o.Albedo = tex2DNode2.rgb;
			float temp_output_3_0 = 0.0;
			o.Metallic = temp_output_3_0;
			o.Smoothness = temp_output_3_0;
			o.Alpha = 1;
			clip( tex2DNode2.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.DynamicAppendNode;14;-647.5964,566.5603;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-469.6469,494.4123;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;375.6093,-54.9;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_Foliage01_BCA;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SinTimeNode;5;-1615.793,417.9124;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;22;-1624.738,180.5132;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;20;-1263.637,259.3478;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;9d446fdc79d86274895f7d9352297658;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-650.6594,802.5912;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;6;-1114.306,752.5417;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;19;-924.7957,541.063;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;12;-1179.87,538.8394;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-274.0115,493.4292;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-112.8684,504.1809;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;21;50.88118,506.7969;Inherit;False;Property;_UseWind;UseWind;5;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-225.8461,649.2943;Inherit;False;Property;_WindStrenght;WindStrenght;6;0;Create;True;0;0;0;False;0;False;0;0.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;87.58451,39.46099;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-415.4016,-55.30527;Inherit;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;0;False;0;False;-1;None;cd3bda10b0933804ea394d2ab7e20661;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-756.2159,95.73437;Inherit;True;Property;_OpacityMask;OpacityMask;0;0;Create;True;0;0;0;False;0;False;-1;None;4a0c84ea3e3612040a2be1758104e755;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-888.0331,868.3574;Inherit;False;Property;_VertexPosgradient;VertexPos gradient;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;24;-2504.221,72.12331;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-2089.021,80.31799;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-2299.89,291.3803;Inherit;False;Property;_NoiseTiling;NoiseTiling;7;0;Create;True;0;0;0;False;0;False;0.01;2.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;30;-1959.137,306.3478;Inherit;False;Property;_pannerspeed;panner speed;8;0;Create;True;0;0;0;False;0;False;0,0;0.01,0.14;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
WireConnection;14;0;19;1
WireConnection;14;2;19;3
WireConnection;11;0;20;0
WireConnection;11;1;14;0
WireConnection;0;0;2;0
WireConnection;0;3;3;0
WireConnection;0;4;3;0
WireConnection;0;10;2;4
WireConnection;0;11;21;0
WireConnection;22;0;26;0
WireConnection;22;2;30;0
WireConnection;20;1;22;0
WireConnection;8;0;6;2
WireConnection;8;1;10;0
WireConnection;19;0;12;0
WireConnection;15;0;11;0
WireConnection;15;1;8;0
WireConnection;16;0;15;0
WireConnection;16;1;29;0
WireConnection;21;0;16;0
WireConnection;26;0;24;0
WireConnection;26;1;27;0
ASEEND*/
//CHKSM=1F24CE7170BD52760E3BFB7A0A2B8D43A2F16CAA