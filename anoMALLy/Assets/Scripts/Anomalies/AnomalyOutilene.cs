using UnityEngine;

public class AnomalyOutline : MonoBehaviour
{
    [Header("Outline")]
    [SerializeField] Color outlineColor = new Color(1f, 0.85f, 0.1f, 1f);
    [SerializeField] float lineWidth = 0.025f;
    [SerializeField] Vector3 boundsPadding = new Vector3(0.05f, 0.05f, 0.05f);

    Renderer[] renderers;
    LineRenderer[] lines;

    bool isVisible;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        CreateLines();
        SetVisible(false);
    }

    private void Update()
    {
        if (!isVisible) return;

        UpdateOutlinePositions();
    }

    void CreateLines()
    {
        lines = new LineRenderer[12];

        for (int i = 0; i < lines.Length; i++)
        {
            GameObject lineObj = new GameObject("Outline_Line_" + i);
            lineObj.transform.SetParent(transform);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;

            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = outlineColor;

            line.material = mat;
            line.startColor = outlineColor;
            line.endColor = outlineColor;

            lines[i] = line;
        }
    }

    public void SetVisible(bool value)
    {
        isVisible = value;

        if (lines == null) return;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
                lines[i].enabled = value;
        }

        if (value)
        {
            UpdateOutlinePositions();
        }
    }

    void UpdateOutlinePositions()
    {
        if (renderers == null || renderers.Length == 0) return;
        if (lines == null || lines.Length == 0) return;

        Bounds bounds = GetCombinedBounds();

        bounds.Expand(boundsPadding);

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3 p0 = new Vector3(min.x, min.y, min.z);
        Vector3 p1 = new Vector3(max.x, min.y, min.z);
        Vector3 p2 = new Vector3(max.x, min.y, max.z);
        Vector3 p3 = new Vector3(min.x, min.y, max.z);

        Vector3 p4 = new Vector3(min.x, max.y, min.z);
        Vector3 p5 = new Vector3(max.x, max.y, min.z);
        Vector3 p6 = new Vector3(max.x, max.y, max.z);
        Vector3 p7 = new Vector3(min.x, max.y, max.z);

        SetLine(0, p0, p1);
        SetLine(1, p1, p2);
        SetLine(2, p2, p3);
        SetLine(3, p3, p0);

        SetLine(4, p4, p5);
        SetLine(5, p5, p6);
        SetLine(6, p6, p7);
        SetLine(7, p7, p4);

        SetLine(8, p0, p4);
        SetLine(9, p1, p5);
        SetLine(10, p2, p6);
        SetLine(11, p3, p7);
    }

    Bounds GetCombinedBounds()
    {
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

        bool hasBounds = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];

            if (rend == null) continue;

            if (!hasBounds)
            {
                combinedBounds = rend.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(rend.bounds);
            }
        }

        return combinedBounds;
    }

    void SetLine(int index, Vector3 start, Vector3 end)
    {
        if (index < 0 || index >= lines.Length) return;
        if (lines[index] == null) return;

        lines[index].SetPosition(0, start);
        lines[index].SetPosition(1, end);
    }
}