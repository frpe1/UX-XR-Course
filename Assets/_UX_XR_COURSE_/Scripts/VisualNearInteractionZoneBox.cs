// Unity 6.2+
// 3D gizmo grid box: 1.60m (W) x 0.80m (D) x 0.80m (H)
// Pivot/origin = BACK-BOTTOM-CENTER of the box (open face: no grid on pivot side).
// Lines are strictly inside faces; no spill outside the box.

using UnityEngine;

[ExecuteAlways]
public class VisualNearInteractionZoneBox : MonoBehaviour
{
    public enum Anchor
    {
        Center,
        FrontBottomCenter,
        BackBottomCenter // <-- default per request
    }

    [Header("Reference Frame")]
    public Transform reference;
    public Vector3 forwardAxis = Vector3.forward;
    public Vector3 upAxis = Vector3.up;

    [Header("Box Size (meters)")]
    [Min(0.01f)] public float width = 1.60f; // X
    [Min(0.01f)] public float depth = 0.80f; // Z
    [Min(0.01f)] public float height = 0.80f; // Y

    [Header("Grid Spacing (meters)")]
    [Min(0.01f)] public float major = 0.10f;  // e.g. 10 cm
    [Min(0f)] public float minor = 0.02f;  // 0 = off

    [Header("Placement")]
    public Anchor anchor = Anchor.BackBottomCenter;
    public Vector3 localOffset = new Vector3(0f, 0.65f, 0.0f);
    // (up) 0 cm in front of pivot back face, 0.65 cm high based on a standard desktop height (tweak as needed)

    [Header("Draw Options")]
    public bool drawBounds = true;     // outer edges only (all faces)
    public bool drawFrontPlane = true; // diorama window (opposite pivot face)
    public bool drawTopPlane = true;
    public bool drawSidePlanes = true; // left & right
    public bool drawFloorPlane = true;
    public bool drawBackPlane = false; // pivot face kept OPEN (no grid)

    [Header("Style")]
    [Range(0f, 1f)] public float alpha = 0.95f;
    public Color boundsColor = new Color(1f, 1f, 1f, 1f);
    public Color majorColor = new Color(0.25f, 0.75f, 1f, 1f);
    public Color minorColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    void OnDrawGizmos()
    {
        var t = reference ? reference : transform;

        // Basis
        Vector3 up = (t.rotation * upAxis).normalized;
        Vector3 fwd = (t.rotation * forwardAxis).normalized;
        Vector3 right = Vector3.Cross(up, fwd).normalized;
        fwd = Vector3.Cross(right, up).normalized;

        // Pivot/origin in world
        Vector3 pivot = t.TransformPoint(localOffset);

        // Compute center from chosen anchor
        Vector3 center = pivot;
        switch (anchor)
        {
            case Anchor.Center:
                break;
            case Anchor.FrontBottomCenter:
                center = pivot - fwd * (depth * 0.5f) + up * (height * 0.5f);
                break;
            case Anchor.BackBottomCenter:
                center = pivot + fwd * (depth * 0.5f) + up * (height * 0.5f); // <-- back face pivot
                break;
        }

        // Half-extents
        Vector3 hx = right * (width * 0.5f);
        Vector3 hz = fwd * (depth * 0.5f);
        Vector3 hy = up * (height * 0.5f);

        // Corners (±x, ±y, ±z)
        Vector3 c000 = center - hx - hy - hz; // (-x,-y,-z) back-bottom-left
        Vector3 c001 = center - hx - hy + hz; // (-x,-y,+z) front-bottom-left
        Vector3 c010 = center - hx + hy - hz; // (-x,+y,-z) back-top-left
        Vector3 c011 = center - hx + hy + hz; // (-x,+y,+z) front-top-left
        Vector3 c100 = center + hx - hy - hz; // (+x,-y,-z) back-bottom-right
        Vector3 c101 = center + hx - hy + hz; // (+x,-y,+z) front-bottom-right
        Vector3 c110 = center + hx + hy - hz; // (+x,+y,-z) back-top-right
        Vector3 c111 = center + hx + hy + hz; // (+x,+y,+z) front-top-right

        // Bounds (edges only)
        if (drawBounds)
        {
            Gizmos.color = WithA(boundsColor, alpha);
            // bottom
            L(c000, c100); L(c100, c101); L(c101, c001); L(c001, c000);
            // top
            L(c010, c110); L(c110, c111); L(c111, c011); L(c011, c010);
            // verticals
            L(c000, c010); L(c100, c110); L(c101, c111); L(c001, c011);
        }

        // Faces (grid strictly within face rects)

        // FRONT (+Z) plane (opposite the pivot back face) — diorama window
        if (drawFrontPlane) DrawGridPlane(c001, c101, c011, right, up, width, height);

        // BACK (-Z) plane (pivot face) kept OPEN by default
        if (drawBackPlane) DrawGridPlane(c000, c100, c010, right, up, width, height);

        // TOP plane
        if (drawTopPlane) DrawGridPlane(c010, c110, c011, right, fwd, width, depth);

        // FLOOR plane
        if (drawFloorPlane) DrawGridPlane(c000, c100, c001, right, fwd, width, depth);

        // LEFT plane
        if (drawSidePlanes) DrawGridPlane(c000, c001, c010, fwd, up, depth, height);

        // RIGHT plane
        if (drawSidePlanes) DrawGridPlane(c100, c101, c110, fwd, up, depth, height);
    }

