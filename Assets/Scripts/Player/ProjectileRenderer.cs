using System.Security.Cryptography;
using UnityEngine;

public class ProjectileRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public int maxIterations = 10000;
    public int maxSegmentCount = 1000;
    public float segmentStepModulo = 10f;

    private Vector3[] segments;
    private int numSegments = 0;

    public void Start()
    {
    }

    public void SimulatePath(GameObject gameObject, Vector3 forceDirection, float mass, float drag, LineRenderer lineRenderer)
    {
        this.lineRenderer = lineRenderer;
        forceDirection *= 10;

       // Rigidbody rigidbody;

        float timestep = Time.fixedDeltaTime;

        float stepDrag = 1 - drag * timestep;
        Vector3 velocity = forceDirection / mass * timestep;
        Vector3 gravity = Physics.gravity * timestep * timestep;
        Vector3 startPosition = gameObject.transform.Find("Main Camera").Find("FirePoint").position;

        if (segments == null || segments.Length != maxSegmentCount)
        {
            segments = new Vector3[maxSegmentCount];
        }

        segments[0] = startPosition;
        numSegments = 1;

        for (int i = 0; i < maxIterations && numSegments < maxSegmentCount; i++)
        {
            velocity += gravity;
            velocity *= stepDrag;

            startPosition += velocity;

            if (i % segmentStepModulo == 0)
            {
                segments[numSegments] = startPosition;
                numSegments++;
            }
        }

        Draw();
    }

    private void Draw()
    {
/*        Color startColor = Color.cyan;
        Color endColor = Color.cyan;
        startColor.a = 0f;
        endColor.a = 0f;*/

        lineRenderer.transform.position = segments[0];

/*        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;*/

        lineRenderer.positionCount = numSegments;
        for (int i = 0; i < numSegments; i++)
        {
            lineRenderer.SetPosition(i, segments[i]);
        }
    }

    public void clear()
    {
        lineRenderer.positionCount = 0;
    }
}
