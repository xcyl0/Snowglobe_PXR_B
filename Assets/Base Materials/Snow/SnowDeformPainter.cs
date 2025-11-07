using UnityEngine;

[ExecuteAlways]
public class SnowDeformPainter : MonoBehaviour
{
    [Header("World Bounds (match your play area/terrain extents)")]
    public Vector3 worldMin = new Vector3(-50, -10, -50);
    public Vector3 worldMax = new Vector3(50, 50, 50);

    [Header("Heightmap")]
    public int heightMapResolution = 1024;
    public RenderTextureFormat heightFormat = RenderTextureFormat.R8;
    public FilterMode heightFilter = FilterMode.Bilinear;
    public string globalTextureName = "_SnowHeightRT";

    [Header("Brush")]
    public Shader brushShader;
    public float brushRadiusMeters = 0.4f;
    [Range(-1f, 1f)] public float brushStrength = 0.15f; // + compress; - add snow
    [Range(0.1f, 4f)] public float brushHardness = 2.0f;

    [Header("Targets")]
    public LayerMask paintLayers = ~0; // which layers deform snow
    public bool useCollision = true;

    Material _brushMat;
    RenderTexture _rt;

    static readonly int ID_MainTex = Shader.PropertyToID("_MainTex");
    static readonly int ID_BrushPos = Shader.PropertyToID("_BrushPos");
    static readonly int ID_BrushRadius = Shader.PropertyToID("_BrushRadius");
    static readonly int ID_Strength = Shader.PropertyToID("_Strength");
    static readonly int ID_WorldMin = Shader.PropertyToID("_WorldMin");
    static readonly int ID_WorldMax = Shader.PropertyToID("_WorldMax");
    static readonly int ID_Hardness = Shader.PropertyToID("_Hardness");

    static readonly int ID_GWorldMin = Shader.PropertyToID("_WorldMin");
    static readonly int ID_GWorldMax = Shader.PropertyToID("_WorldMax");

    void OnEnable()
    {
        EnsureResources();
        Shader.SetGlobalVector(ID_GWorldMin, new Vector4(worldMin.x, worldMin.y, worldMin.z, 0));
        Shader.SetGlobalVector(ID_GWorldMax, new Vector4(worldMax.x, worldMax.y, worldMax.z, 0));
        Shader.SetGlobalTexture(globalTextureName, _rt);
    }

    void OnDisable()
    {
        if (_rt) RenderTexture.ReleaseTemporary(_rt);
        _rt = null;
        if (_brushMat) DestroyImmediate(_brushMat);
        _brushMat = null;
    }

    void EnsureResources()
    {
        if (_rt == null || _rt.width != heightMapResolution)
        {
            if (_rt) RenderTexture.ReleaseTemporary(_rt);
            _rt = RenderTexture.GetTemporary(heightMapResolution, heightMapResolution, 0, heightFormat);
            _rt.wrapMode = TextureWrapMode.Clamp;
            _rt.filterMode = heightFilter;
            _rt.Create();

            // Start full white = untouched snow (height=1)
            var temp = RenderTexture.active;
            RenderTexture.active = _rt;
            GL.Clear(false, true, Color.white);
            RenderTexture.active = temp;
        }

        if (_brushMat == null)
        {
            if (!brushShader) brushShader = Shader.Find("Hidden/Snow/HeightBrush");
            _brushMat = new Material(brushShader);
        }
    }

    void Update()
    {
        // keep globals in sync (in case you tweak in editor)
        Shader.SetGlobalVector(ID_GWorldMin, new Vector4(worldMin.x, worldMin.y, worldMin.z, 0));
        Shader.SetGlobalVector(ID_GWorldMax, new Vector4(worldMax.x, worldMax.y, worldMax.z, 0));
        Shader.SetGlobalTexture(globalTextureName, _rt);
    }

    // Public API to paint at a world position (e.g., footsteps)
    public void PaintAt(Vector3 worldPos, float radiusMeters, float strength)
    {
        EnsureResources();
        _brushMat.SetTexture(ID_MainTex, _rt);
        _brushMat.SetVector(ID_BrushPos, new Vector4(worldPos.x, worldPos.y, worldPos.z, 0));
        _brushMat.SetFloat(ID_BrushRadius, radiusMeters);
        _brushMat.SetFloat(ID_Strength, strength);
        _brushMat.SetFloat(ID_Hardness, brushHardness);
        _brushMat.SetVector(ID_WorldMin, new Vector4(worldMin.x, worldMin.y, worldMin.z, 0));
        _brushMat.SetVector(ID_WorldMax, new Vector4(worldMax.x, worldMax.y, worldMax.z, 0));

        // Blit in-place
        RenderTexture tmp = RenderTexture.GetTemporary(_rt.descriptor);
        Graphics.Blit(_rt, tmp);
        Graphics.Blit(tmp, _rt, _brushMat, 0);
        RenderTexture.ReleaseTemporary(tmp);
    }

    // Example: stamp where colliders touch
    void OnCollisionStay(Collision col)
    {
        if (!useCollision) return;
        if (((1 << col.gameObject.layer) & paintLayers) == 0) return;

        foreach (var c in col.contacts)
        {
            PaintAt(c.point, brushRadiusMeters, brushStrength * Time.deltaTime * 60f);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!useCollision) return;
        if (((1 << other.gameObject.layer) & paintLayers) == 0) return;

        // Approximate with closest point to ground
        Vector3 p = other.ClosestPoint(transform.position);
        PaintAt(p, brushRadiusMeters, brushStrength * Time.deltaTime * 60f);
    }
}
