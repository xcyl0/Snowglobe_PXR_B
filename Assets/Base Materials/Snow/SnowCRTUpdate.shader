Shader "Hidden/SnowCRTUpdate"
{
    SubShader
    {
        Tags{ "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "CustomRenderTextureUpdate" // name is fine even if your UI doesn't show it
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCustomRenderTexture.cginc"

            float4 _WorldMin, _WorldMax;

            int    _StampCount;
            float4 _StampPos[16];
            float  _StampRadius[16];
            float  _StampStrength[16];
            float  _StampHardness;
            float  _Damping;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert (appdata v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            inline float2 WorldToUV(float3 w)
            {
                float2 minv = _WorldMin.xz;
                float2 maxv = _WorldMax.xz;
                float2 size = max(maxv - minv, float2(1e-4,1e-4));
                return saturate((w.xz - minv) / size);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float h = tex2D(_SelfTexture2D, i.uv).r; // previous frame

                int count = clamp(_StampCount, 0, 16);
                float2 size = max(_WorldMax.xz - _WorldMin.xz, float2(1e-4,1e-4));
                float invMaxExtent = 1.0 / max(max(size.x, size.y), 1e-4);

                for (int k = 0; k < count; k++)
                {
                    float2 uvStamp = WorldToUV(_StampPos[k].xyz);
                    float uvR = _StampRadius[k] * invMaxExtent;
                    float d = distance(i.uv, uvStamp);
                    float falloff = saturate(1.0 - pow(saturate(d / max(uvR,1e-5)), max(_StampHardness,0.1)));
                    h = saturate(h - _StampStrength[k] * falloff);
                }

                h = lerp(1.0, h, saturate(_Damping)); // recovery (1=frozen)
                return fixed4(h,h,h,1);
            }
            ENDCG
        }
    }
}
