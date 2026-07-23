// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_Lava"
{
	Properties
	{
		_emissive("emissive", 2D) = "white" {}
		_boost("boost", Float) = 1
		_time("time", Float) = 1
		_wavelength("wavelength", Float) = 5
		_displacementstrength("displacement strength", Float) = 1
		_Color0("Color 0", Color) = (1,1,1,1)
		_Dessaturation("Dessaturation", Float) = 0
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
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _displacementstrength;
		uniform float _wavelength;
		uniform float _time;
		uniform float4 _Color0;
		uniform sampler2D _emissive;
		uniform float4 _emissive_ST;
		uniform float _boost;
		uniform float _Dessaturation;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime13 = _Time.y * _time;
			float temp_output_18_0 = frac( ( ( v.texcoord.xy.x * _wavelength ) + mulTime13 ) );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 temp_cast_0 = (( _displacementstrength * ( saturate( ( ( 1.0 - temp_output_18_0 ) * temp_output_18_0 * 4.0 ) ) * ase_vertexNormal.y ) )).xxx;
			v.vertex.xyz += temp_cast_0;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_emissive = i.uv_texcoord * _emissive_ST.xy + _emissive_ST.zw;
			float3 desaturateInitialColor30 = ( tex2D( _emissive, uv_emissive ) * _boost ).rgb;
			float desaturateDot30 = dot( desaturateInitialColor30, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar30 = lerp( desaturateInitialColor30, desaturateDot30.xxx, _Dessaturation );
			float4 temp_output_28_0 = ( _Color0 * float4( desaturateVar30 , 0.0 ) );
			o.Albedo = temp_output_28_0.rgb;
			o.Emission = temp_output_28_0.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.SaturateNode;9;-642.9869,416.5146;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-853.9869,415.8146;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-243.7519,433.912;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;18;-1657.221,401.6955;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;6;-1371.132,282.1237;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-2366.958,394.7419;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-2080.222,385.6955;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-1829.48,447.2597;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;13;-2204.552,772.9692;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-2340.652,778.1699;Inherit;False;Property;_time;time;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;2;-536.1331,714.8893;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-160.3264,310.652;Inherit;False;Property;_displacementstrength;displacement strength;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;60.67358,356.652;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2284.686,560.9016;Inherit;False;Property;_wavelength;wavelength;3;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1043.987,642.8146;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;46.38074,25.18909;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;26;-263.6193,-173.8109;Inherit;False;Property;_Color0;Color 0;5;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;446,10;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_Lava;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-398.7367,51.86786;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-402.7367,150.8679;Inherit;False;Property;_boost;boost;1;0;Create;True;0;0;0;False;0;False;1;0.39;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-724,45;Inherit;True;Property;_emissive;emissive;0;0;Create;True;0;0;0;False;0;False;-1;6ce1075fd973e2f4fac21208fdeae924;bdd461c1cde0c7d438984cb3a05faf30;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;30;-159.619,47.78909;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-160.619,153.7891;Inherit;False;Property;_Dessaturation;Dessaturation;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
WireConnection;9;0;7;0
WireConnection;7;0;6;0
WireConnection;7;1;18;0
WireConnection;7;2;8;0
WireConnection;12;0;9;0
WireConnection;12;1;2;2
WireConnection;18;0;23;0
WireConnection;6;0;18;0
WireConnection;21;0;4;1
WireConnection;21;1;22;0
WireConnection;23;0;21;0
WireConnection;23;1;13;0
WireConnection;13;0;14;0
WireConnection;24;0;25;0
WireConnection;24;1;12;0
WireConnection;28;0;26;0
WireConnection;28;1;30;0
WireConnection;0;0;28;0
WireConnection;0;2;28;0
WireConnection;0;11;24;0
WireConnection;10;0;1;0
WireConnection;10;1;11;0
WireConnection;30;0;10;0
WireConnection;30;1;31;0
ASEEND*/
//CHKSM=362C9B9AD818CB85254C75BF32EC62A725DF43F2