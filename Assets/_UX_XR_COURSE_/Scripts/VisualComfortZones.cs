
using UnityEngine;

/// <summary>
/// Unity 6.2+
/// Visual Comfort Zones (avstånd + yaw/pitch) Visualiserad med hjälp av Gizmos.
/// Defaults: 
/// - Avstånd: Exclusion <1.0m, Interactive 1.0–1.25m, Optimal 1.25–5.0m (max 5.0m).
/// - Yaw (horizontal): Optimal +/-15 grader, Comfort +/-60 grader.
/// - Pitch (vertical): Optimal -10 grader..+20 grader, Comfort -40 grader..+60 grader
/// Koppla till Offset för XR Rigg.
/// Används för att veta vart man ska placera rena läsbara UI med text eller objekt allmänt för bästa comfort
/// </summary>
[ExecuteAlways]
public class VisualComfortZones : MonoBehaviour
{
    [Header("General")]
    public bool drawInPlayMode = true;
    [Range(8, 256)] public int segments = 128;
    [Range(0.05f, 1f)] public float lineAlpha = 0.9f;

    [Header("Reference Axes (head-local)")]
    public Transform reference;                 // om null, använd denna transform
    public Vector3 upAxis = Vector3.up;
    public Vector3 forwardAxis = Vector3.forward;

    [Header("Distance Bands (meters)")]
    [Min(0.01f)] public float exclusionRadius = 1.0f;       // < 1.0 m (undvik mindre än)
    [Min(0.01f)] public float interactiveInner = 1.0f;      // 1.0 m
    [Min(0.01f)] public float interactiveOuter = 1.25f;     // 1.25 m
    [Min(0.01f)] public float optimalInner = 1.25f;         // 1.25 m
    [Min(0.01f)] public float optimalOuter = 5.0f;          // 5.0 m (soft max)

    [Header("Angular Limits (degrees)")]
    [Range(0, 180)] public float optimalYaw = 15f;           // håll inom +/- 15 grader
    [Range(0, 180)] public float comfortYaw = 60f;           // håll inom +/- 60 grader

    public float optimalPitchDown = -10f;                   // -10 grader
    public float optimalPitchUp = 20f;                      // +20 grader
    public float comfortPitchDown = -40f;                   // -40 grader
    public float comfortPitchUp = 60f;                      // +60 grader

    [Header("What To Draw")]
    public bool drawDistanceBands = true;
    public bool drawYawCones = true;
    public bool drawPitchBands = true;
    public bool drawPeripheralHints = true;                     // visa “avoid” begränsning utanför comfort

    [Header("Colors")]
    public Color exclusionColor = new Color(1f, 0.3f, 0.3f);    // red (för nära)
    public Color interactiveColor = new Color(1f, 0.8f, 0.2f);  // amber (micro/raycast)
    public Color optimalColor = new Color(0.1f, 1f, 0.4f);      // green
    public Color comfortColor = new Color(0f, 0.55f, 1f);       // blue
    public Color peripheralColor = new Color(1f, 1f, 1f);       // white lines för att ge hints

    void OnDrawGizmos()
    {
        if (!drawInPlayMode && Application.isPlaying) return;
        if (segments < 8) segments = 8;

        var t = reference ? reference : transform;
        Vector3 origin = t.position;
        Vector3 up = SafeAxis(t, upAxis);
        Vector3 fwd = SafeAxis(t, forwardAxis);
        Vector3 right = Vector3.Cross(up, fwd).normalized;
        fwd = Vector3.Cross(right, up).normalized;

        if (drawDistanceBands)
        {
            DrawCircle(origin, up, exclusionRadius, WithA(exclusionColor, lineAlpha));
            DrawRingBand(origin, up, interactiveInner, interactiveOuter, WithA(interactiveColor, lineAlpha));
            DrawRingBand(origin, up, optimalInner, optimalOuter, WithA(optimalColor, lineAlpha));

            if (drawPeripheralHints)
                DrawCircle(origin, up, optimalOuter, WithA(peripheralColor, lineAlpha * 0.5f));
        }

        float r = Mathf.Max(optimalOuter, interactiveOuter);

        if (drawYawCones)
        {
            DrawHorizontalCone(origin, fwd, up, comfortYaw, r, WithA(comfortColor, lineAlpha));
            DrawHorizontalCone(origin, fwd, up, optimalYaw, r, WithA(optimalColor, lineAlpha));

            if (drawPeripheralHints && comfortYaw < 180f)
            {
                Gizmos.color = WithA(peripheralColor, lineAlpha * 0.55f);
                float hintSpan = 10f; 
                PolyArc(origin, Yaw(fwd, up, comfortYaw + hintSpan), up, -hintSpan, +hintSpan, r, segments / 3);
                PolyArc(origin, Yaw(fwd, up, -comfortYaw - hintSpan), up, -hintSpan, +hintSpan, r, segments / 3);
            }
        }

        if (drawPitchBands)
        {
            DrawPitchBand(origin, fwd, up, right, comfortPitchDown, comfortPitchUp, r, WithA(comfortColor, lineAlpha));
            DrawPitchBand(origin, fwd, up, right, optimalPitchDown, optimalPitchUp, r, WithA(optimalColor, lineAlpha));

            if (drawPeripheralHints)
            {
                Gizmos.color = WithA(peripheralColor, lineAlpha * 0.55f);
                Vector3 dirLow = Pitch(fwd, right, comfortPitchDown);
                Vector3 dirHigh = Pitch(fwd, right, comfortPitchUp);
                PolyArc(origin, dirLow, up, -12f, +12f, r, segments / 3);
                PolyArc(origin, dirHigh, up, -12f, +12f, r, segments / 3);
            }
        }
    }

