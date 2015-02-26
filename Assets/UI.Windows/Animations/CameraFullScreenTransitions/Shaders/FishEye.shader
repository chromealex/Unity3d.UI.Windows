Shader "UI.Windows/Transitions/Fish Eye" {

	Properties {
		
		 //[HideInInspector]
		 _MainTex("From (RGB)", 2D) = "white" {}
         //[HideInInspector]
         _ClearScreen("ClearScreen (RGB)", 2D) = "white" {}
         _Value("Value", Range(0, 1)) = 0
         
		_Size ( "Size", Range( 0.0, 0.4 ) ) = 0.2
		_Zoom ( "Zoom", Range( 0.0, 150.0 ) ) = 100.0
		//_ColorSeparation ( "Color Separation", Range( 0.0, 5.0 ) ) = 0.2
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
			sampler2D _MainTex;
			sampler2D _ClearScreen;
			uniform float _Value;
			uniform float _Size;
			uniform float _Zoom;
			//uniform float _ColorSeparation;
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
				value = (1 - value) * 0.5;
				
				float inv = 1.0 - _Value;
				float2 disp = _Size * half2( cos( _Zoom * uv.x ), sin( _Zoom * uv.y ) );

				half4 texFrom = half4
				(
					tex2D( _MainTex, uv + value * disp * ( 1.0 /*- _ColorSeparation*/ ) ).r,
					tex2D( _MainTex, uv + value * disp ).g,
					tex2D( _MainTex, uv + value * disp * ( 1.0 /*+ _ColorSeparation*/ ) ).b,
				1.0 );

				return t2 * _Value + texFrom * inv;
			}

			ENDCG
		} // end Pass
	} // end SubShader

	FallBack "Diffuse"
}