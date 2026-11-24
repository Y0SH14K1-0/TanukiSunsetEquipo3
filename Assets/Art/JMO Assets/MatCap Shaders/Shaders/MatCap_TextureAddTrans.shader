// MatCap Shader, (c) 2015-2019 Jean Moreno

Shader "MatCap/Vertex/Textured Add Trans"
{
	Properties
	{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
	}
	
	Subshader
	{
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fog
				#include "UnityCG.cginc"
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv 	: TEXCOORD0;
					float2 cap	: TEXCOORD1;
					UNITY_FOG_COORDS(2)
				};
				
				uniform float4 _MainTex_ST;
				
				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;
					
					UNITY_TRANSFER_FOG(o, o.pos);

					return o;
				}
				
				uniform float4 _Color;
				uniform sampler2D _MainTex;
				uniform sampler2D _MatCap;
				
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
					fixed4 mc = tex2D(_MatCap, i.cap);
					//tex.a = _Color.a;
					mc.a = mc.r * _Color.a;

			#ifndef UNITY_COLORSPACE_GAMMA
					// perform the blending operation in gamma space to get the same result in linear space
					tex.rgba = LinearToGammaSpace(tex.rgb);
					mc.rgba = LinearToGammaSpace(mc.rgb);
					mc *= 2.0;
					mc = saturate(tex + mc - 1.0);
					mc.rgba = GammaToLinearSpace(mc.rgb);
					UNITY_APPLY_FOG(i.fogCoord, mc);
					return mc.rgba;
			#else
					mc.rgba = tex.rgba + (mc.rgba * 2.0) - 1.0;
					UNITY_APPLY_FOG(i.fogCoord, mc);
					return mc;
			#endif
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}
