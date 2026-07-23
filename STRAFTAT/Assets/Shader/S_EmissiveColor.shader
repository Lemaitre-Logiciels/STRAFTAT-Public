// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_EmissiveColor"
{
	Properties
	{
		_EmissiveBoost("EmissiveBoost", Float) = 1
		_Color0("Color 0", Color) = (1,1,1,0)
		_DirtTex("DirtTex", 2D) = "white" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Power("Power", Float) = 0
		_Float0("Float 0", Float) = 0
		_tilingdirt("tiling dirt", Float) = 0
		_NoiseValue("NoiseValue", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample0;
		uniform float _tilingdirt;
		uniform float4 _Color0;
		uniform sampler2D _DirtTex;
		uniform float _Float0;
		uniform float _Power;
		uniform float _NoiseValue;
		uniform float _EmissiveBoost;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 tex2DNode15 = tex2D( _TextureSample0, ( _tilingdirt * i.uv_texcoord ) );
			o.Albedo = tex2DNode15.rgb;
			float4 temp_cast_1 = (_Power).xxxx;
			o.Emission = ( ( ( _Color0 * saturate( ( pow( tex2D( _DirtTex, ( _Float0 * i.uv_texcoord ) ) , temp_cast_1 ) + _NoiseValue ) ) ) * _EmissiveBoost ) * tex2DNode15 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
-414;1013;1906;459;2024.037;696.1655;1.501102;True;True
Node;AmplifyShaderEditor.RangedFloatNode;8;-1834,13.5;Float;False;Property;_Float0;Float 0;5;0;Create;True;0;0;False;0;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1877,154.5;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-1617,108.5;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1089.197,197.1097;Float;False;Property;_Power;Power;4;0;Create;True;0;0;False;0;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1419,78.5;Float;True;Property;_DirtTex;DirtTex;2;0;Create;True;0;0;False;0;None;a87d8939bd8786f499b13c9a56c4b477;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;6;-1021.197,82.10973;Float;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-927.4977,207.4971;Float;False;Property;_NoiseValue;NoiseValue;7;0;Create;True;0;0;False;0;0;-0.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-788.0853,82.3547;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1391.477,-513.8207;Float;False;Property;_tilingdirt;tiling dirt;6;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;13;-651.9645,79.06137;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1;-793,-191.5;Float;False;Property;_Color0;Color 0;1;0;Create;True;0;0;False;0;1,1,1,0;0.75891,0.4575472,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-1434.477,-372.8208;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-410,228.5;Float;False;Property;_EmissiveBoost;EmissiveBoost;0;0;Create;True;0;0;False;0;1;13.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-475,7.5;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1174.477,-418.8208;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;15;-859.8044,-490.6888;Float;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;False;0;None;93ca740d28e742b45a1f75df8385b162;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-226,87.5;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-12.77489,72.59994;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;275.3546,-1.397739;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;S_EmissiveColor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;15;1;19;0
WireConnection;2;0;5;0
WireConnection;2;1;3;0
WireConnection;16;0;2;0
WireConnection;16;1;15;0
WireConnection;0;0;15;0
WireConnection;0;2;16;0
ASEEND*/
//CHKSM=6CDD8AF25FA51EAA841A7F1E9E5C5F231A77826D