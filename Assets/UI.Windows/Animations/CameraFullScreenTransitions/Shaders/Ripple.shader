Shader "UI.Windows/Transitions/Ripple" {

	Properties {
	
		 [HideInInspector] _MainTex("From (RGB)", 2D) = "white" {}
         [HideInInspector] _ClearScreen("ClearScreen (RGB)", 2D) = "white" {}
         _Value("Value", Range(0, 1)) = 0
         
		_Amplitude ( "Amplitude", Float ) = 10.0
		_Speed ( "Speed", Float ) = 5.0
		
	}

	SubShader {
	
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

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
			uniform fixed _Value;
			uniform float _Amplitude;
			uniform float _Speed;
            float4 _MainTex_TexelSize;


			fixed4 frag( v2f_img i ) : COLOR {
			
	         	 float2 uv = i.uv;
	         	 float2 ouv = uv;
	         	 #if UNITY_UV_STARTS_AT_TOP
				 if (_MainTex_TexelSize.y < 0) uv.y = 1 - uv.y;
				 #endif
				
				fixed4 t1 = tex2D(_MainTex, ouv);
				fixed4 t2 = tex2D(_ClearScreen, uv);
				if (t1.a - t2.a == 0 && t1.r - t2.r == 0 && t1.g - t2.g == 0 && t1.b - t2.b == 0) discard;
				
				fixed value = 1 - _Value;
				if (value < 0.5) value = 1 - value;
			
				half2 dir = uv - half2( 0.5, 0.5 );
				float dist = length( dir );
				half2 offset = dir * ( sin( _Time.x * dist * _Amplitude * value - value * _Speed) + 0.5 ) / 30.0;

				return lerp( lerp( tex2D( _MainTex, ouv + offset ), tex2D(_ClearScreen, uv + offset), _Value), fixed4( 0.0, 0.0, 0.0, 0.0 ), smoothstep( 0.5, 1.0, value ) );
				
			}

			ENDCG
			
		} // end Pass
		
	} // end SubShader

	FallBack "Diffuse"
}