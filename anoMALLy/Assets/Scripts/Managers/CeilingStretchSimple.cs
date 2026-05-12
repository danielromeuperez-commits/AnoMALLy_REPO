using UnityEngine;

public class CeilingStretchSimple : MonoBehaviour
{
    public enum WorldAxis
    {
        X,
        Z
    }

    public enum LocalScaleAxis
    {
        X,
        Y,
        Z
    }

    public enum TextureAxis
    {
        X,
        Y
    }

    [Header("Player")]
    [SerializeField] Transform player;

    [Header("Managers")]
    [SerializeField] AnomalyManager anomalyManager;

    [Header("Zona final que se aleja")]
    [SerializeField] Transform corridorEndSection;

    [Header("Techos que se estiran")]
    [SerializeField] Transform[] ceilingsToStretch;

    [Header("Dirección hacia delante")]
    [SerializeField] WorldAxis moveAxis = WorldAxis.X;
    [SerializeField] bool invertDirection = true;

    [Header("Eje que escala el techo")]
    [SerializeField] LocalScaleAxis ceilingScaleAxis = LocalScaleAxis.X;

    [Header("Eje del tiling")]
    [SerializeField] TextureAxis textureTilingAxis = TextureAxis.X;

    [Header("Estirado")]
    [SerializeField] float maxStretchDistance = 100f;
    [SerializeField] float followSpeed = 20f;

    [Header("Tiling")]
    [SerializeField] float textureRepeatEveryWorldUnit = 1f;

    [Header("Ajuste manual del tileado")]
    [SerializeField, Range(-5f, 5f)] float manualTilingSpeed = 1f;
    [SerializeField, Range(-5f, 5f)] float manualOffsetSpeed = 0f;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;

    bool isActive;
    bool stretchDisabled;

    Vector3 triggerStartPosition;
    Vector3 corridorEndStartPosition;

    Vector3[] ceilingStartScales;
    Vector3[] ceilingStartPositions;
    Vector3[] fixedBackEdges;
    float[] ceilingStartWorldLengths;
    Material[] ceilingMaterials;
    Vector2[] originalTiling;
    Vector2[] originalOffset;

    private void Start()
    {
        triggerStartPosition = transform.position;

        if (anomalyManager == null)
        {
            anomalyManager = FindAnyObjectByType<AnomalyManager>();
        }

        if (corridorEndSection != null)
        {
            corridorEndStartPosition = corridorEndSection.position;
        }

        SetupCeilings();

        UpdateCeilings(0f);
        UpdateEndSection(0f);
    }

    private void Update()
    {
        if (!isActive || player == null) return;

        if (anomalyManager != null && anomalyManager.AllAnomaliesFixed)
        {
            DisableStretchForever();
            return;
        }

        float stretchAmount = GetPlayerDistanceFromTriggerStart();
        stretchAmount = Mathf.Clamp(stretchAmount, 0f, maxStretchDistance);

        MoveTrigger(stretchAmount);
        UpdateCeilings(stretchAmount);
        UpdateEndSection(stretchAmount);
    }

    void DisableStretchForever()
    {
        if (stretchDisabled) return;

        stretchDisabled = true;
        isActive = false;

        transform.position = triggerStartPosition;

        if (corridorEndSection != null)
        {
            corridorEndSection.position = corridorEndStartPosition;
        }

        RestoreOriginalCeilings();

        if (showDebugLogs)
        {
            Debug.Log(gameObject.name + ": todas las anomalías corregidas. El techo ya no se estira.");
        }
    }

    void RestoreOriginalCeilings()
    {
        for (int i = 0; i < ceilingsToStretch.Length; i++)
        {
            if (ceilingsToStretch[i] == null) continue;

            ceilingsToStretch[i].localScale = ceilingStartScales[i];
            ceilingsToStretch[i].position = ceilingStartPositions[i];

            RestoreOriginalTiling(i);
        }
    }

    void SetupCeilings()
    {
        ceilingStartScales = new Vector3[ceilingsToStretch.Length];
        ceilingStartPositions = new Vector3[ceilingsToStretch.Length];
        fixedBackEdges = new Vector3[ceilingsToStretch.Length];
        ceilingStartWorldLengths = new float[ceilingsToStretch.Length];
        ceilingMaterials = new Material[ceilingsToStretch.Length];
        originalTiling = new Vector2[ceilingsToStretch.Length];
        originalOffset = new Vector2[ceilingsToStretch.Length];

        Vector3 direction = GetForwardDirection();

        for (int i = 0; i < ceilingsToStretch.Length; i++)
        {
            if (ceilingsToStretch[i] == null) continue;

            ceilingStartScales[i] = ceilingsToStretch[i].localScale;
            ceilingStartPositions[i] = ceilingsToStretch[i].position;

            Renderer rend = ceilingsToStretch[i].GetComponent<Renderer>();

            if (rend == null)
            {
                Debug.LogWarning("El techo no tiene Renderer: " + ceilingsToStretch[i].name);
                continue;
            }

            ceilingMaterials[i] = rend.material;

            SaveOriginalTiling(i);

            Bounds bounds = rend.bounds;

            float length = GetBoundsLength(bounds);

            ceilingStartWorldLengths[i] = Mathf.Max(0.01f, length);

            fixedBackEdges[i] = GetBackEdge(bounds, direction);
        }
    }

