Shader "Sprites/ToonOutlineWarmGlow"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)

        // Toon
        _LightDir ("Light Dir (x,y,z)", Vector) = (0.5,0.5,1,0)
        _BrightColor ("Bright Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.25,0.28,0.35,1)
        _Threshold ("Toon Threshold", Range(0,1)) = 0.5

        // Outline
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineThickness ("Outline (px)", Range(0,8)) = 2

        // Warm Glow (내부 전체적으로 빛나는 효과)
        _WarmTintColor ("Warm Tint Color", Color) = (1.0, 0.92, 0.45, 1)
        _WarmTintStrength ("Warm Tint Strength", Range(0,1)) = 0.35
        _AmbientAddColor ("Ambient Add Color", Color) = (1.0, 0.9, 0.4, 1)
        _AmbientAddStrength ("Ambient Add Strength", Range(0,2)) = 0.35
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off

        // ---------- PASS 1: Outline ----------
        Pass
        {
            Name "OUTLINE"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineThickness;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {
                float a0 = tex2D(_MainTex, i.uv).a;
                float2 px = (_MainTex_TexelSize.zw>0)?_MainTex_TexelSize.xy:float2(1.0/_ScreenParams.x,1.0/_ScreenParams.y);
                int r = (int)_OutlineThickness;
                float aMax = a0;
                for(int x=-r;x<=r;x++){
                    for(int y=-r;y<=r;y++){
                        if(x*x+y*y>r*r) continue;
                        aMax = max(aMax, tex2D(_MainTex, i.uv+float2(x*px.x,y*px.y)).a);
                    }
                }
                float mask = saturate(aMax - a0);
                if(mask<=0.0001) discard;

                fixed4 col = _OutlineColor;
                col.a *= mask;
                return col;
            }
            ENDCG
        }

        // ---------- PASS 2: Main Body with Warm Glow ----------
        Pass
        {
            Name "TOON"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;
            float4 _Color, _BrightColor, _ShadowColor, _LightDir;
            float _Threshold;
            float4 _WarmTintColor; float _WarmTintStrength;
            float4 _AmbientAddColor; float _AmbientAddStrength;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 col:COLOR; };

            v2f vert(appdata v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv,_MainTex);
                o.col = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * i.col;

                // Toon 조명
                float3 L = normalize(_LightDir.xyz);
                float3 N = float3(0,0,1);
                float stepVal = (dot(N,L) > _Threshold) ? 1.0 : 0.0;
                float3 lit = lerp(_ShadowColor.rgb, _BrightColor.rgb, stepVal);

                // 기본 색
                float3 baseCol = tex.rgb * lit;

                // ① Warm Tint (노란 톤 섞기)
                baseCol = lerp(baseCol, baseCol * _WarmTintColor.rgb, _WarmTintStrength);

                // ② Ambient Add (전체적으로 환해짐)
                baseCol += _AmbientAddColor.rgb * (_AmbientAddStrength * tex.a);

                fixed4 c;
                c.rgb = baseCol;
                c.a   = tex.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
