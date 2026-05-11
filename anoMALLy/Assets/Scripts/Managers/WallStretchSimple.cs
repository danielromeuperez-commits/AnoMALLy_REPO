using UnityEngine;

public class WallStretch : MonoBehaviour
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

    [Header("Paredes que se estiran")]
    [SerializeField] Transform[] floorsToStretch;

    [Header("Dirección hacia delante")]
    [SerializeField] WorldAxis moveAxis = WorldAxis.X;
    [SerializeField] bool invertDirection = true;

    [Header("Eje que escala la pared")]
    [SerializeField] LocalScaleAxis floorScaleAxis = LocalScaleAxis.X;

    [Header("Eje del tiling")]
    [SerializeField] TextureAxis textureTilingAxis = TextureAxis.X;

    [Header("Corrección de tiling")]
    [SerializeField] bool invertTextureTiling = false;
    [SerializeField] bool tileFromEnd = true;

    [Header("Estirado")]
    [SerializeField] float maxStretchDistance = 100f;
    [SerializeField] float followSpeed = 20f;

    [Header("Tiling")]
    [SerializeField] float textureRepeatEveryWorldUnit = 1f;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;

    bool isActive;
    bool stretchDisabled;

    Vector3 triggerStartPosition;
    Vector3 corridorEndStartPosition;

    Vector3[] floorStartScales;
    Vector3[] floorStartPositions;
    Vector3[] fixedBackEdges;
    float[] floorStartWorldLengths;
    Material[] floorMaterials;
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

        SetupFloors();

        UpdateFloors(0f);
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
        UpdateFloors(stretchAmount);
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

        RestoreOriginalWalls();

        if (showDebugLogs)
        {
            Debug.Log(gameObject.name + ": todas las anomalías corregidas. Las paredes ya no se alejan.");
        }
    }

    void RestoreOriginalWalls()
    {
        for (int i = 0; i < floorsToStretch.Length; i++)
        {
            if (floorsToStretch[i] == null) continue;

            floorsToStretch[i].localScale = floorStartScales[i];
            floorsToStretch[i].position = floorStartPositions[i];

            RestoreOriginalTiling(i);
        }
    }

    void RestoreOriginalTiling(int index)
    {
        if (floorMaterials == null) return;
        if (index < 0 || index >= floorMaterials.Length) return;
        if (floorMaterials[index] == null) return;

        if (floorMaterials[index].HasProperty("_BaseMap"))
        {
            floorMaterials[index].SetTextureScale("_BaseMap", originalTiling[index]);
            floorMaterials[index].SetTextureOffset("_BaseMap", originalOffset[index]);
        }
        else if (floorMaterials[index].HasProperty("_MainTex"))
        {
            floorMaterials[index].SetTextureScale("_MainTex", originalTiling[index]);
            floorMaterials[index].SetTextureOffset("_MainTex", originalOffset[index]);
        }
    }

    void SetupFloors()
    {
        floorStartScales = new Vector3[floorsToStretch.Length];
        floorStartPositions = new Vector3[floorsToStretch.Length];
        fixedBackEdges = new Vector3[floorsToStretch.Length];
        floorStartWorldLengths = new float[floorsToStretch.Length];
        floorMaterials = new Material[floorsToStretch.Length];
        originalTiling = new Vector2[floorsToStretch.Length];
        originalOffset = new Vector2[floorsToStretch.Length];

        Vector3 direction = GetForwardDirection();

        for (int i = 0; i < floorsToStretch.Length; i++)
        {
            if (floorsToStretch[i] == null) continue;

            floorStartScales[i] = floorsToStretch[i].localScale;
            floorStartPositions[i] = floorsToStretch[i].position;

            Renderer rend = floorsToStretch[i].GetComponent<Renderer>();

            if (rend == null)
            {
                Debug.LogWarning("La pared no tiene Renderer: " + floorsToStretch[i].name);
                continue;
            }

            floorMaterials[i] = rend.material;

            SaveOriginalTiling(i);

            Bounds bounds = rend.bounds;

            float length = GetBoundsLength(bounds);

            floorStartWorldLengths[i] = Mathf.Max(0.01f, length);

            fixedBackEdges[i] = GetBackEdge(bounds, direction);
        }
    }

    void SaveOriginalTiling(int index)
    {
        if (floorMaterials[index] == null) return;

        if (floorMaterials[index].HasProperty("_BaseMap"))
        {
            originalTiling[index] = floorMaterials[index].GetTextureScale("_BaseMap");
            originalOffset[index] = floorMaterials[index].GetTextureOffset("_BaseMap");
        }
        else if (floorMaterials[index].HasProperty("_MainTex"))
        {
            originalTiling[index] = floorMaterials[index].GetTextureScale("_MainTex");
            originalOffset[index] = floorMaterials[index].GetTextureOffset("_MainTex");
        }
        else
        {
            originalTiling[index] = Vector2.one;
            originalOffset[index] = Vector2.zero;
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

    void UpdateFloors(float stretchAmount)
    {
        if (stretchDisabled) return;

        Vector3 direction = GetForwardDirection();

        for (int i = 0; i < floorsToStretch.Length; i++)
        {
            if (floorsToStretch[i] == null) continue;

            Renderer rend = floorsToStretch[i].GetComponent<Renderer>();
            if (rend == null) continue;

            float originalLength = Mathf.Max(0.01f, floorStartWorldLengths[i]);
            float newLength = originalLength + stretchAmount;
            float scaleMultiplier = newLength / originalLength;

            Vector3 newScale = floorStartScales[i];

            switch (floorScaleAxis)
            {
                case LocalScaleAxis.X:
                    newScale.x = floorStartScales[i].x * scaleMultiplier;
                    break;

                case LocalScaleAxis.Y:
                    newScale.y = floorStartScales[i].y * scaleMultiplier;
                    break;

                case LocalScaleAxis.Z:
                    newScale.z = floorStartScales[i].z * scaleMultiplier;
                    break;
            }

            floorsToStretch[i].localScale = newScale;

            Bounds newBounds = rend.bounds;
            Vector3 currentBackEdge = GetBackEdge(newBounds, direction);

            Vector3 correction = fixedBackEdges[i] - currentBackEdge;

            floorsToStretch[i].position += correction;

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
        if (floorMaterials == null) return;
        if (index < 0 || index >= floorMaterials.Length) return;
        if (floorMaterials[index] == null) return;

        float repeatAmount = currentWorldLength / textureRepeatEveryWorldUnit;

        Vector2 tiling = Vector2.one;
        Vector2 offset = Vector2.zero;

        if (textureTilingAxis == TextureAxis.X)
        {
            // Tiling invertido
            tiling.x = -repeatAmount;
            tiling.y = 1f;

            // Compensación visual
            offset.x = repeatAmount;
        }
        else
        {
            tiling.x = 1f;
            tiling.y = -repeatAmount;

            offset.y = repeatAmount;
        }

        if (floorMaterials[index].HasProperty("_BaseMap"))
        {
            floorMaterials[index].SetTextureScale("_BaseMap", tiling);
            floorMaterials[index].SetTextureOffset("_BaseMap", offset);
        }
        else if (floorMaterials[index].HasProperty("_MainTex"))
        {
            floorMaterials[index].SetTextureScale("_MainTex", tiling);
            floorMaterials[index].SetTextureOffset("_MainTex", offset);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (stretchDisabled) return;
        if (!other.CompareTag("Player")) return;

        isActive = true;

        if (showDebugLogs)
            Debug.Log("Player tocó WallStretch. La pared empieza a seguirlo.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (stretchDisabled) return;
        if (!other.CompareTag("Player")) return;

        isActive = true;
    }
}