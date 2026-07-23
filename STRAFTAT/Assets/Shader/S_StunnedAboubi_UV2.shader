// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_StunnedAboubi_UV2"
{
	Properties
	{
		_Float0("Float 0", Float) = 100
		_BC("BC", 2D) = "white" {}
		_EmissiveColor("EmissiveColor", Color) = (0.5990566,0.8961561,1,0)
		_EmissiveBoost("EmissiveBoost", Float) = 1
		[Toggle(_STUNNED_ON)] _Stunned("Stunned", Float) = 0
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _STUNNED_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv3_texcoord3;
		};

		uniform sampler2D _BC;
		uniform float4 _BC_ST;
		uniform float _Float0;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveBoost;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv2_BC = i.uv3_texcoord3 * _BC_ST.xy + _BC_ST.zw;
			float4 tex2DNode4 = tex2D( _BC, uv2_BC );
			o.Albedo = tex2DNode4.rgb;
			float4 temp_cast_1 = (0.0).xxxx;
			float mulTime2 = _Time.y * _Float0;
			#ifdef _STUNNED_ON
				float4 staticSwitch13 = ( tex2DNode4 * ( ( saturate( sin( mulTime2 ) ) * _EmissiveColor ) * _EmissiveBoost ) );
			#else
				float4 staticSwitch13 = temp_cast_1;
			#endif
			o.Emission = staticSwitch13.rgb;
			float temp_output_10_0 = 0.0;
			o.Metallic = temp_output_10_0;
			o.Smoothness = temp_output_10_0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.RangedFloatNode;10;-69.5,-6;Inherit;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-98.5,85;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-711.0817,140.6772;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;11;-1053.082,75.67722;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-503.7399,140.5416;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-733.0888,364.6772;Inherit;False;Property;_EmissiveBoost;EmissiveBoost;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;7;-974.5333,339.3353;Inherit;False;Property;_EmissiveColor;EmissiveColor;2;0;Create;True;0;0;0;False;0;False;0.5990566,0.8961561,1,0;0.5990566,0.8961561,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;1;-1242.424,72.46423;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;2;-1434.424,72.14808;Inherit;True;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1604.45,67.17387;Inherit;False;Property;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;100;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;364.6646,-133.1614;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_StunnedAboubi_UV2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.StaticSwitch;13;160.0729,77.62213;Inherit;False;Property;_Stunned;Stunned;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;14;65.07288,-59.37787;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-768.5,-141;Inherit;True;Property;_BC;BC;1;0;Create;True;0;0;0;False;0;False;-1;None;a5e97cde7ea5da54bbbf2fc0c0f76156;True;2;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;12;0;4;0
WireConnection;12;1;8;0
WireConnection;6;0;11;0
WireConnection;6;1;7;0
WireConnection;11;0;1;0
WireConnection;8;0;6;0
WireConnection;8;1;9;0
WireConnection;1;0;2;0
WireConnection;2;0;3;0
WireConnection;0;0;4;0
WireConnection;0;2;13;0
WireConnection;0;3;10;0
WireConnection;0;4;10;0
WireConnection;13;1;14;0
WireConnection;13;0;12;0
ASEEND*/
//CHKSM=C094EC428FC9D6F6BCF284985256FC9AD4087403