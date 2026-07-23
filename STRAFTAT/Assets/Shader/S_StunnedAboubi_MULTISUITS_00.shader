// Made with Amplify Shader Editor v1.9.1.7
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_StunnedAboubi_MULTISUITS_00"
{
	Properties
	{
		_ASEOutlineColor( "Outline Color", Color ) = (0.4094338,0.8569002,1,0)
		_ASEOutlineWidth( "Outline Width", Float ) = 0
		_Float0("Float 0", Float) = 100
		_BC("BC", 2D) = "white" {}
		_EmissiveColor("EmissiveColor", Color) = (0.5990566,0.8961561,1,0)
		_EmissiveBoost("EmissiveBoost", Float) = 1
		[Toggle(_STUNNED_ON)] _Stunned("Stunned", Float) = 0
		_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		[Toggle(_USE_TEX_ON)] _use_Tex("use_Tex", Float) = 1
		_Color0("Color 0", Color) = (0,0,0,0)
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		
		
		
		struct Input {
			half filler;
		};
		float4 _ASEOutlineColor;
		float _ASEOutlineWidth;
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += ( v.normal * _ASEOutlineWidth );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _ASEOutlineColor.rgb;
			o.Alpha = 1;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USE_TEX_ON
		#pragma shader_feature_local _STUNNED_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv2_texcoord2;
		};

		uniform float4 _Color0;
		uniform sampler2D _BC;
		uniform float4 _BC_ST;
		uniform float _Float0;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveBoost;
		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv1_BC = i.uv2_texcoord2 * _BC_ST.xy + _BC_ST.zw;
			float4 tex2DNode4 = tex2D( _BC, uv1_BC );
			#ifdef _USE_TEX_ON
				float4 staticSwitch17 = tex2DNode4;
			#else
				float4 staticSwitch17 = _Color0;
			#endif
			o.Albedo = staticSwitch17.rgb;
			float4 temp_cast_1 = (0.0).xxxx;
			float mulTime2 = _Time.y * _Float0;
			#ifdef _STUNNED_ON
				float4 staticSwitch13 = ( tex2DNode4 * ( ( saturate( sin( mulTime2 ) ) * _EmissiveColor ) * _EmissiveBoost ) );
			#else
				float4 staticSwitch13 = temp_cast_1;
			#endif
			o.Emission = staticSwitch13.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19107
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-98.5,85;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-711.0817,140.6772;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;11;-1053.082,75.67722;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-503.7399,140.5416;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-733.0888,364.6772;Inherit;False;Property;_EmissiveBoost;EmissiveBoost;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;7;-974.5333,339.3353;Inherit;False;Property;_EmissiveColor;EmissiveColor;2;0;Create;True;0;0;0;False;0;False;0.5990566,0.8961561,1,0;0.5990566,0.8961561,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;1;-1242.424,72.46423;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;2;-1434.424,72.14808;Inherit;True;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1604.45,67.17387;Inherit;False;Property;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;100;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;364.6646,-133.1614;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_StunnedAboubi_MULTISUITS_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;True;0;0.4094338,0.8569002,1,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.StaticSwitch;13;160.0729,77.62213;Inherit;False;Property;_Stunned;Stunned;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;14;65.07288,-59.37787;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-85.94641,-71.42914;Inherit;False;Property;_Metallic;Metallic;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-90.5,11;Inherit;False;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;-481.9464,-442.4291;Inherit;False;Property;_Color0;Color 0;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;17;-64.94641,-290.4291;Inherit;False;Property;_use_Tex;use_Tex;7;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;-766.6903,-141;Inherit;True;Property;_BC;BC;1;0;Create;True;0;0;0;False;0;False;-1;None;7d9813000dee583409fd5385e39fd7dd;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;12;0;4;0
WireConnection;12;1;8;0
WireConnection;6;0;11;0
WireConnection;6;1;7;0
WireConnection;11;0;1;0
WireConnection;8;0;6;0
WireConnection;8;1;9;0
WireConnection;1;0;2;0
WireConnection;2;0;3;0
WireConnection;0;0;17;0
WireConnection;0;2;13;0
WireConnection;0;3;15;0
WireConnection;0;4;10;0
WireConnection;13;1;14;0
WireConnection;13;0;12;0
WireConnection;17;1;18;0
WireConnection;17;0;4;0
ASEEND*/
//CHKSM=2DE5D2463673ACB7F23940CF1B285B46FE79997F