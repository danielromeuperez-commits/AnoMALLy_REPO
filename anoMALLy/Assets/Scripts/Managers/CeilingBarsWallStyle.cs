using UnityEngine;

public class CeilingBarsWallStyle : MonoBehaviour
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

    [Header("Barras del techo")]
    [SerializeField] Transform[] barsToStretch;

    [Header("Dirección hacia delante")]
    [SerializeField] WorldAxis moveAxis = WorldAxis.X;
    [SerializeField] bool invertDirection = true;

    [Header("Lado físico")]
    [Tooltip("ON = la barra se estira hacia delante. OFF = se estira hacia el lado contrario.")]
    [SerializeField] bool stretchTowardsForward = true;

    [Header("Eje que escala la barra")]
    [SerializeField] LocalScaleAxis barScaleAxis = LocalScaleAxis.X;

    [Header("Eje del tiling")]
    [SerializeField] TextureAxis textureTilingAxis = TextureAxis.X;

    [Header("Estirado")]
    [SerializeField] float maxStretchDistance = 100f;

    [Header("Tiling")]
    [SerializeField] float textureRepeatEveryWorldUnit = 1f;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;

    bool isActive;

    Vector3 triggerStartPosition;

    Vector3[] barStartScales;
    Vector3[] fixedEdges;
    float[] barStartWorldLengths;
    Material[] barMaterials;

    private void Start()
    {
        triggerStartPosition = transform.position;

        if (anomalyManager == null)
        {
            anomalyManager = FindAnyObjectByType<AnomalyManager>();
        }

        SetupBars();

        UpdateBars(0f);
    }

    private void Update()
    {
        if (!isActive || player == null) return;

        if (anomalyManager != null && anomalyManager.AllAnomaliesFixed)
        {
            UpdateBars(0f);
            return;
        }

        float currentStretchAmount = GetPlayerDistanceFromTriggerStart();
        currentStretchAmount = Mathf.Clamp(currentStretchAmount, 0f, maxStretchDistance);

        UpdateBars(currentStretchAmount);
    }

    void SetupBars()
    {
        barStartScales = new Vector3[barsToStretch.Length];
        fixedEdges = new Vector3[barsToStretch.Length];
        barStartWorldLengths = new float[barsToStretch.Length];
        barMaterials = new Material[barsToStretch.Length];

        Vector3 direction = GetForwardDirection();

        for (int i = 0; i < barsToStretch.Length; i++)
        {
            if (barsToStretch[i] == null) continue;

            barStartScales[i] = barsToStretch[i].localScale;

            Renderer rend = barsToStretch[i].GetComponent<Renderer>();

            if (rend == null)
            {
                Debug.LogWarning("La barra no tiene Renderer: " + barsToStretch[i].name);
                continue;
            }

            barMaterials[i] = rend.material;

            Bounds bounds = rend.bounds;

            float length = GetBoundsLength(bounds);

            barStartWorldLengths[i] = Mathf.Max(0.01f, length);

            fixedEdges[i] = GetFixedEdge(bounds, direction);
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

    void UpdateBars(float stretchAmount)
    {
        Vector3 direction = GetForwardDirection();

        for (int i = 0; i < barsToStretch.Length; i++)
        {
            if (barsToStretch[i] == null) continue;

            Renderer rend = barsToStretch[i].GetComponent<Renderer>();
            if (rend == null) continue;

            float originalLength = Mathf.Max(0.01f, barStartWorldLengths[i]);
            float newLength = originalLength + stretchAmount;
            float scaleMultiplier = newLength / originalLength;

            Vector3 newScale = barStartScales[i];

            switch (barScaleAxis)
            {
                case LocalScaleAxis.X:
                    newScale.x = barStartScales[i].x * scaleMultiplier;
                    break;

                case LocalScaleAxis.Y:
                    newScale.y = barStartScales[i].y * scaleMultiplier;
                    break;

                case LocalScaleAxis.Z:
                    newScale.z = barStartScales[i].z * scaleMultiplier;
                    break;
            }

            barsToStretch[i].localScale = newScale;

            Bounds newBounds = rend.bounds;
            Vector3 currentFixedEdge = GetFixedEdge(newBounds, direction);

            Vector3 correction = fixedEdges[i] - currentFixedEdge;

            barsToStretch[i].position += correction;

            UpdateTiling(i, newLength);
        }
    }

    float GetBoundsLength(Bounds bounds)
    {
        if (moveAxis == WorldAxis.X)
            return bounds.size.x;

        return bounds.size.z;
    }

    Vector3 GetFixedEdge(Bounds bounds, Vector3 direction)
    {
        float halfLength;

        if (moveAxis == WorldAxis.X)
            halfLength = bounds.extents.x;
        else
            halfLength = bounds.extents.z;

        if (stretchTowardsForward)
        {
            // Se queda fijo el lado de atrás.
            // El otro lado crece hacia delante.
            return bounds.center - direction * halfLength;
        }
        else
        {
            // Se queda fijo el lado de delante.
            // El otro lado crece hacia atrás.
            return bounds.center + direction * halfLength;
        }
    }

    void UpdateTiling(int index, float currentWorldLength)
    {
        if (barMaterials == null) return;
        if (index < 0 || index >= barMaterials.Length) return;
        if (barMaterials[index] == null) return;

        float repeatAmount = currentWorldLength / textureRepeatEveryWorldUnit;

        Vector2 tiling = Vector2.one;

        if (textureTilingAxis == TextureAxis.X)
        {
            tiling.x = repeatAmount;
            tiling.y = 1f;
        }
        else
        {
            tiling.x = 1f;
            tiling.y = repeatAmount;
        }

        if (barMaterials[index].HasProperty("_BaseMap"))
        {
            barMaterials[index].SetTextureScale("_BaseMap", tiling);
        }
        else if (barMaterials[index].HasProperty("_MainTex"))
        {
            barMaterials[index].SetTextureScale("_MainTex", tiling);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isActive = true;

        if (showDebugLogs)
            Debug.Log("Player tocó CeilingBarsWallStyle. Las barras del techo empiezan a estirarse.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isActive = true;
    }
}