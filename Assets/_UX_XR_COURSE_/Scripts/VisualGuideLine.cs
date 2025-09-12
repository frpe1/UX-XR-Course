using UnityEngine;

public class VisualGuideLine : MonoBehaviour
{
    [SerializeField]
    private bool isVisible = true;

    [SerializeField]
    private Color lineColor;

    private void OnDrawGizmos()
    {
        ShowHelpLines();
    }

    private void OnDrawGizmosSelected()
    {
        ShowHelpLines();
    }

    private void ShowHelpLines()
    {
        if (isVisible)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5);
        }
    }
}
