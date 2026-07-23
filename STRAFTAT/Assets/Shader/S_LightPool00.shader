// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_LightPool00"
{
	Properties
	{
		_Color1("Color 1", Color) = (1,1,1,0)
		_Color0("Color 0", Color) = (0.664151,0.9602816,1,0)
		_t1("t1", 2D) = "white" {}
		_distancedepthfade("distancedepthfade", Float) = 0
		_Color2("Color 2", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPosition34;
		};

		uniform float4 _Color2;
		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform sampler2D _t1;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _distancedepthfade;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 vertexPos34 = ase_vertex3Pos;
			float4 ase_screenPos34 = ComputeScreenPos( UnityObjectToClipPos( vertexPos34 ) );
			o.screenPosition34 = ase_screenPos34;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (0.2).xx;
			float2 uv_TexCoord29 = i.uv_texcoord * temp_cast_0;
			float cos32 = cos( 0.1 * _Time.y );
			float sin32 = sin( 0.1 * _Time.y );
			float2 rotator32 = mul( uv_TexCoord29 - float2( 0.5,0 ) , float2x2( cos32 , -sin32 , sin32 , cos32 )) + float2( 0.5,0 );
			float2 temp_cast_1 = (0.6).xx;
			float2 uv_TexCoord3 = i.uv_texcoord * temp_cast_1;
			float2 panner2 = ( 1.0 * _Time.y * float2( 0.05,0.05 ) + uv_TexCoord3);
			float2 temp_cast_2 = (0.8).xx;
			float2 uv_TexCoord10 = i.uv_texcoord * temp_cast_2;
			float2 panner9 = ( 1.0 * _Time.y * float2( -0.03,0.05 ) + uv_TexCoord10);
			float temp_output_33_0 = ( tex2D( _t1, rotator32 ).r + ( ( tex2D( _t1, panner2 ).r * tex2D( _t1, panner9 ).r ) * 2.0 ) );
			float4 lerpResult5 = lerp( ( ( ( abs( _SinTime.w ) / 2.0 ) + 0.5 ) * _Color0 ) , _Color1 , temp_output_33_0);
			float4 ase_screenPos34 = i.screenPosition34;
			float4 ase_screenPosNorm34 = ase_screenPos34 / ase_screenPos34.w;
			ase_screenPosNorm34.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm34.z : ase_screenPosNorm34.z * 0.5 + 0.5;
			float screenDepth34 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm34.xy ));
			float distanceDepth34 = abs( ( screenDepth34 - LinearEyeDepth( ase_screenPosNorm34.z ) ) / ( _distancedepthfade ) );
			float4 lerpResult44 = lerp( _Color2 , lerpResult5 , saturate( ( temp_output_33_0 + saturate( distanceDepth34 ) ) ));
			o.Emission = lerpResult44.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.ColorNode;8;-630.6845,-168.9726;Inherit;False;Property;_Color1;Color 1;0;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;2;-943.9839,53.32734;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-1328.785,-24.67274;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-701.2498,-343.1727;Inherit;False;Property;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;0.664151,0.9602816,1,0;0.237522,0.3018866,0.2834968,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinTimeNode;15;-1097.04,-554.3621;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.AbsOpNode;17;-858.3774,-573.4583;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-251.3774,-419.4583;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;19;-679.3774,-583.4583;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-503.3774,-583.4583;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;4;-1375.68,294.7482;Inherit;False;Constant;_Vector0;Vector 0;1;0;Create;True;0;0;0;False;0;False;0.05,0.05;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-306.9,231.1219;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1627.932,-1.419365;Inherit;False;Constant;_Float1;Float 0;4;0;Create;True;0;0;0;False;0;False;0.6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;9;-920.6041,657.814;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;11;-1178.003,895.7137;Inherit;False;Constant;_Vector1;Vector 0;1;0;Create;True;0;0;0;False;0;False;-0.03,0.05;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-1305.404,579.814;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;22;-1516.812,590.2706;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;0;False;0;False;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;26;-884.9728,1167.509;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;28;-1142.372,1405.409;Inherit;False;Constant;_Vector2;Vector 0;1;0;Create;True;0;0;0;False;0;False;-0.03,0.05;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-1269.773,1089.51;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-629.3839,15.62742;Inherit;True;Property;_T1;T1;0;0;Create;True;0;0;0;False;0;False;-1;b667a267b3f0b7941a0adb11cf2fa2ce;b667a267b3f0b7941a0adb11cf2fa2ce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-606.0051,620.1141;Inherit;True;Property;_T2;T1;1;0;Create;True;0;0;0;False;0;False;-1;b667a267b3f0b7941a0adb11cf2fa2ce;b667a267b3f0b7941a0adb11cf2fa2ce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;31;-977.8118,383.4383;Inherit;True;Property;_t1;t1;2;0;Create;True;0;0;0;False;0;False;None;b667a267b3f0b7941a0adb11cf2fa2ce;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RotatorNode;32;-920.2126,1042.612;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0;False;2;FLOAT;0.1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1479.379,1099.966;Inherit;False;Constant;_Float2;Float 0;4;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;27;-485.9203,909.7184;Inherit;True;Property;_T3;T1;1;0;Create;True;0;0;0;False;0;False;-1;b667a267b3f0b7941a0adb11cf2fa2ce;b667a267b3f0b7941a0adb11cf2fa2ce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-60.85216,229.8249;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;125.2648,308.1733;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;34;339.0043,544.0798;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;244.154,699.1525;Inherit;False;Property;_distancedepthfade;distancedepthfade;3;0;Create;True;0;0;0;False;0;False;0;2.07;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;42;60.56389,631.7309;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;5;1342.478,-251.1826;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;833.4528,322.3699;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;37;701.0967,493.9618;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;14;1053.42,331.3723;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1795.602,-309.3719;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_LightPool00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.LerpOp;44;1605.214,-184.2229;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;45;1130.395,-35.3392;Inherit;False;Property;_Color2;Color 2;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;2;0;3;0
WireConnection;2;2;4;0
WireConnection;3;0;24;0
WireConnection;17;0;15;4
WireConnection;16;0;21;0
WireConnection;16;1;7;0
WireConnection;19;0;17;0
WireConnection;21;0;19;0
WireConnection;13;0;1;1
WireConnection;13;1;12;1
WireConnection;9;0;10;0
WireConnection;9;2;11;0
WireConnection;10;0;22;0
WireConnection;26;0;29;0
WireConnection;26;2;28;0
WireConnection;29;0;30;0
WireConnection;1;0;31;0
WireConnection;1;1;2;0
WireConnection;12;0;31;0
WireConnection;12;1;9;0
WireConnection;32;0;29;0
WireConnection;27;0;31;0
WireConnection;27;1;32;0
WireConnection;25;0;13;0
WireConnection;33;0;27;1
WireConnection;33;1;25;0
WireConnection;34;1;42;0
WireConnection;34;0;35;0
WireConnection;5;0;16;0
WireConnection;5;1;8;0
WireConnection;5;2;33;0
WireConnection;39;0;33;0
WireConnection;39;1;37;0
WireConnection;37;0;34;0
WireConnection;14;0;39;0
WireConnection;0;2;44;0
WireConnection;44;0;45;0
WireConnection;44;1;5;0
WireConnection;44;2;14;0
ASEEND*/
//CHKSM=2A0A2685E8361901A3F148FE5036CCBB44E31533