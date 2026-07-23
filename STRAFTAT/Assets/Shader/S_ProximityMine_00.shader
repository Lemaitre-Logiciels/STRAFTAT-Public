// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_ProximityMine_00"
{
	Properties
	{
		_gradient("gradient", Float) = 0.06
		_Color0("Color 0", Color) = (0.8867924,0.2133321,0.2133321,0)
		_time("time", Float) = 11.46
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend One One
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _Color0;
		uniform float _time;
		uniform float _gradient;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime20 = _Time.y * _time;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float temp_output_3_0 = saturate( ase_vertex3Pos.y );
			o.Emission = saturate( ( _Color0 * saturate( ( sin( mulTime20 ) + 1.27 ) ) * ( ( ( 1.0 - temp_output_3_0 ) * ceil( temp_output_3_0 ) ) - saturate( ( ( 12.54 + temp_output_3_0 ) * _gradient ) ) ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.PosVertexDataNode;1;-1476.361,347.3466;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;3;-1257.361,334.3466;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;4;-1031.361,229.3466;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-770.3611,280.3466;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;6;-1019.361,463.3466;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-835.3611,-97.65344;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;-1041.361,55.34661;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;13;-492.3613,198.3466;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;14;-617.3611,64.3466;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1055.361,-51.6534;Inherit;False;Property;_gradient;gradient;0;0;Create;True;0;0;0;False;0;False;0.06;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1243.361,84.34661;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;12.54;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-769.2222,-451.2946;Inherit;False;Property;_Color0;Color 0;2;0;Create;True;0;0;0;False;0;False;0.8867924,0.2133321,0.2133321,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;20;-1003.723,-259.1849;Inherit;True;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1191.723,-257.1849;Inherit;False;Property;_time;time;3;0;Create;True;0;0;0;False;0;False;11.46;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;23;-447.7227,-169.1849;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-655.7227,-218.1849;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;1.27;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;150.8,-11.7;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_ProximityMine_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;4;1;False;;1;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SaturateNode;24;-27.69922,24.82294;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-252.0121,-230.099;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;19;-823.2227,-310.1849;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
WireConnection;3;0;1;2
WireConnection;4;0;3;0
WireConnection;5;0;4;0
WireConnection;5;1;6;0
WireConnection;6;0;3;0
WireConnection;7;0;10;0
WireConnection;7;1;8;0
WireConnection;10;0;11;0
WireConnection;10;1;3;0
WireConnection;13;0;5;0
WireConnection;13;1;14;0
WireConnection;14;0;7;0
WireConnection;20;0;21;0
WireConnection;23;0;22;0
WireConnection;22;0;19;0
WireConnection;0;2;24;0
WireConnection;24;0;18;0
WireConnection;18;0;15;0
WireConnection;18;1;23;0
WireConnection;18;2;13;0
WireConnection;19;0;20;0
ASEEND*/
//CHKSM=BE1C847FA85AD0D91AF93209C9DF62DB8FBBAC69