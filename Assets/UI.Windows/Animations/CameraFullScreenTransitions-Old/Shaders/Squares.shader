Shader "UI.Windows/Transitions/Squares" {

	Properties {
		
		 //[HideInInspector]
		 _MainTex("From (RGB)", 2D) = "white" {}
         //[HideInInspector]
         _ClearScreen("ClearScreen (RGB)", 2D) = "white" {}
         _Value("Value", Range(0, 1)) = 0
         
		_Size ( "Size", Vector ) = ( 64.0, 45.0, 0, 0 )
		_Smoothness ( "Smoothness", float ) = 0.5
		
	}

	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off

		Pass {
			CGPROGRAM
			#pragma exclude_renderers ps3 xbox360
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			// uniforms
			uniform sampler2D _MainTex;
			uniform sampler2D _ClearScreen;
			uniform half _Value;
			uniform float2 _Size;
			uniform float _Smoothness;
            float4 _MainTex_TexelSize;

			float rand( float2 co ) {
			
				float x = sin( dot( co.xy, float2( 12.9898, 78.233 ) ) ) * 43758.5453;
				return x - floor( x );
				
			}
			
			fixed4 frag( v2f_img i ) : COLOR {
			
	         	 float2 uv = i.uv;
	         	 float2 ouv = uv;
	         	 #if UNITY_UV_STARTS_AT_TOP
				 if (_MainTex_TexelSize.y < 0) uv.y = 1 - uv.y;
				 #endif
				
				fixed4 t1 = tex2D(_MainTex, ouv);
				fixed4 t2 = tex2D(_ClearScreen, uv);
				if (t1.a - t2.a == 0 && t1.r - t2.r == 0 && t1.g - t2.g == 0 && t1.b - t2.b == 0) discard;
				
				float r = rand( floor( _Size.xy * uv ) );
				float m = smoothstep( 0.0, -_Smoothness, r - ( _Value * ( 1.0 + _Smoothness ) ) );

				return lerp(t1, t2, m );
				
			}

			ENDCG
		} // end Pass
	} // end SubShader

	FallBack "Diffuse"
}