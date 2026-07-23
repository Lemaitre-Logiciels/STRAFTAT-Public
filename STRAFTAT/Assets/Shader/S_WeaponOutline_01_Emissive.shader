// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_WeaponOutline_01_Emissive"
{
	Properties
	{
		_BC("BC", 2D) = "white" {}
		_MS("MS", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Color_Outline("Color_Outline", Color) = (1,0.8196079,0.427451,0)
		_OutlineWidth("OutlineWidth", Float) = 0
		[Toggle(_USEOUTLINE_ON)] _UseOutline("UseOutline", Float) = 0
		_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		[Toggle(_USEMETALLIC_ON)] _UseMetallic("UseMetallic", Float) = 1
		[Toggle(_USESMOOTHNESS_ON)] _UseSmoothness("UseSmoothness", Float) = 1
		[Toggle(_USENORMAL_ON)] _UseNormal("UseNormal", Float) = 1
		_Emissive("Emissive", 2D) = "white" {}
		_EmissiveBoost("EmissiveBoost", Float) = 1
		_timeglow("timeglow", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		
		
		
		struct Input
		{
			half filler;
		};
		uniform float4 _Color_Outline;
		uniform float _OutlineWidth;
		
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float outlineVar = _OutlineWidth;
			v.vertex.xyz += ( v.normal * outlineVar );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _Color_Outline.rgb;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEOUTLINE_ON
		#pragma shader_feature_local _USENORMAL_ON
		#pragma shader_feature_local _USEMETALLIC_ON
		#pragma shader_feature_local _USESMOOTHNESS_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _BC;
		uniform float4 _BC_ST;
		uniform float _timeglow;
		uniform sampler2D _Emissive;
		uniform float4 _Emissive_ST;
		uniform float _EmissiveBoost;
		uniform float _Metallic;
		uniform sampler2D _MS;
		uniform float4 _MS_ST;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 temp_cast_0 = (0.0).xxx;
			#ifdef _USEOUTLINE_ON
				float3 staticSwitch8 = 0;
			#else
				float3 staticSwitch8 = temp_cast_0;
			#endif
			v.vertex.xyz += staticSwitch8;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color19 = IsGammaSpace() ? float4(0,0,1,0) : float4(0,0,1,0);
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			#ifdef _USENORMAL_ON
				float4 staticSwitch14 = float4( UnpackNormal( tex2D( _Normal, uv_Normal ) ) , 0.0 );
			#else
				float4 staticSwitch14 = color19;
			#endif
			o.Normal = staticSwitch14.rgb;
			float2 uv_BC = i.uv_texcoord * _BC_ST.xy + _BC_ST.zw;
			o.Albedo = tex2D( _BC, uv_BC ).rgb;
			float mulTime29 = _Time.y * _timeglow;
			float2 panner30 = ( sin( mulTime29 ) * float2( 0,1 ) + float2( 0,0 ));
			float2 uv_Emissive = i.uv_texcoord * _Emissive_ST.xy + _Emissive_ST.zw;
			o.Emission = ( float4( ( float2( 0,0 ) + ( ( pow( i.uv_texcoord.x , 6.88 ) * 0.5 ) * panner30 ) ), 0.0 , 0.0 ) * ( tex2D( _Emissive, uv_Emissive ) * _EmissiveBoost ) ).rgb;
			float4 temp_cast_5 = (_Metallic).xxxx;
			float2 uv_MS = i.uv_texcoord * _MS_ST.xy + _MS_ST.zw;
			float4 tex2DNode2 = tex2D( _MS, uv_MS );
			#ifdef _USEMETALLIC_ON
				float4 staticSwitch12 = tex2DNode2;
			#else
				float4 staticSwitch12 = temp_cast_5;
			#endif
			o.Metallic = staticSwitch12.r;
			#ifdef _USESMOOTHNESS_ON
				float staticSwitch13 = tex2DNode2.a;
			#else
				float staticSwitch13 = _Smoothness;
			#endif
			o.Smoothness = staticSwitch13;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_WeaponOutline_01_Emissive;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.StaticSwitch;8;-405.959,715.6813;Inherit;False;Property;_UseOutline;UseOutline;5;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OutlineNode;4;-693.5688,799.6254;Inherit;False;0;False;None;0;0;Front;True;True;True;True;0;False;;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-662.3026,673.3987;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-890.52,881.6336;Inherit;False;Property;_OutlineWidth;OutlineWidth;4;0;Create;True;0;0;0;False;0;False;0;0.007;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;6;-1184.32,802.3332;Inherit;False;Property;_Color_Outline;Color_Outline;3;0;Create;True;0;0;0;False;0;False;1,0.8196079,0.427451,0;1,0.8196079,0.427451,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-907.9661,-247.7261;Inherit;True;Property;_BC;BC;0;0;Create;True;0;0;0;False;0;False;-1;None;e908818757b77ea4c8c9e39190216709;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-603.7441,421.8079;Inherit;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-611.5322,520.4507;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;13;-383.0965,312.7819;Inherit;False;Property;_UseSmoothness;UseSmoothness;9;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;12;-366.2222,154.4342;Inherit;False;Property;_UseMetallic;UseMetallic;8;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;14;-485.6349,-18.19026;Inherit;False;Property;_UseNormal;UseNormal;10;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-939.3083,4.020025;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;0;False;0;False;-1;None;7b3173608c6a3394ebe4dd825152f94c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-1202.09,-81.78841;Inherit;False;Constant;_Color0;Color 0;11;0;Create;True;0;0;0;False;0;False;0,0,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-904.9661,196.2738;Inherit;True;Property;_MS;MS;1;0;Create;True;0;0;0;False;0;False;-1;None;29f1a4f9b88682946966e74675141bb0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;20;-2008.071,222.3664;Inherit;True;Property;_Emissive;Emissive;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1538.418,353.2903;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1989.94,466.616;Inherit;False;Property;_EmissiveBoost;EmissiveBoost;12;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;24;-2382.611,668.4904;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;25;-2165.612,642.4904;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;6.88;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-1913.61,627.4904;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;29;-2257.109,1093.225;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1489.531,698.4245;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;30;-1829.633,888.9957;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SinOpNode;33;-2016.983,1055.333;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-1357.237,379.8451;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-1061.81,311.1529;Inherit;False;2;2;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-2430.091,1082.272;Inherit;True;Property;_timeglow;timeglow;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
WireConnection;0;0;1;0
WireConnection;0;1;14;0
WireConnection;0;2;34;0
WireConnection;0;3;12;0
WireConnection;0;4;13;0
WireConnection;0;11;8;0
WireConnection;8;1;9;0
WireConnection;8;0;4;0
WireConnection;4;0;6;0
WireConnection;4;1;7;0
WireConnection;13;1;11;0
WireConnection;13;0;2;4
WireConnection;12;1;10;0
WireConnection;12;0;2;0
WireConnection;14;1;19;0
WireConnection;14;0;3;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;25;0;24;1
WireConnection;26;0;25;0
WireConnection;29;0;35;0
WireConnection;32;0;26;0
WireConnection;32;1;30;0
WireConnection;30;1;33;0
WireConnection;33;0;29;0
WireConnection;27;1;32;0
WireConnection;34;0;27;0
WireConnection;34;1;21;0
ASEEND*/
//CHKSM=2165459E95242087D4CC1B0FC47E02FEE5044D5C