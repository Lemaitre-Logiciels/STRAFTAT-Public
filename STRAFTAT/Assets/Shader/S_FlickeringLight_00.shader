// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_FlickeringLight_00"
{
	Properties
	{
		_EmissiveBoost("EmissiveBoost", Float) = 1
		_Color0("Color 0", Color) = (1,1,1,0)
		_DirtTex("DirtTex", 2D) = "white" {}
		_EmissionMap("EmissionMap", 2D) = "white" {}
		_Power("Power", Float) = 0
		_Float0("Float 0", Float) = 0
		_tilingdirt("tiling dirt", Float) = 0
		_NoiseValue("NoiseValue", Float) = 0
		_Float1("Float 1", Float) = 1
		[Toggle(_FLICKER_ON)] _Flicker("Flicker", Float) = 0
		_Tint("Tint", Color) = (0,0,0,0)
		[Toggle(_USETINT_ON)] _UseTint("UseTint", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USETINT_ON
		#pragma shader_feature_local _FLICKER_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _EmissionMap;
		uniform float _tilingdirt;
		uniform float4 _Tint;
		uniform float4 _Color0;
		uniform sampler2D _DirtTex;
		uniform float _Float0;
		uniform float _Power;
		uniform float _NoiseValue;
		uniform float _EmissiveBoost;
		uniform float _Float1;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 tex2DNode15 = tex2D( _EmissionMap, ( _tilingdirt * i.uv_texcoord ) );
			#ifdef _USETINT_ON
				float4 staticSwitch31 = ( _Tint * tex2DNode15 );
			#else
				float4 staticSwitch31 = tex2DNode15;
			#endif
			o.Albedo = staticSwitch31.rgb;
			float4 temp_cast_1 = (_Power).xxxx;
			float mulTime21 = _Time.y * sin( _Float1 );
			float2 temp_cast_2 = (mulTime21).xx;
			float dotResult4_g1 = dot( temp_cast_2 , float2( 12.9898,78.233 ) );
			float lerpResult10_g1 = lerp( 0.1 , 1.0 , frac( ( sin( dotResult4_g1 ) * 43758.55 ) ));
			#ifdef _FLICKER_ON
				float staticSwitch27 = lerpResult10_g1;
			#else
				float staticSwitch27 = 1.0;
			#endif
			o.Emission = ( ( ( ( _Color0 * saturate( ( pow( tex2D( _DirtTex, ( _Float0 * i.uv_texcoord ) ) , temp_cast_1 ) + _NoiseValue ) ) ) * _EmissiveBoost ) * staticSwitch31 ) * staticSwitch27 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.RangedFloatNode;8;-1834,13.5;Float;False;Property;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1877,154.5;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-1617,108.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1089.197,197.1097;Float;False;Property;_Power;Power;4;0;Create;True;0;0;0;False;0;False;0;0.48;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1419,78.5;Inherit;True;Property;_DirtTex;DirtTex;2;0;Create;True;0;0;0;False;0;False;-1;None;50c7a81bd0ae4d8438bcbbcdc40cf3b1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;6;-1021.197,82.10973;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-927.4977,207.4971;Float;False;Property;_NoiseValue;NoiseValue;7;0;Create;True;0;0;0;False;0;False;0;-0.27;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-788.0853,82.3547;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1391.477,-513.8207;Float;False;Property;_tilingdirt;tiling dirt;6;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;13;-651.9645,79.06137;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1;-793,-191.5;Float;False;Property;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-1434.477,-372.8208;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-410,228.5;Float;False;Property;_EmissiveBoost;EmissiveBoost;0;0;Create;True;0;0;0;False;0;False;1;2.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-475,7.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1174.477,-418.8208;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-226,87.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-12.77489,72.59994;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;275.3546,-1.397739;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_FlickeringLight_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;15;-859.8044,-490.6888;Inherit;True;Property;_EmissionMap;EmissionMap;3;0;Create;True;0;0;0;False;0;False;-1;None;6d8074ee592d5944fb15a3bec186e6e7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;119.7272,192.1136;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;20;-246.0112,435.4991;Inherit;False;Random Range;-1;;1;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;21;-705.9415,449.3906;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;22;-934.5722,432.2685;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1145.074,430.2538;Inherit;False;Property;_Float1;Float 1;8;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;27;-42.98901,351.8931;Inherit;False;Property;_Flicker;Flicker;9;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-220.989,349.8931;Inherit;False;Constant;_Float4;Float 4;10;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-439.2377,672.4035;Inherit;False;Constant;_Float2;Float 2;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-452.3315,600.8862;Inherit;False;Constant;_Float3;Float 2;9;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-454.4871,-401.6331;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;31;-209.7584,-455.3319;Inherit;False;Property;_UseTint;UseTint;11;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;30;-773.5726,-705.1466;Inherit;False;Property;_Tint;Tint;10;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;10;0;8;0
WireConnection;10;1;9;0
WireConnection;4;1;10;0
WireConnection;6;0;4;0
WireConnection;6;1;7;0
WireConnection;11;0;6;0
WireConnection;11;1;12;0
WireConnection;13;0;11;0
WireConnection;5;0;1;0
WireConnection;5;1;13;0
WireConnection;19;0;18;0
WireConnection;19;1;17;0
WireConnection;2;0;5;0
WireConnection;2;1;3;0
WireConnection;16;0;2;0
WireConnection;16;1;31;0
WireConnection;0;0;31;0
WireConnection;0;2;26;0
WireConnection;15;1;19;0
WireConnection;26;0;16;0
WireConnection;26;1;27;0
WireConnection;20;1;21;0
WireConnection;20;2;25;0
WireConnection;20;3;24;0
WireConnection;21;0;22;0
WireConnection;22;0;23;0
WireConnection;27;1;28;0
WireConnection;27;0;20;0
WireConnection;29;0;30;0
WireConnection;29;1;15;0
WireConnection;31;1;15;0
WireConnection;31;0;29;0
ASEEND*/
//CHKSM=3E74701161F6F5A078CA125DD47E6684F75CBF8A