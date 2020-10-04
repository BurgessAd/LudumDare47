using System.Security.Cryptography;
using UnityEngine;

public class ProjectileRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public int maxIterations = 10000;
    public int maxSegmentCount = 1000;
    public float segmentStepModulo = 10f;
    private float killFloor = - 20f;

    private Vector3[] segments;
    private int numSegments = 0;

    public void Start()
    {
    }

    public void SimulatePath(GameObject firePoint, float force, float mass, float drag, LineRenderer lineRenderer)
    {
        this.lineRenderer = lineRenderer;
        Vector3 forceDirection = Camera.main.transform.forward;
        forceDirection *= force;

       // Rigidbody rigidbody;

        float timestep = Time.fixedDeltaTime;

        float stepDrag = 1 - drag * timestep;
        Vector3 velocity = forceDirection / mass * timestep;
        Vector3 gravity = new Vector3(0f,-9,0f) * timestep * timestep;
        Vector3 startPosition = firePoint.transform.position;

        if (segments == null || segments.Length != maxSegmentCount)
        {
            segments = new Vector3[maxSegmentCount];
        }

        segments[0] = startPosition;
        numSegments = 1;

        for (int i = 0; i < maxIterations && numSegments < maxSegmentCount && startPosition.y > killFloor; i++)
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
        lineRenderer.transform.position = segments[0];
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
