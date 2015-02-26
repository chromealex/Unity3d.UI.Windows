Shader "UI.Windows/Transitions/Pixelate" {

	Properties {
	
         [HideInInspector] _MainTex("From (RGB)", 2D) = "white" {}
         [HideInInspector] _ClearScreen("ClearScreen (RGB)", 2D) = "white" {}
         _Value("Value", Range(0, 1)) = 0
         
         _MaxSize("Max Size", Range(0, 1)) = 1
		 _Aspect("Aspect", float) = 1.0
		
	}

	SubShader {
	
		Pass {
		
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }


			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _ClearScreen;
			fixed _Value;
			fixed _MaxSize;
			fixed _Aspect;
            uniform half4 _MainTex_TexelSize;

			fixed4 frag( v2f_img i ):COLOR {
				
	         	 float2 uv = i.uv;
				float2 ouv = uv;
	         	 #if UNITY_UV_STARTS_AT_TOP
				 if (_MainTex_TexelSize.y < 0) uv.y = 1 - uv.y;
				 #endif
				 
				fixed4 t1 = tex2D(_MainTex, ouv);
				fixed4 t2 = tex2D(_ClearScreen, uv);
				if (t1.a - t2.a == 0 && t1.r - t2.r == 0 && t1.g - t2.g == 0 && t1.b - t2.b == 0) discard;
				
				fixed value = _Value;
				if (value < 0.5) value = 1 - value;
				value = (1 - value) * 0.5 * _MaxSize;
				if (value < 0.0001) value = 0.0001;
				
				float2 cellSize = float2(value * _Aspect, value);
			    float2 steppedUV = round(uv.xy / cellSize) * cellSize;
			    float2 steppedUVi = steppedUV;
			    
	         	 #if UNITY_UV_STARTS_AT_TOP
				 if (_MainTex_TexelSize.y < 0) steppedUVi.y = 1 - steppedUVi.y;
				 #endif
			    return lerp(tex2D(_MainTex, steppedUVi), tex2D(_ClearScreen, steppedUV), _Value);
			    
			}

			ENDCG
			
		}
	}

	FallBack off
}