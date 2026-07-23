// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_XPBar_00"
{
	Properties
	{
		_time("time", Float) = 1
		_time2("time2", Float) = 1
		_Float1("Float 1", Float) = 30.59
		_Color1("Color 1", Color) = (0.4567801,0,0.6588235,0)
		_Color0("Color 0", Color) = (0.800145,0.03301889,1,0)
		_wavelength("wavelength", Float) = 5
		_wavelength2("wavelength2", Float) = 5
		_bar2_numbers("bar2_numbers", Float) = 0.1
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
		uniform float _wavelength;
		uniform float _time;
		uniform float _bar2_numbers;
		uniform float _wavelength2;
		uniform float _time2;
		uniform float _Float1;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float mulTime8 = _Time.y * _time;
			float temp_output_3_0 = frac( ( ( ( ase_worldPos.x * 0.01 ) * _wavelength ) + mulTime8 ) );
			float mulTime27 = _Time.y * _time2;
			float temp_output_20_0 = frac( ( ( ( ase_worldPos.x * _bar2_numbers ) * _wavelength2 ) + mulTime27 ) );
			float4 lerpResult13 = lerp( _Color0 , _Color1 , saturate( ( saturate( ( ( 1.0 - temp_output_3_0 ) * temp_output_3_0 * 4.0 ) ) * saturate( ( ( 1.0 - temp_output_20_0 ) * temp_output_20_0 * _Float1 ) ) ) ));
			o.Emission = lerpResult13.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.WorldPosInputsNode;15;-2607.466,-198.1165;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-2622.934,125.5198;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-545.965,21.39221;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-735.9653,248.3922;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;3;-1281.21,45.26736;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;4;-995.1205,-74.30439;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1704.211,29.26736;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-1453.469,90.83163;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1964.641,421.7419;Inherit;False;Property;_time;time;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-1908.675,204.4735;Inherit;False;Property;_wavelength;wavelength;5;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1953.618,-70.88218;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;8;-1828.54,416.5411;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;17;-363.8608,741.3171;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-613.2609,718.2171;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;20;-1348.505,742.0923;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;21;-1062.415,622.5205;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1771.506,726.0923;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-1520.764,787.6566;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;27;-1895.835,1113.366;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1;-328.6381,37.36489;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-100.483,478.883;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1434.588,15.71494;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_XPBar_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.ColorNode;14;584.9773,-118.2654;Inherit;False;Property;_Color1;Color 1;3;0;Create;True;0;0;0;False;0;False;0.4567801,0,0.6588235,0;0,1,0.6042852,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;13;1048.954,13.15028;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-802.2612,946.2171;Inherit;False;Property;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;30.59;9.71;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1975.97,901.2984;Inherit;False;Property;_wavelength2;wavelength2;6;0;Create;True;0;0;0;False;0;False;5;35.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-2031.935,1118.567;Inherit;False;Property;_time2;time2;1;0;Create;True;0;0;0;False;0;False;1;-2.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;29;192.8801,329.5582;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;559.498,-307.343;Inherit;False;Property;_Color0;Color 0;4;0;Create;True;0;0;0;False;0;False;0.800145,0.03301889,1,0;0.800145,0.03301889,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-2020.912,625.9427;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2255.291,685.5865;Inherit;False;Property;_bar2_numbers;bar2_numbers;7;0;Create;True;0;0;0;False;0;False;0.1;-70.94;0;0;0;1;FLOAT;0
WireConnection;2;0;4;0
WireConnection;2;1;3;0
WireConnection;2;2;11;0
WireConnection;3;0;7;0
WireConnection;4;0;3;0
WireConnection;6;0;16;0
WireConnection;6;1;10;0
WireConnection;7;0;6;0
WireConnection;7;1;8;0
WireConnection;16;0;15;1
WireConnection;8;0;9;0
WireConnection;17;0;18;0
WireConnection;18;0;21;0
WireConnection;18;1;20;0
WireConnection;18;2;19;0
WireConnection;20;0;23;0
WireConnection;21;0;20;0
WireConnection;22;0;26;0
WireConnection;22;1;25;0
WireConnection;23;0;22;0
WireConnection;23;1;27;0
WireConnection;27;0;24;0
WireConnection;1;0;2;0
WireConnection;28;0;1;0
WireConnection;28;1;17;0
WireConnection;0;2;13;0
WireConnection;13;0;12;0
WireConnection;13;1;14;0
WireConnection;13;2;29;0
WireConnection;29;0;28;0
WireConnection;26;0;15;1
WireConnection;26;1;30;0
ASEEND*/
//CHKSM=80FCF8DBCC0CDD07035365D0C855E4B25FD1CF04