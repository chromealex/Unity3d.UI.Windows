Shader "UI.Windows/Transitions/Color Mask" {
     Properties {
         [HideInInspector] _MainTex("From (RGB)", 2D) = "white" {}
         [HideInInspector] _ClearScreen("ClearScreen (RGB)", 2D) = "white" {}
         _Value("Value", Range(0, 1)) = 0
         
         _Mask("Mask (RGB)", 2D) = "white" {}
     }
     SubShader {
         Tags { "RenderType"="Opaque" }
         LOD 200
         
         CGPROGRAM
         #pragma surface surf Lambert
 
		#include "UnityCG.cginc"

         sampler2D _MainTex;
         half4 _MainTex_TexelSize;
         sampler2D _ClearScreen;
         sampler2D _Mask;
         fixed _Value;
 
         struct Input {
             float2 uv_MainTex;
             float2 uv_Mask;
         };
 
         void surf(Input IN, inout SurfaceOutput o) {
         
         	 float2 uv = IN.uv_MainTex;
         	 float2 ouv = uv;
         	 #if UNITY_UV_STARTS_AT_TOP
			 if (_MainTex_TexelSize.y < 0) uv.y = 1 - uv.y;
			 #endif
			
             half4 t1 = tex2D(_MainTex, ouv);
             half4 t2 = tex2D(_ClearScreen, uv);
			 if (t1.a - t2.a == 0 && t1.r - t2.r == 0 && t1.g - t2.g == 0 && t1.b - t2.b == 0) discard;
			 
             half4 g = tex2D(_Mask, IN.uv_Mask);
             
             if ((g.r + g.g + g.b) * 0.33333f < _Value) {
             
                 o.Albedo = t2.rgb;
                 
             } else {
             
                 o.Albedo = t1.rgb;
                 
             }
             
             o.Alpha = t1.a;//lerp(c.a, d.a, _Value);
             
         }
         
         ENDCG
     } 
     FallBack "Diffuse"
 }