Shader "UI.Windows/Transitions/Fish Eye" {

	Properties {
		
		 //[HideInInspector]
		 _MainTex("From (RGB)", 2D) = "white" {}
         //[HideInInspector]
         _ClearScreen("ClearScreen (RGB)", 2D) = "white" {}
         _Value("Value", Range(0, 1)) = 0
         
		_Size ( "Size", Range( 0.0, 0.4 ) ) = 0.2
		_Zoom ( "Zoom", Range( 0.0, 150.0 ) ) = 100.0
		_ColorSeparation ( "Color Separation", Range( 0.0, 5.0 ) ) = 0.2
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
			uniform float _ColorSeparation;


			fixed4 frag( v2f_img i ) : COLOR {
			
				fixed value = _Value;
				if (value < 0.5) value = 1 - value;
				value = (1 - value) * 0.5;
				
				float inv = 1.0 - _Value;
				float2 disp = _Size * half2( cos( _Zoom * i.uv.x ), sin( _Zoom * i.uv.y ) );
				half4 texTo = tex2D( _ClearScreen, i.uv);//half4( 0.0, 0.0, 0.0, 0.0 );

				half4 texFrom = half4
				(
					tex2D( _MainTex, i.uv + value * disp * ( 1.0 - _ColorSeparation ) ).r,
					tex2D( _MainTex, i.uv + value * disp ).g,
					tex2D( _MainTex, i.uv + value * disp * ( 1.0 + _ColorSeparation ) ).b,
				1.0 );

				return texTo * _Value + texFrom * inv;
			}

			ENDCG
		} // end Pass
	} // end SubShader

	FallBack "Diffuse"
}