using UnityEngine;

public class DoorStretchSimple : MonoBehaviour
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

    [Header("Suelos que se estiran")]
    [SerializeField] Transform[] floorsToStretch;

    [Header("Dirección hacia delante")]
    [SerializeField] WorldAxis moveAxis = WorldAxis.X;
    [SerializeField] bool invertDirection = true;

    [Header("Eje que escala el suelo")]
    [SerializeField] LocalScaleAxis floorScaleAxis = LocalScaleAxis.X;

    [Header("Eje del tiling")]
    [SerializeField] TextureAxis textureTilingAxis = TextureAxis.X;

    [Header("Estirado")]
    [SerializeField] float maxStretchDistance = 100f;
    [SerializeField] float followSpeed = 20f;

    [Header("Tiling")]
    [SerializeField] float textureRepeatEveryWorldUnit = 1f;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;

    bool isActive;

    Vector3 triggerStartPosition;

    Vector3[] floorStartScales;
    Vector3[] fixedBackEdges;
    float[] floorStartWorldLengths;
    Material[] floorMaterials;

    private void Start()
    {
        triggerStartPosition = transform.position;

        SetupFloors();

        UpdateFloors(0f);
    }

    private void Update()
    {
        if (!isActive || player == null) return;

        float stretchAmount = GetPlayerDistanceFromTriggerStart();
        stretchAmount = Mathf.Clamp(stretchAmount, 0f, maxStretchDistance);

        MoveTrigger(stretchAmount);
        UpdateFloors(stretchAmount);
    }

    void SetupFloors()
    {
        floorStartScales = new Vector3[floorsToStretch.Length];
        fixedBackEdges = new Vector3[floorsToStretch.Length];
        floorStartWorldLengths = new float[floorsToStretch.Length];
        floorMaterials = new Material[floorsToStretch.Length];

        Vector3 direction = GetForwardDirection();

        for (int i = 0; i < floorsToStretch.Length; i++)
        {
            if (floorsToStretch[i] == null) continue;

            floorStartScales[i] = floorsToStretch[i].localScale;

            Renderer rend = floorsToStretch[i].GetComponent<Renderer>();

            if (rend == null)
            {
                Debug.LogWarning("El suelo no tiene Renderer: " + floorsToStretch[i].name);
                continue;
            }

            floorMaterials[i] = rend.material;

            Bounds bounds = rend.bounds;

            float length = GetBoundsLength(bounds);

            floorStartWorldLengths[i] = Mathf.Max(0.01f, length);

            // Guardamos el borde trasero real del mesh.
            // Este punto NO se debe mover nunca.
            fixedBackEdges[i] = GetBackEdge(bounds, direction);
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
        Vector3 direction = GetForwardDirection();

        Vector3 targetPosition = triggerStartPosition + direction * stretchAmount;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    void UpdateFloors(float stretchAmount)
    {
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

            // 1. Escalamos.
            floorsToStretch[i].localScale = newScale;

            // 2. Calculamos dónde está ahora el borde trasero.
            Bounds newBounds = rend.bounds;
            Vector3 currentBackEdge = GetBackEdge(newBounds, direction);

            // 3. Movemos SOLO lo necesario para que el borde trasero vuelva a su sitio.
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

        if (floorMaterials[index].HasProperty("_BaseMap"))
        {
            floorMaterials[index].SetTextureScale("_BaseMap", tiling);
        }
        else if (floorMaterials[index].HasProperty("_MainTex"))
        {
            floorMaterials[index].SetTextureScale("_MainTex", tiling);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isActive = true;

        if (showDebugLogs)
            Debug.Log("Player tocó Door_Stretch_Trigger. El trigger empieza a seguirlo.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isActive = true;
    }
}