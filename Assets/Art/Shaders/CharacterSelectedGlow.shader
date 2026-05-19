Shader "Quoridor/UI/Character Selected Glow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,0.82,0.22,1)
        _GlowSize ("Glow Size", Range(0, 12)) = 4
        _GlowStrength ("Glow Strength", Range(0, 3)) = 1.4
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _GlowColor;
            float4 _MainTex_TexelSize;
            float4 _ClipRect;
            float _GlowSize;
            float _GlowStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 sprite = tex2D(_MainTex, IN.texcoord) * IN.color;
                float2 step = _MainTex_TexelSize.xy * _GlowSize;

                float glowAlpha = 0;
                glowAlpha = max(glowAlpha, tex2D(_MainTex, IN.texcoord + float2(step.x, 0)).a);
                glowAlpha = max(glowAlpha, tex2D(_MainTex, IN.texcoord + float2(-step.x, 0)).a);
                glowAlpha = max(glowAlpha, tex2D(_MainTex, IN.texcoord + float2(0, step.y)).a);
                glowAlpha = max(glowAlpha, tex2D(_MainTex, IN.texcoord + float2(0, -step.y)).a);
                glowAlpha = max(glowAlpha, tex2D(_MainTex, IN.texcoord + step).a);
                glowAlpha = max(glowAlpha, tex2D(_MainTex, IN.texcoord - step).a);
                glowAlpha = saturate((glowAlpha - sprite.a) * _GlowStrength);

                fixed4 glow = _GlowColor * glowAlpha;
                fixed4 color = sprite + glow * (1 - sprite.a);
                color.a = max(sprite.a, glow.a);
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                return color;
            }
            ENDCG
        }
    }
}
