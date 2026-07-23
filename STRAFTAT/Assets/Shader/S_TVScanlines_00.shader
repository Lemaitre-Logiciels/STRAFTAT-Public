// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_TVScanlines_00"
{
	Properties
	{
		_time("time", Float) = 1
		_time2("time2", Float) = 1
		_Float0("Float 0", Float) = 4
		_Float1("Float 1", Float) = 4
		_Color1("Color 1", Color) = (0,0,0,0)
		_Color0("Color 0", Color) = (0,1,0.3772221,0)
		_wavelength("wavelength", Float) = 5
		_wavelength2("wavelength2", Float) = 5
		_lines2_number("lines2_number", Float) = 0.1
		_lines1_number("lines1_number", Float) = 0.1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float _lines1_number;
		uniform float _wavelength;
		uniform float _time;
		uniform float _Float0;
		uniform float _lines2_number;
		uniform float _wavelength2;
		uniform float _time2;
		uniform float _Float1;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float mulTime11 = _Time.y * _time;
			float temp_output_7_0 = frac( ( ( ( ase_worldPos.y * _lines1_number ) * _wavelength ) + mulTime11 ) );
			float temp_output_5_0 = saturate( ( ( 1.0 - temp_output_7_0 ) * temp_output_7_0 * _Float0 ) );
			float mulTime26 = _Time.y * _time2;
			float temp_output_22_0 = frac( ( ( ( ase_worldPos.y * _lines2_number ) * _wavelength2 ) + mulTime26 ) );
			float4 lerpResult2 = lerp( _Color0 , _Color1 , ( temp_output_5_0 * saturate( ( ( 1.0 - temp_output_22_0 ) * temp_output_22_0 * _Float1 ) ) ));
			o.Emission = lerpResult2.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.WorldPosInputsNode;4;-2898.892,-219.5488;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;16;-2914.361,104.0876;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;5;-768.4468,-673.1169;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1017.846,-696.2169;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;7;-1821.081,-710.3361;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;8;-1534.992,-829.908;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-2244.082,-726.3359;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;-1993.34,-664.7719;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;11;-2368.412,-339.0622;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2504.512,-333.8615;Inherit;False;Property;_time;time;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-2448.546,-551.1299;Inherit;False;Property;_wavelength;wavelength;6;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-2493.49,-826.4857;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;22;-1801.977,26.53268;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;23;-1515.888,-93.03902;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2224.98,10.53292;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-1974.236,72.09691;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;26;-2349.31,397.8068;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;20;-749.3431,63.75194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-998.7426,40.65191;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-546.1527,-254.9355;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-2429.444,185.7389;Inherit;False;Property;_wavelength2;wavelength2;7;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1188.743,267.6519;Inherit;False;Property;_Float1;Float 1;3;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-2474.388,-89.61678;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-2485.41,403.0075;Inherit;False;Property;_time2;time2;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;169.737,-937.3022;Inherit;False;Property;_Color1;Color 1;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;144.2574,-1126.38;Inherit;False;Property;_Color0;Color 0;5;0;Create;True;0;0;0;False;0;False;0,1,0.3772221,0;0.800145,0.03301889,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;2;633.7141,-805.8868;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1444.451,-841.881;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_TVScanlines_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.FresnelNode;33;190.2333,-116.5049;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;34;-759.3424,-542.8645;Inherit;True;Property;_TextureSample0;Texture Sample 0;10;0;Create;True;0;0;0;False;0;False;-1;128b271de61833348a7959cbb55b844f;128b271de61833348a7959cbb55b844f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-450.2099,-547.0332;Inherit;True;2;2;0;FLOAT;0.1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-176.7117,-817.3003;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;40;-1184.714,-926.832;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0.45;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;39;-941.614,-887.8316;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,-0.14;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-2657.538,3.284424;Inherit;False;Property;_lines2_number;lines2_number;8;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2834.913,-737.5997;Inherit;False;Property;_lines1_number;lines1_number;9;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1204.1,-469.2171;Inherit;False;Property;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
WireConnection;5;0;6;0
WireConnection;6;0;8;0
WireConnection;6;1;7;0
WireConnection;6;2;14;0
WireConnection;7;0;10;0
WireConnection;8;0;7;0
WireConnection;9;0;15;0
WireConnection;9;1;13;0
WireConnection;10;0;9;0
WireConnection;10;1;11;0
WireConnection;11;0;12;0
WireConnection;15;0;4;2
WireConnection;15;1;41;0
WireConnection;22;0;25;0
WireConnection;23;0;22;0
WireConnection;24;0;30;0
WireConnection;24;1;28;0
WireConnection;25;0;24;0
WireConnection;25;1;26;0
WireConnection;26;0;27;0
WireConnection;20;0;21;0
WireConnection;21;0;23;0
WireConnection;21;1;22;0
WireConnection;21;2;29;0
WireConnection;31;0;5;0
WireConnection;31;1;20;0
WireConnection;30;0;4;2
WireConnection;30;1;32;0
WireConnection;2;0;3;0
WireConnection;2;1;1;0
WireConnection;2;2;31;0
WireConnection;0;2;2;0
WireConnection;34;1;39;0
WireConnection;35;1;34;1
WireConnection;38;0;5;0
WireConnection;38;1;35;0
WireConnection;39;0;40;0
WireConnection;39;1;11;0
ASEEND*/
//CHKSM=06417DC92807ED1BBDEF1B3802A58C65F2F4ACEB