    void SaveOriginalTiling(int index)
    {
        if (ceilingMaterials[index] == null) return;

        if (ceilingMaterials[index].HasProperty("_BaseMap"))
        {
            originalTiling[index] = ceilingMaterials[index].GetTextureScale("_BaseMap");
            originalOffset[index] = ceilingMaterials[index].GetTextureOffset("_BaseMap");
        }
        else if (ceilingMaterials[index].HasProperty("_MainTex"))
        {
            originalTiling[index] = ceilingMaterials[index].GetTextureScale("_MainTex");
            originalOffset[index] = ceilingMaterials[index].GetTextureOffset("_MainTex");
        }
        else
        {
            originalTiling[index] = Vector2.one;
            originalOffset[index] = Vector2.zero;
        }
    }

    void RestoreOriginalTiling(int index)
    {
        if (ceilingMaterials == null) return;
        if (index < 0 || index >= ceilingMaterials.Length) return;
        if (ceilingMaterials[index] == null) return;

        if (ceilingMaterials[index].HasProperty("_BaseMap"))
        {
            ceilingMaterials[index].SetTextureScale("_BaseMap", originalTiling[index]);
            ceilingMaterials[index].SetTextureOffset("_BaseMap", originalOffset[index]);
        }
        else if (ceilingMaterials[index].HasProperty("_MainTex"))
        {
            ceilingMaterials[index].SetTextureScale("_MainTex", originalTiling[index]);
            ceilingMaterials[index].SetTextureOffset("_MainTex", originalOffset[index]);
        }
    }

    float GetPlayerDistanceFromTriggerStart()
    {
        Vector3 direction = GetForwardDirection();

        float distance = Vector3.Dot(player.position - triggerStartPosition, direction);

        return Mathf.Max(0f, distance);
    }

    Vector3 GetForwardDirection()
    {
        Vector3 direction = moveAxis == WorldAxis.X ? Vector3.right : Vector3.forward;

        if (invertDirection)
            direction *= -1f;

        return direction;
    }

    void MoveTrigger(float stretchAmount)
    {
        if (stretchDisabled) return;

        Vector3 direction = GetForwardDirection();

        Vector3 targetPosition = triggerStartPosition + direction * stretchAmount;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    void UpdateEndSection(float stretchAmount)
    {
        if (stretchDisabled) return;
        if (corridorEndSection == null) return;

        Vector3 direction = GetForwardDirection();

        corridorEndSection.position = corridorEndStartPosition + direction * stretchAmount;
    }

    void UpdateCeilings(float stretchAmount)
    {
        if (stretchDisabled) return;

        Vector3 direction = GetForwardDirection();

        for (int i = 0; i < ceilingsToStretch.Length; i++)
        {
            if (ceilingsToStretch[i] == null) continue;

            Renderer rend = ceilingsToStretch[i].GetComponent<Renderer>();
            if (rend == null) continue;

            float originalLength = Mathf.Max(0.01f, ceilingStartWorldLengths[i]);
            float newLength = originalLength + stretchAmount;
            float scaleMultiplier = newLength / originalLength;

            Vector3 newScale = ceilingStartScales[i];

            switch (ceilingScaleAxis)
            {
                case LocalScaleAxis.X:
                    newScale.x = ceilingStartScales[i].x * scaleMultiplier;
                    break;

                case LocalScaleAxis.Y:
                    newScale.y = ceilingStartScales[i].y * scaleMultiplier;
                    break;

                case LocalScaleAxis.Z:
                    newScale.z = ceilingStartScales[i].z * scaleMultiplier;
                    break;
            }

            ceilingsToStretch[i].localScale = newScale;

            Bounds newBounds = rend.bounds;
            Vector3 currentBackEdge = GetBackEdge(newBounds, direction);

            Vector3 correction = fixedBackEdges[i] - currentBackEdge;

            ceilingsToStretch[i].position += correction;

            UpdateTiling(i, newLength);
        }
    }

    float GetBoundsLength(Bounds bounds)
    {
        if (moveAxis == WorldAxis.X)
            return bounds.size.x;

        return bounds.size.z;
    }

    Vector3 GetBackEdge(Bounds bounds, Vector3 direction)
    {
        float halfLength;

        if (moveAxis == WorldAxis.X)
            halfLength = bounds.extents.x;
        else
            halfLength = bounds.extents.z;

        return bounds.center - direction * halfLength;
    }

    void UpdateTiling(int index, float currentWorldLength)
    {
        if (stretchDisabled) return;

        if (ceilingMaterials == null) return;
        if (index < 0 || index >= ceilingMaterials.Length) return;
        if (ceilingMaterials[index] == null) return;

        float repeatAmount = currentWorldLength / textureRepeatEveryWorldUnit;

        float finalTilingAmount = repeatAmount * manualTilingSpeed;
        float finalOffsetAmount = repeatAmount * manualOffsetSpeed;

        Vector2 tiling = originalTiling[index];
        Vector2 offset = originalOffset[index];

        if (textureTilingAxis == TextureAxis.X)
        {
            tiling.x = finalTilingAmount;
            offset.x = finalOffsetAmount;
        }
        else
        {
            tiling.y = finalTilingAmount;
            offset.y = finalOffsetAmount;
        }

        if (ceilingMaterials[index].HasProperty("_BaseMap"))
        {
            ceilingMaterials[index].SetTextureScale("_BaseMap", tiling);
            ceilingMaterials[index].SetTextureOffset("_BaseMap", offset);
        }
        else if (ceilingMaterials[index].HasProperty("_MainTex"))
        {
            ceilingMaterials[index].SetTextureScale("_MainTex", tiling);
            ceilingMaterials[index].SetTextureOffset("_MainTex", offset);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (stretchDisabled) return;
        if (!other.CompareTag("Player")) return;

        isActive = true;

        if (showDebugLogs)
            Debug.Log("Player tocó CeilingStretchSimple. El techo empieza a estirarse.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (stretchDisabled) return;
        if (!other.CompareTag("Player")) return;

        isActive = true;
    }
}