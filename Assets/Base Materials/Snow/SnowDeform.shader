Shader "VRChat/SnowDeform"
{
    Properties
    {
        // Base surface (below snow)
        _BaseColor("Base Albedo", Color) = (0.5,0.5,0.5,1)
        _BaseTex("Base Albedo (RGB) Smoothness (A)", 2D) = "white" {}
        _BaseNormal("Base Normal", 2D) = "bump" {}
        _BaseMetal("Base Metallic", Range(0,1)) = 0.0
        _BaseSmooth("Base Smoothness", Range(0,1)) = 0.4

        // Snow layer
        _SnowColor("Snow Albedo Tint", Color) = (0.92,0.95,1,1)
        _SnowTex("Snow Albedo (RGB) Smoothness (A)", 2D) = "white" {}
        _SnowNormal("Snow Normal", 2D) = "bump" {}
        _SnowSmooth("Snow Smoothness", Range(0,1)) = 0.7
        _SnowMetal("Snow Metallic", Range(0,1)) = 0.0

        // Triplanar scale for snow
        _SnowTiling("Snow Tiling", Float) = 0.3

        // Snow height / thickness
        _SnowDepth("Max Snow Height (m)", Range(0,0.2)) = 0.08
        _SnowBlendHeight("Snow Cover Threshold", Range(0,1)) = 0.3
        _SnowSharpness("Snow Rim Sharpness", Range(0.5,8)) = 3.0

        // Heightmap bounds in world space
        _WorldMin("World Bounds Min (xyz)", Vector) = (-50,-50,-50)
        _WorldMax("World Bounds Max (xyz)", Vector) = ( 50, 50,  50)

        // Optional: limit displacement on steep slopes
        _SlopeLimit("Max Slope Angle Affects Snow", Range(0,1)) = 0.8 // 0=ignore,1=full effect

        // Lighting tweaks
        _SnowAOBoost("Snow AO Boost", Range(0,1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard addshadow fullforwardshadows vertex:vert
        #pragma target 3.0
        #pragma multi_compile_instancing

        sampler2D _BaseTex;
        sampler2D _BaseNormal;
        half _BaseMetal, _BaseSmooth;
        fixed4 _BaseColor;

        sampler2D _SnowTex;
        sampler2D _SnowNormal;
        half _SnowSmooth, _SnowMetal;
        fixed4 _SnowColor;
        float _SnowTiling;

        // Global snow heightmap (R channel used)
        sampler2D _SnowHeightRT;

        float _SnowDepth;
        float _SnowBlendHeight;
        float _SnowSharpness;

        float4 _WorldMin;
        float4 _WorldMax;

        float _SlopeLimit;
        float _SnowAOBoost;

        struct Input
        {
            float2 uv_BaseTex;
            float3 worldPos;
            float3 worldNormal;
        };

        // --- Utilities ---

        // World->UV mapping into heightmap using XY over world bounds.
        float2 WorldToHeightUV(float3 wpos)
        {
            float2 minv = _WorldMin.xz;
            float2 maxv = _WorldMax.xz;
            float2 uv = (wpos.xz - minv) / maxv - minv; // (w.xz - min)/(max-min)
            return saturate(uv);
        }

        // Basic triplanar sampling (no fancy blending for speed)
        // Returns albedo (RGB) and smoothness (A) from snow texture.
        float4 TriplanarTex(sampler2D tex, float3 wpos, float3 n, float tiling)
        {
            n = abs(n);
            n = max(n, 1e-5);
            float sum = n.x + n.y + n.z;
            n /= sum;

            float3 p = wpos * tiling;

            float4 x = tex2D(tex, p.zy);
            float4 y = tex2D(tex, p.xz);
            float4 z = tex2D(tex, p.xy);

            return x * n.x + y * n.y + z * n.z;
        }

        // Triplanar normal (simple unpack & blend)
        float3 TriplanarNormal(sampler2D tex, float3 wpos, float3 n, float tiling)
        {
            n = abs(n);
            n = max(n, 1e-5);
            float sum = n.x + n.y + n.z;
            n /= sum;

            float3 p = wpos * tiling;
            float3 nx = UnpackNormal(tex2D(tex, p.zy));
            float3 ny = UnpackNormal(tex2D(tex, p.xz));
            float3 nz = UnpackNormal(tex2D(tex, p.xy));

            float3 blended = normalize(nx * n.x + ny * n.y + nz * n.z);
            return blended;
        }

        // Sample height; brighter = untouched snow. We push down by (1 - height).
        float SampleSnowAmount(float3 wpos)
        {
            float2 uv = WorldToHeightUV(wpos);
            float h = tex2D(_SnowHeightRT, uv).r; // 0..1
            return saturate(h);
        }

        // Vertex modifier: compress snow along normal based on heightmap and slope
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 wnorm = UnityObjectToWorldNormal(v.normal);

            // Slope factor: reduce displacement on steep slopes if desired
            float slopeCos = saturate(dot(normalize(wnorm), float3(0,1,0))); // 1=flat, 0=vertical
            float slopeWeight = lerp(1.0, slopeCos, _SlopeLimit);

            float snowH = SampleSnowAmount(wpos); // 1 fresh -> 0 fully compressed
            float compress = (1.0 - snowH) * _SnowDepth * slopeWeight;

            // Move along normal inward
            v.vertex.xyz -= v.normal * compress;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Base surface
            fixed4 baseTex = tex2D(_BaseTex, IN.uv_BaseTex) * _BaseColor;
            float3 baseN = UnpackNormal(tex2D(_BaseNormal, IN.uv_BaseTex));
            float baseSmooth = _BaseSmooth;
            float baseMetal = _BaseMetal;

            // Snow layer via triplanar
            float3 wn = normalize(IN.worldNormal);
            float4 snowTex = TriplanarTex(_SnowTex, IN.worldPos, wn, _SnowTiling);
            float3 snowN = TriplanarNormal(_SnowNormal, IN.worldPos, wn, _SnowTiling);
            float snowSmooth = _SnowSmooth;
            float snowMetal = _SnowMetal;

            // Blend factor from heightmap: higher height => more snow coverage
            float snowH = SampleSnowAmount(IN.worldPos);
            // Push coverage higher so shallow dents expose base sooner
            float cover = saturate(pow(saturate((snowH - _SnowBlendHeight) / max(1e-4, (1.0 - _SnowBlendHeight))), _SnowSharpness));

            // Albedo
            float3 snowAlbedo = snowTex.rgb * _SnowColor.rgb;
            float3 albedo = lerp(baseTex.rgb, snowAlbedo, cover);

            // Normals
            float3 nrm = normalize(lerp(baseN, snowN, cover));

            // Smoothness/Metal
            float smooth = lerp(baseSmooth, snowSmooth, cover);
            float metal = lerp(baseMetal, snowMetal, cover);

            // Output
            o.Albedo = albedo;
            o.Normal = nrm;
            o.Smoothness = smooth;
            o.Metallic = metal;

            // Small AO-ish darkening in dents to sell depth
            o.Occlusion = saturate(1.0 - (1.0 - snowH) * _SnowAOBoost);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
