// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/S_Nuages_00"
{
	Properties
	{
		_OpacityMask("OpacityMask", 2D) = "white" {}
		_Opacity("Opacity", Float) = 1
		_Color0("Color0", Color) = (1,1,1,0)
		_Color1("Color1", Color) = (0.3584906,0.3584906,0.3584906,0)
		_Contrast("Contrast", Float) = 1
		[Toggle(_USECOLORLERP_ON)] _UsecolorLerp("UsecolorLerp", Float) = 0
		[Toggle(_USEGRADIENTMASK_ON)] _UseGradientMask("UseGradientMask", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USECOLORLERP_ON
		#pragma shader_feature_local _USEGRADIENTMASK_ON
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _OpacityMask;
		uniform float4 _OpacityMask_ST;
		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float _Contrast;
		uniform float _Opacity;


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1));
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1));
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			Gradient gradient17 = NewGradient( 0, 5, 2, float4( 0.8000001, 0.8078432, 0.7607844, 0.1143664 ), float4( 0.7373877, 0.6668299, 0.764151, 0.1554284 ), float4( 0.5563961, 0.5849056, 0.3283197, 0.225803 ), float4( 0.1234574, 0.1320755, 0.08036669, 0.7477989 ), float4( 0.1490196, 0.1568628, 0.09411766, 0.9911956 ), 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float2 uv_OpacityMask = i.uv_texcoord * _OpacityMask_ST.xy + _OpacityMask_ST.zw;
			float4 tex2DNode1 = tex2D( _OpacityMask, uv_OpacityMask );
			float4 lerpResult6 = lerp( _Color0 , _Color1 , tex2DNode1.r);
			#ifdef _USECOLORLERP_ON
				float4 staticSwitch19 = lerpResult6;
			#else
				float4 staticSwitch19 = SampleGradient( gradient17, tex2DNode1.r );
			#endif
			o.Emission = staticSwitch19.rgb;
			float4 temp_cast_1 = (tex2DNode1.r).xxxx;
			float4 temp_output_13_0 = saturate( ( saturate( CalculateContrast(_Contrast,temp_cast_1) ) * _Opacity ) );
			#ifdef _USEGRADIENTMASK_ON
				float4 staticSwitch20 = ( temp_output_13_0 * saturate( ( ( i.uv_texcoord.x * ( 1.0 - i.uv_texcoord.x ) * 4.69 ) * ( i.uv_texcoord.y * ( 1.0 - i.uv_texcoord.y ) * 4.69 ) ) ) );
			#else
				float4 staticSwitch20 = temp_output_13_0;
			#endif
			o.Alpha = staticSwitch20.r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.SaturateNode;12;-366.551,192.3105;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-230.5505,208.3105;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-917.7073,520.6059;Inherit;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;0;False;0;False;1;1.07;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;10;-738.7079,199.6059;Inherit;True;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-1058.707,119.6059;Inherit;True;Property;_OpacityMask;OpacityMask;0;0;Create;True;0;0;0;False;0;False;-1;None;05a60c76e0e03c242a6822516029dde4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-949.7079,352.6059;Inherit;False;Property;_Contrast;Contrast;4;0;Create;True;0;0;0;False;0;False;1;4.55;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;16;-344.4446,-26.69587;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-1233.873,-215.0813;Inherit;False;Property;_Color1;Color1;3;0;Create;True;0;0;0;False;0;False;0.3584906,0.3584906,0.3584906,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-851.8726,-254.0813;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GradientNode;15;-596.7611,-177.0198;Inherit;False;0;4;2;1,1,1,0.1143664;0.7373877,0.6668299,0.764151,0.1554284;0.254717,0.254717,0.254717,0.225803;0,0,0,0.7477989;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.ColorNode;7;-1238.873,-388.0814;Inherit;False;Property;_Color0;Color0;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.5943396,0.5943396,0.5943396,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientNode;17;-629.3361,-50.16959;Inherit;False;0;5;2;0.8000001,0.8078432,0.7607844,0.1143664;0.7373877,0.6668299,0.764151,0.1554284;0.5563961,0.5849056,0.3283197,0.225803;0.1234574,0.1320755,0.08036669,0.7477989;0.1490196,0.1568628,0.09411766,0.9911956;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.StaticSwitch;19;-31.03605,-193.5695;Inherit;False;Property;_UsecolorLerp;UsecolorLerp;5;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;540.5998,-89.00003;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Custom/S_Nuages_00;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-1631.203,806.7609;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;23;-1270.203,784.7609;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-950.2032,709.7609;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1050.203,971.7609;Inherit;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;0;False;0;False;4.69;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-1216.203,1143.428;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-896.2033,1068.428;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-611.2031,856.7609;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;13;-72.24006,197.5467;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;33;-317.0651,834.4196;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;6.296401,371.4879;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;20;175.9615,258.634;Inherit;False;Property;_UseGradientMask;UseGradientMask;6;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
WireConnection;12;0;10;0
WireConnection;2;0;12;0
WireConnection;2;1;3;0
WireConnection;10;1;1;1
WireConnection;10;0;11;0
WireConnection;16;0;17;0
WireConnection;16;1;1;1
WireConnection;6;0;7;0
WireConnection;6;1;8;0
WireConnection;6;2;1;1
WireConnection;19;1;16;0
WireConnection;19;0;6;0
WireConnection;0;2;19;0
WireConnection;0;9;20;0
WireConnection;23;0;21;1
WireConnection;22;0;21;1
WireConnection;22;1;23;0
WireConnection;22;2;24;0
WireConnection;28;0;21;2
WireConnection;29;0;21;2
WireConnection;29;1;28;0
WireConnection;29;2;24;0
WireConnection;31;0;22;0
WireConnection;31;1;29;0
WireConnection;13;0;2;0
WireConnection;33;0;31;0
WireConnection;32;0;13;0
WireConnection;32;1;33;0
WireConnection;20;1;13;0
WireConnection;20;0;32;0
ASEEND*/
//CHKSM=FD3F1BB5BB8C01C550EC7CEC9BAF3E77DC6ED072