// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_PropsEmissive_00"
{
	Properties
	{
		_BC("BC", 2D) = "white" {}
		_MS("MS", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		[Toggle(_USEMETALLIC_ON)] _UseMetallic("UseMetallic", Float) = 1
		[Toggle(_USESMOOTHNESS_ON)] _UseSmoothness("UseSmoothness", Float) = 1
		[Toggle(_USENORMAL_ON)] _UseNormal("UseNormal", Float) = 1
		_Emissive("Emissive", 2D) = "white" {}
		_EmissiveColor("EmissiveColor", Color) = (0.3647059,0.8992954,1,0)
		_EmissiveBoost("EmissiveBoost", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature_local _USENORMAL_ON
		#pragma shader_feature_local _USEMETALLIC_ON
		#pragma shader_feature_local _USESMOOTHNESS_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _BC;
		uniform float4 _BC_ST;
		uniform sampler2D _Emissive;
		uniform float4 _Emissive_ST;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveBoost;
		uniform float _Metallic;
		uniform sampler2D _MS;
		uniform float4 _MS_ST;
		uniform float _Smoothness;

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
			float2 uv_Emissive = i.uv_texcoord * _Emissive_ST.xy + _Emissive_ST.zw;
			o.Emission = ( ( tex2D( _Emissive, uv_Emissive ) * _EmissiveColor ) * _EmissiveBoost ).rgb;
			float4 temp_cast_4 = (_Metallic).xxxx;
			float2 uv_MS = i.uv_texcoord * _MS_ST.xy + _MS_ST.zw;
			float4 tex2DNode2 = tex2D( _MS, uv_MS );
			#ifdef _USEMETALLIC_ON
				float4 staticSwitch12 = tex2DNode2;
			#else
				float4 staticSwitch12 = temp_cast_4;
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
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_PropsEmissive_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.StaticSwitch;13;-383.0965,312.7819;Inherit;False;Property;_UseSmoothness;UseSmoothness;9;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;12;-366.2222,154.4342;Inherit;False;Property;_UseMetallic;UseMetallic;8;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;8;-353.411,1245.67;Inherit;False;Property;_UseOutline;UseOutline;5;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OutlineNode;4;-641.0206,1329.615;Inherit;False;0;False;None;0;0;Front;True;True;True;True;0;False;;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-609.7545,1203.388;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-837.9719,1411.623;Inherit;False;Property;_OutlineWidth;OutlineWidth;4;0;Create;True;0;0;0;False;0;False;0;0.007;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;6;-1131.772,1332.322;Inherit;False;Property;_Color_Outline;Color_Outline;3;0;Create;True;0;0;0;False;0;False;1,0.8196079,0.427451,0;1,0.8196079,0.427451,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;14;-755.4356,-196.7902;Inherit;False;Property;_UseNormal;UseNormal;10;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-1131.209,-98.57999;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;0;False;0;False;-1;None;1134bf1357f9a4d41a66e9ced2a27ef0;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-1357.891,-191.9886;Inherit;False;Constant;_Color0;Color 0;11;0;Create;True;0;0;0;False;0;False;0,0,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-856.6671,-397.8262;Inherit;True;Property;_BC;BC;0;0;Create;True;0;0;0;False;0;False;-1;None;e5faf7961403b6e46bf091c3792ea553;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1334.967,46.69154;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;20;-1725.694,-30.02932;Inherit;True;Property;_Emissive;Emissive;11;0;Create;True;0;0;0;False;0;False;-1;None;6dedb7623081c98438057a6ec8fb7cb2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;22;-1652.867,172.4917;Inherit;False;Property;_EmissiveColor;EmissiveColor;12;0;Create;True;0;0;0;False;0;False;0.3647059,0.8992954,1,0;0.3647058,0.8992954,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-901.1661,179.1739;Inherit;True;Property;_MS;MS;1;0;Create;True;0;0;0;False;0;False;-1;None;29b19f302dce7114cb59a45c4e9eb196;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-530.0979,153.3693;Inherit;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-617.4168,398.0532;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1198.273,351.1975;Inherit;False;Property;_EmissiveBoost;EmissiveBoost;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-1039.365,94.69226;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
WireConnection;0;0;1;0
WireConnection;0;1;14;0
WireConnection;0;2;23;0
WireConnection;0;3;12;0
WireConnection;0;4;13;0
WireConnection;13;1;11;0
WireConnection;13;0;2;4
WireConnection;12;1;10;0
WireConnection;12;0;2;0
WireConnection;8;1;9;0
WireConnection;8;0;4;0
WireConnection;4;0;6;0
WireConnection;4;1;7;0
WireConnection;14;1;19;0
WireConnection;14;0;3;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;23;0;21;0
WireConnection;23;1;24;0
ASEEND*/
//CHKSM=B5BAC90ABD76F64BD91ABC7C5A7706ED70C97F5C