    void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color)
    {
        if (radius <= 0f) return;
        Vector3 n = normal.normalized;
        Vector3 basis = Mathf.Abs(Vector3.Dot(n, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up;
        Vector3 u = Vector3.Cross(n, basis).normalized;
        Vector3 v = Vector3.Cross(n, u);
        Gizmos.color = color;

        Vector3 prev = center + (u * radius);
        float step = Mathf.PI * 2f / segments;
        for (int i = 1; i <= segments; i++)
        {
            float a = step * i;
            Vector3 p = center + (u * Mathf.Cos(a) + v * Mathf.Sin(a)) * radius;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }

    void DrawRingBand(Vector3 center, Vector3 normal, float innerR, float outerR, Color color)
    {
        if (outerR <= innerR || outerR <= 0f) return;
        DrawCircle(center, normal, outerR, color);
        DrawCircle(center, normal, innerR, WithA(color, color.a * 0.8f));

        Vector3 n = normal.normalized;
        Vector3 basis = Mathf.Abs(Vector3.Dot(n, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up;
        Vector3 u = Vector3.Cross(n, basis).normalized;
        Vector3 v = Vector3.Cross(n, u);
        Gizmos.color = WithA(color, color.a * 0.5f);
        int stitches = Mathf.Max(8, segments / 4);
        float step = Mathf.PI * 2f / stitches;

        for (int i = 0; i < stitches; i++)
        {
            float a = i * step;
            Vector3 dir = (u * Mathf.Cos(a) + v * Mathf.Sin(a));
            Gizmos.DrawLine(center + dir * innerR, center + dir * outerR);
        }
    }

    void DrawHorizontalCone(Vector3 origin, Vector3 forward, Vector3 up, float halfYawDeg, float radius, Color color)
    {
        if (radius <= 0f || halfYawDeg <= 0f) return;

        Gizmos.color = color;
        PolyArc(origin, forward, up, -halfYawDeg, +halfYawDeg, radius, segments);

        Vector3 leftDir = Yaw(forward, up, -halfYawDeg);
        Vector3 rightDir = Yaw(forward, up, +halfYawDeg);
        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);
    }

    void DrawPitchBand(Vector3 origin, Vector3 forward, Vector3 up, Vector3 right, float pitchDownDeg, float pitchUpDeg, float radius, Color color)
    {
        if (radius <= 0f || pitchUpDeg <= pitchDownDeg) return;

        Gizmos.color = color;
        PolyMeridian(origin, forward, right, pitchDownDeg, pitchUpDeg, radius, segments);

        float yawSkew = 12f;
        Vector3 fLeft = Yaw(forward, up, -yawSkew);
        Vector3 fRight = Yaw(forward, up, +yawSkew);
        Gizmos.color = WithA(color, color.a * 0.8f);
        PolyMeridian(origin, fLeft, right, pitchDownDeg, pitchUpDeg, radius, segments / 2);
        PolyMeridian(origin, fRight, right, pitchDownDeg, pitchUpDeg, radius, segments / 2);

        Gizmos.color = WithA(color, color.a * 0.65f);
        Vector3 lowDir = Pitch(forward, right, pitchDownDeg);
        Vector3 highDir = Pitch(forward, right, pitchUpDeg);
        PolyArc(origin, lowDir, up, -15f, +15f, radius, segments / 3);
        PolyArc(origin, highDir, up, -15f, +15f, radius, segments / 3);
    }

    void PolyArc(Vector3 origin, Vector3 dir, Vector3 n, float a0, float a1, float radius, int segs)
    {
        Vector3 d0 = dir.normalized;
        float start = a0;
        float end = a1;
        float step = (end - start) / Mathf.Max(1, segs);

        Vector3 prev = origin + RotateAroundAxis(d0, n, start) * radius;
        for (int i = 1; i <= segs; i++)
        {
            float a = start + step * i;
            Vector3 p = origin + RotateAroundAxis(d0, n, a) * radius;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }

    void PolyMeridian(Vector3 origin, Vector3 forward, Vector3 right, float minPitchDeg, float maxPitchDeg, float radius, int segs)
    {
        float step = (maxPitchDeg - minPitchDeg) / Mathf.Max(1, segs);
        Vector3 prev = origin + Pitch(forward, right, minPitchDeg) * radius;
        for (int i = 1; i <= segs; i++)
        {
            float a = minPitchDeg + step * i;
            Vector3 p = origin + Pitch(forward, right, a) * radius;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }


    static Vector3 SafeAxis(Transform t, Vector3 localAxis) => (t.rotation * localAxis).normalized;

    static Vector3 RotateAroundAxis(Vector3 v, Vector3 axis, float degrees)
        => Quaternion.AngleAxis(degrees, axis.normalized) * v.normalized;

    static Vector3 Yaw(Vector3 forward, Vector3 up, float degrees)
        => RotateAroundAxis(forward, up, degrees);

    static Vector3 Pitch(Vector3 forward, Vector3 right, float degrees)
        => RotateAroundAxis(forward, right, degrees);

    static Color WithA(Color c, float a) => new Color(c.r, c.g, c.b, Mathf.Clamp01(a));
}
