Shader "Hidden/Snow/HeightBrush"
{
    Properties
    {
        _MainTex ("Height RT (R)", 2D) = "white" {}
        _BrushPos ("Brush Pos (world xyz)", Vector) = (0,0,0,0)
        _BrushRadius ("Radius (meters)", Float) = 0.5
        _Strength ("Strength per frame", Range(-1,1)) = 0.2
        _WorldMin ("World Min", Vector) = (-50,-50,-50,0)
        _WorldMax ("World Max", Vector) = ( 50, 50, 50,0)
        _Hardness ("Falloff Hardness", Range(0.1,4)) = 2.0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _BrushPos;
            float _BrushRadius;
            float _Strength;
            float4 _WorldMin;
            float4 _WorldMax;
            float _Hardness;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            float2 WorldToUV(float3 w)
            {
                float2 minv = _WorldMin.xz;
                float2 maxv = _WorldMax.xz;
                return saturate((w.xz - minv) / (maxv - minv));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float h = tex2D(_MainTex, i.uv).r;

                // Compute brush influence at this texel
                float2 brushUV = WorldToUV(_BrushPos.xyz);
                float d = distance(i.uv, brushUV); // texture space distance
                // Convert world radius to UV radius
                float2 minv = _WorldMin.xz;
                float2 maxv = _WorldMax.xz;
                float2 size = maxv - minv;
                float uvRadius = _BrushRadius / max(size.x, size.y);

                float falloff = saturate(1.0 - pow(saturate(d / max(uvRadius, 1e-4)), _Hardness));

                // Subtract to make a dent (positive _Strength compresses snow)
                h = saturate(h - _Strength * falloff);

                return fixed4(h, h, h, 1);
            }
            ENDCG
        }
    }
}
