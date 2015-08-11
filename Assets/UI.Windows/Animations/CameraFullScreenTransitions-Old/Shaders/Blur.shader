Shader "UI.Windows/Transitions/Blur" {

	Properties {
	
		 [HideInInspector] _MainTex("From (RGB)", 2D) = "white" {}
         [HideInInspector] _ClearScreen("ClearScreen (RGB)", 2D) = "white" {}
         _Value("Value", Range(0, 1)) = 0
         
		_MaxSize ( "Max Blur Size", Range( 0.0, 0.05 ) ) = 0.05
	}

	SubShader {
	
		Tags { "RenderType" = "Opaque" }

		Pass {
			CGPROGRAM
			#pragma exclude_renderers ps3 xbox360
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"


			// uniforms
			sampler2D _MainTex;
			sampler2D _ClearScreen;
			fixed _Value;
			fixed _MaxSize;
            float4 _MainTex_TexelSize;


			fixed4 frag( v2f_img i ) : COLOR {
			
	         	 float2 uv = i.uv;
	         	 float2 ouv = uv;
	         	 #if UNITY_UV_STARTS_AT_TOP
				 if (_MainTex_TexelSize.y < 0) ouv.y = 1 - ouv.y;
				 #endif
				
				fixed4 t1 = tex2D(_MainTex, uv);
				fixed4 t2 = tex2D(_ClearScreen, ouv);
			 	if (t1.a - t2.a == 0 && t1.r - t2.r == 0 && t1.g - t2.g == 0 && t1.b - t2.b == 0) discard;
				
				fixed value = _Value;
				if (value < 0.5) value = 1 - value;
				value = (1 - value) * 0.5 * _MaxSize;
				
				fixed4 sum = fixed4( 0.0, 0.0, 0.0, 0.0 );
				sum += tex2D( _MainTex, half2( uv.x, uv.y - 4.0 * value ) ) * 0.05;
				//sum += tex2D( _MainTex, half2( uv.x, uv.y - 3.0 * value ) ) * 0.09;
				sum += tex2D( _MainTex, half2( uv.x, uv.y - 2.0 * value ) ) * 0.12;
				sum += tex2D( _MainTex, half2( uv.x, uv.y - value ) ) * 0.15;
				sum += tex2D( _MainTex, half2( uv.x, uv.y ) ) * 0.16;
				sum += tex2D( _MainTex, half2( uv.x, uv.y + value ) ) * 0.15;
				sum += tex2D( _MainTex, half2( uv.x, uv.y + 2.0 * value ) ) * 0.12;
				//sum += tex2D( _MainTex, half2( uv.x, uv.y + 3.0 * value ) ) * 0.09;
				sum += tex2D( _MainTex, half2( uv.x, uv.y + 4.0 * value ) ) * 0.05;

				return sum;
				
			}

			ENDCG
		} // end Pass
	} // end SubShader

	FallBack "Diffuse"
}