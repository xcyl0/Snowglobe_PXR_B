Shader "VRChat/SnowCRTDeform"
{
    Properties
    {
        // World & CRT
        _WorldMin("World Bounds Min (xyz)", Vector) = (-50,-10,-50,0)
        _WorldMax("World Bounds Max (xyz)", Vector) = ( 50, 50, 50,0)
        _SnowHeight("Snow Height CRT (R)", 2D) = "white" {}

        // Base (ground below snow)
        _BaseColor("Base Albedo Tint", Color) = (0.8,0.8,0.8,1)
        _BaseTex("Base Albedo (RGB) Smoothness (A)", 2D) = "white" {}
        _BaseNormal("Base Normal", 2D) = "bump" {}
        _BaseMetal("Base Metallic", Range(0,1)) = 0.0
        _BaseSmooth("Base Smoothness", Range(0,1)) = 0.4

        // Snow
        _SnowColor("Snow Albedo Tint", Color) = (0.92,0.95,1,1)
        _SnowTex("Snow Albedo (RGB) Smoothness (A)", 2D) = "white" {}
        _SnowNormal("Snow Normal", 2D) = "bump" {}
        _SnowSmooth("Snow Smoothness", Range(0,1)) = 0.7
        _SnowMetal("Snow Metallic", Range(0,1)) = 0.0
        _SnowTiling("Snow Triplanar Tiling (tiles/m)", Float) = 0.3

        // Deform / Blend
        _SnowDepth("Max Snow Height (m)", Range(0,0.25)) = 0.08
        _SnowBlendHeight("Snow Cover Threshold", Range(0,0.99)) = 0.3
        _SnowSharpness("Snow Rim Sharpness", Range(0.5,8)) = 3.0
        _SlopeLimit("Max Slope Affects Snow", Range(0,1)) = 0.8
        _SnowAOBoost("Dent AO Boost", Range(0,1)) = 0.2

        // Debug / Safety
        [Toggle] _DebugFlat("Debug Flat (no snow, no CRT)", Float) = 0
        [Toggle] _UseHeight("Use Height Map (enable CRT sampling)", Float) = 1
        [Toggle] _DebugHeight("Debug Height (show CRT)", Float) = 0
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        CGPROGRAM
        #pragma target 4.0
        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
        #pragma multi_compile_instancing

        sampler2D _BaseTex, _BaseNormal;
        half _BaseMetal, _BaseSmooth;
        fixed4 _BaseColor;

        sampler2D _SnowTex, _SnowNormal;
        half _SnowSmooth, _SnowMetal;
        fixed4 _SnowColor;
        float _SnowTiling;

        sampler2D _SnowHeight;

        float4 _WorldMin, _WorldMax;
        float _SnowDepth, _SnowBlendHeight, _SnowSharpness, _SlopeLimit, _SnowAOBoost;
        float _DebugFlat, _UseHeight, _DebugHeight;

        struct Input {
            float2 uv_BaseTex;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        // ---------- Helpers ----------
        inline float2 WorldToUV(float3 wpos)
        {
            float2 minv = _WorldMin.xz;
            float2 maxv = _WorldMax.xz;
            float2 size = max(maxv - minv, float2(1e-3,1e-3));
            float2 uv = (wpos.xz - minv) / size;
            uv = frac(uv);
            return saturate(uv);
        }

        inline float2 TriUV(float a, float b, float tiling)
        {
            float t = max(tiling, 1e-4);
            return frac(float2(a, b) * t);
        }

        // ---- CHANGED: no rcp; denominator is sum + 1e-5 to silence div-by-0
        inline float4 TriTex(sampler2D tex, float3 wp, float3 wn, float tiling)
        {
            float3 w = abs(wn);
            float sum = w.x + w.y + w.z + 1e-5;
            w = w / sum;

            float4 x = tex2D(tex, TriUV(wp.z, wp.y, tiling));
            float4 y = tex2D(tex, TriUV(wp.x, wp.z, tiling));
            float4 z = tex2D(tex, TriUV(wp.x, wp.y, tiling));
            return x*w.x + y*w.y + z*w.z;
        }

        inline float3 SafeUnpackNormal(sampler2D tex, float2 uv)
        {
            float3 n = UnpackNormal(tex2D(tex, uv));
            float len2 = dot(n, n);
            if (!(len2 > 1e-4)) n = float3(0,0,1);
            return normalize(n);
        }

        // ---- CHANGED: same safe normalization as TriTex
        inline float3 SafeTriNorm(sampler2D tex, float3 wp, float3 wn, float tiling)
        {
            float3 w = abs(wn);
            float sum = w.x + w.y + w.z + 1e-5;
            w = w / sum;

            float2 uvx = TriUV(wp.z, wp.y, tiling);
            float2 uvy = TriUV(wp.x, wp.z, tiling);
            float2 uvz = TriUV(wp.x, wp.y, tiling);

            float3 nx = UnpackNormal(tex2D(tex, uvx));
            float3 ny = UnpackNormal(tex2D(tex, uvy));
            float3 nz = UnpackNormal(tex2D(tex, uvz));
            float3 n = nx*w.x + ny*w.y + nz*w.z;

            float len2 = dot(n, n);
            if (!(len2 > 1e-4)) n = float3(0,0,1);
            return normalize(n);
        }

        inline float SampleSnowLOD(float3 wp)
        {
            if (_UseHeight < 0.5) return 1.0;
            float2 uv = WorldToUV(wp);
            return tex2Dlod(_SnowHeight, float4(uv, 0, 0)).r;
        }

        inline float SampleSnow(float3 wp)
        {
            if (_UseHeight < 0.5) return 1.0;
            return tex2D(_SnowHeight, WorldToUV(wp)).r;
        }

        // ---------- Vertex: apply compression ----------
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            if (_DebugFlat > 0.5) return;

            float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 wnorm = UnityObjectToWorldNormal(v.normal);

            float slopeCos = saturate(dot(normalize(wnorm), float3(0,1,0)));
            float slopeW = lerp(1.0, slopeCos, saturate(_SlopeLimit));

            float h = saturate(SampleSnowLOD(wpos));
            float compress = (1.0 - h) * saturate(_SnowDepth) * slopeW;

            v.vertex.xyz -= v.normal * compress;
        }

        // ---------- Surface: blend base/snow ----------
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Debug: flat lit base path
            if (_DebugFlat > 0.5)
            {
                fixed4 c = tex2D(_BaseTex, IN.uv_BaseTex) * _BaseColor;
                float3 n = SafeUnpackNormal(_BaseNormal, IN.uv_BaseTex);
                o.Albedo     = c.rgb;
                o.Normal     = n;
                o.Smoothness = _BaseSmooth;
                o.Metallic   = _BaseMetal;
                o.Occlusion  = 1.0;
                return;
            }

            // Debug: visualize height texture
            if (_DebugHeight > 0.5)
            {
                float h = saturate(SampleSnow(IN.worldPos));
                o.Albedo = h.xxx;
                o.Normal = float3(0,0,1);
                o.Smoothness = 0.0;
                o.Metallic = 0.0;
                o.Occlusion = 1.0;
                return;
            }

            // Base
            fixed4 baseTex = tex2D(_BaseTex, IN.uv_BaseTex) * _BaseColor;
            float3 baseN = SafeUnpackNormal(_BaseNormal, IN.uv_BaseTex);

            // Snow (triplanar)
            float3 wn = normalize(IN.worldNormal);
            float tiling = max(_SnowTiling, 1e-4);
            float4 snowTex = TriTex(_SnowTex, IN.worldPos, wn, tiling);
            float3 snowN  = SafeTriNorm(_SnowNormal, IN.worldPos, wn, tiling);

            // Height → coverage (already guarded)
            float h = saturate(SampleSnow(IN.worldPos));
            float bh = saturate(_SnowBlendHeight);
            float denom = max(1e-3, 1.0 - bh);
            float cover01 = saturate((h - bh) / denom);
            float cover = saturate(pow(cover01, max(_SnowSharpness, 0.5)));

            // Blend
            float3 snowAlb = snowTex.rgb * _SnowColor.rgb;
            o.Albedo     = lerp(baseTex.rgb, snowAlb, cover);
            o.Normal     = normalize(lerp(baseN, snowN, cover));
            o.Smoothness = lerp(_BaseSmooth, _SnowSmooth, cover);
            o.Metallic   = lerp(_BaseMetal,  _SnowMetal,  cover);
            o.Occlusion  = saturate(1.0 - (1.0 - h) * saturate(_SnowAOBoost));
        }
        ENDCG
    }
}
