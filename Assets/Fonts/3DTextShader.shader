﻿Shader "RGSK/3DText"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
    }
 
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1"}
     
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
         
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
         
            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
         
            struct appdata {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
 
            float4 _MainTex_ST;
         
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.color = v.color;
                o.uv    = TRANSFORM_TEX (v.texcoord, _MainTex);
                return o;
            }
         
            fixed4 frag (v2f o) : COLOR
            {
                // this gives us text or not based on alpha, apparently
                o.color.a  *= tex2D( _MainTex, o.uv ).a;
                return o.color;
            }
            ENDCG
        }
    }
}