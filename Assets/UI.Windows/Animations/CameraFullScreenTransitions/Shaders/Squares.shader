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

			float rand( float2 co ) {
			
				float x = sin( dot( co.xy, float2( 12.9898, 78.233 ) ) ) * 43758.5453;
				return x - floor( x );
				
			}
			
			fixed4 frag( v2f_img i ) : COLOR {
			
				float r = rand( floor( _Size.xy * i.uv ) );
				float m = smoothstep( 0.0, -_Smoothness, r - ( _Value * ( 1.0 + _Smoothness ) ) );

				return lerp(tex2D(_MainTex, i.uv), tex2D(_ClearScreen, i.uv), m );
				
			}

			ENDCG
		} // end Pass
	} // end SubShader

	FallBack "Diffuse"
}