Shader "Quoridor/Sprites/Pawn Near Goal Enchant"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EnchantColor ("Enchant Color", Color) = (0.75,0.35,1,1)
        _SecondColor ("Second Color", Color) = (0.25,0.9,1,1)
        _LineDensity ("Line Density", Range(8, 42)) = 24
        _LineWidth ("Line Width", Range(0.02, 0.35)) = 0.11
        _LineSpeed ("Line Speed", Range(-4, 4)) = 1.4
        _GlowStrength ("Glow Strength", Range(0, 2)) = 0.85
        _FlashStrength ("Flash Strength", Range(0, 1)) = 0.22
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _EnchantColor;
            fixed4 _SecondColor;
            float _LineDensity;
            float _LineWidth;
            float _LineSpeed;
            float _GlowStrength;
            float _FlashStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 sprite = tex2D(_MainTex, IN.texcoord) * IN.color;
                float diagonal = IN.texcoord.x + IN.texcoord.y * 0.72 + _Time.y * _LineSpeed;
                float wave = frac(diagonal * _LineDensity);
                float enchantBand = smoothstep(0, _LineWidth, wave) * (1 - smoothstep(_LineWidth, _LineWidth * 2.2, wave));
                float pulse = 0.65 + 0.35 * sin(_Time.y * 5.2 + diagonal * 8.0);
                fixed4 enchant = lerp(_EnchantColor, _SecondColor, saturate(IN.texcoord.y + pulse * 0.25));
                float alpha = sprite.a * saturate(enchantBand * pulse + _FlashStrength) * _GlowStrength;
                return fixed4(enchant.rgb, alpha);
            }
            ENDCG
        }
    }
}
