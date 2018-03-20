Shader "ME/DevicePreview"
{
    Properties
    {
    }
    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always
        //Blend One OneMinusSrcAlpha//SrcAlpha OneMinusSrcAlpha

        GrabPass {
            "_GrabTex"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
   
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uvgrab : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                #if UNITY_UV_STARTS_AT_TOP
                float scale = -1.0;
                #else
                float scale = 1.0;
                #endif
                o.uvgrab.xy = (half2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                
                return o;
            }

            sampler2D _GrabTex;

            fixed4 frag (v2f i) : SV_Target
            {
                half4 screenCol = tex2D(_GrabTex, i.uv);
                return screenCol * 2;
            }
            ENDCG
        }
    }
}