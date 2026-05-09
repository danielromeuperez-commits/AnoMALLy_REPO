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

    [Header("Managers")]
    [SerializeField] AnomalyManager anomalyManager;

    [Header("Zona final que se aleja")]
    [SerializeField] Transform corridorEndSection;

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

    [Header("Audio movimiento pasillo")]
    [SerializeField] AudioSource corridorMoveLoopSource;
    [SerializeField] float movementSoundMinDelta = 0.02f;
    [SerializeField] float movementSoundVolume = 0.35f;
    [SerializeField] float soundFadeSpeed = 8f;

    [Tooltip("Tiempo que espera antes de parar el audio cuando deja de detectar movimiento. Evita cortes petados.")]
    [SerializeField] float stopAfterNoMovementTime = 0.15f;

    [Tooltip("Tiempo mínimo que el audio debe sonar antes de poder pararse. Evita Play/Stop demasiado rápidos.")]
    [SerializeField] float minPlayTime = 0.3f;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;

    bool isActive;

    Vector3 triggerStartPosition;
    Vector3 corridorEndStartPosition;

    Vector3[] floorStartScales;
    Vector3[] fixedBackEdges;
    float[] floorStartWorldLengths;
    Material[] floorMaterials;

    float lastStretchAmount;
    float noMovementTimer;
    float playTimer;
    float targetAudioVolume;

    private void Start()
    {
        triggerStartPosition = transform.position;

        if (corridorEndSection != null)
        {
            corridorEndStartPosition = corridorEndSection.position;
        }

        SetupAudio();

        SetupFloors();

        UpdateFloors(0f);
        UpdateEndSection(0f);

        lastStretchAmount = 0f;
    }

    private void Update()
    {
        if (!isActive || player == null)
        {
            HandleCorridorAudio(false);
            return;
        }

        // Si ya están todas las anomalías corregidas,
        // la puerta deja de alejarse y vuelve a su posición normal.
        if (anomalyManager != null && anomalyManager.AllAnomaliesFixed)
        {
            float stretchAmount = 0f;

            HandleCorridorAudio(false);

            MoveTrigger(stretchAmount);
            UpdateFloors(stretchAmount);
            UpdateEndSection(stretchAmount);

            lastStretchAmount = stretchAmount;

            return;
        }

        float currentStretchAmount = GetPlayerDistanceFromTriggerStart();
        currentStretchAmount = Mathf.Clamp(currentStretchAmount, 0f, maxStretchDistance);

        bool corridorIsMoving = Mathf.Abs(currentStretchAmount - lastStretchAmount) > movementSoundMinDelta;

        HandleCorridorAudio(corridorIsMoving);

        MoveTrigger(currentStretchAmount);
        UpdateFloors(currentStretchAmount);
        UpdateEndSection(currentStretchAmount);

        lastStretchAmount = currentStretchAmount;
    }

    void SetupAudio()
    {
        if (corridorMoveLoopSource == null) return;

        corridorMoveLoopSource.loop = true;
        corridorMoveLoopSource.playOnAwake = false;
        corridorMoveLoopSource.volume = 0f;
        corridorMoveLoopSource.Stop();

        targetAudioVolume = 0f;
        noMovementTimer = 0f;
        playTimer = 0f;
    }

    void HandleCorridorAudio(bool corridorIsMoving)
    {
        if (corridorMoveLoopSource == null) return;

        if (corridorIsMoving)
        {
            noMovementTimer = 0f;
            targetAudioVolume = movementSoundVolume;

            if (!corridorMoveLoopSource.isPlaying)
            {
                corridorMoveLoopSource.volume = 0f;
                corridorMoveLoopSource.Play();
                playTimer = 0f;
            }
        }
        else
        {
            noMovementTimer += Time.deltaTime;

            if (noMovementTimer >= stopAfterNoMovementTime && playTimer >= minPlayTime)
            {
                targetAudioVolume = 0f;
            }
        }

        if (corridorMoveLoopSource.isPlaying)
        {
            playTimer += Time.deltaTime;
        }

        corridorMoveLoopSource.volume = Mathf.MoveTowards(
            corridorMoveLoopSource.volume,
            targetAudioVolume,
            soundFadeSpeed * Time.deltaTime
        );

        if (corridorMoveLoopSource.isPlaying &&
            targetAudioVolume <= 0f &&
            corridorMoveLoopSource.volume <= 0.01f)
        {
            corridorMoveLoopSource.Stop();
            corridorMoveLoopSource.volume = 0f;
            playTimer = 0f;
        }
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

    void UpdateEndSection(float stretchAmount)
    {
        if (corridorEndSection == null) return;

        Vector3 direction = GetForwardDirection();

        corridorEndSection.position = corridorEndStartPosition + direction * stretchAmount;
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