    // Draw a grid limited to the rectangle defined by:
    // p00 (lower-left), p10 (lower-right), p01 (upper-left), with u along (p10 - p00) and v along (p01 - p00).
    void DrawGridPlane(Vector3 p00, Vector3 p10, Vector3 p01, Vector3 uDir, Vector3 vDir, float uSize, float vSize)
    {
        Vector3 u = (p10 - p00).normalized; // robust: derive from corners
        Vector3 v = (p01 - p00).normalized;

        // Exact corners
        Vector3 q00 = p00;
        Vector3 q10 = p00 + u * uSize;
        Vector3 q01 = p00 + v * vSize;
        Vector3 q11 = q10 + v * vSize;

        // Outer rectangle (contained)
        Gizmos.color = WithA(majorColor, alpha);
        L(q00, q10); L(q10, q11); L(q11, q01); L(q01, q00);

        // Major lines
        if (major > 0.0001f)
        {
            Gizmos.color = WithA(majorColor, alpha);
            int uCount = Mathf.Max(0, Mathf.FloorToInt(uSize / major) - 1);
            int vCount = Mathf.Max(0, Mathf.FloorToInt(vSize / major) - 1);

            for (int i = 1; i <= uCount; i++)
            {
                float du = i * major;
                Vector3 a = q00 + u * du;
                L(a, a + v * vSize);
            }
            for (int j = 1; j <= vCount; j++)
            {
                float dv = j * major;
                Vector3 a = q00 + v * dv;
                L(a, a + u * uSize);
            }
        }

        // Minor lines (skipping majors)
        if (minor >= 0.01f && minor < major)
        {
            Gizmos.color = WithA(minorColor, alpha * 0.6f);
            int uCount = Mathf.Max(0, Mathf.FloorToInt(uSize / minor) - 1);
            int vCount = Mathf.Max(0, Mathf.FloorToInt(vSize / minor) - 1);

            for (int i = 1; i <= uCount; i++)
            {
                float du = i * minor;
                if (Mathf.Abs(Mathf.Repeat(du, major)) < 1e-4f) continue;
                Vector3 a = q00 + u * du;
                L(a, a + v * vSize);
            }
            for (int j = 1; j <= vCount; j++)
            {
                float dv = j * minor;
                if (Mathf.Abs(Mathf.Repeat(dv, major)) < 1e-4f) continue;
                Vector3 a = q00 + v * dv;
                L(a, a + u * uSize);
            }
        }
    }

    static void L(Vector3 a, Vector3 b) => Gizmos.DrawLine(a, b);
    static Color WithA(Color c, float a) => new Color(c.r, c.g, c.b, Mathf.Clamp01(a));
}


