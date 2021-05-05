using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeComponent : MonoBehaviour
{

    [SerializeField] private float m_GravityStrength;
    [SerializeField] private float m_fLength;
    [SerializeField] private uint m_ropeIterations;
    [SerializeField] private float m_RopeRadius;
    [SerializeField] private float m_fDistBetweenSegments;

    [SerializeField] private GameObject m_RopeElementPrefab;
    [SerializeField] private Transform m_RopeTransform;
    [SerializeField] private LineRenderer m_RopeLineRenderer;

    [SerializeField] private Rigidbody m_StartBody;
    [SerializeField] private Rigidbody m_EndBody;



    private readonly List<RopeSegmentComponent> m_RopeSegments = new List<RopeSegmentComponent>();
    private float m_fDistBetweenFirstSegments;
    private float m_fDistBetweenLastSegments;

    private void CreateRopeSegment() 
    {
        RopeSegmentComponent ropeObject = Instantiate(m_RopeElementPrefab, m_RopeTransform, true).GetComponent<RopeSegmentComponent>();
        ropeObject.SetRopeSize(m_RopeRadius);
        m_RopeSegments.Add(ropeObject);
        if (m_RopeSegments.Count > 0) 
        {
            ropeObject.GetComponent<RopeSegmentComponent>().SetNewPosition(m_RopeSegments[m_RopeSegments.Count - 1].GetCurrentPosition());
        }
    }

    private void RemoveRopeSegment() 
    {
        m_RopeSegments.RemoveAt(0);
    }

	private void OnValidate()
	{
        m_RopeLineRenderer.startWidth = m_RopeRadius * 2;
        m_RopeLineRenderer.endWidth = m_RopeRadius * 2;
        GenerateRopeSegments();
	}

	private void GenerateRopeSegments() 
    {
        int numSegments = (int)Mathf.Ceil(m_fLength / m_fDistBetweenSegments) + 1;

        // make sure that our rope is the right size
        for (int i = 0; i < Mathf.Min(numSegments, m_RopeSegments.Count); i++) 
        {
            m_RopeSegments[i].SetRopeSize(m_RopeRadius);
        }

        // fill in rope segments if we don't have enough
        for (int i = m_RopeSegments.Count; i < numSegments; i++) 
        {
            CreateRopeSegment();
        }
        // remove rope segments if we have too many
        for (int i = numSegments; i < m_RopeSegments.Count; i++) 
        {
            RemoveRopeSegment();
        }

        m_fDistBetweenFirstSegments = m_fLength % m_fDistBetweenSegments;
    }

    void ApplyVerletIntegration() 
    {
        Vector3 gravityDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
        for (int i = 0; i < m_RopeSegments.Count-1; i++) 
        {
            m_RopeSegments[i].UpdateVerlet(gravityDisplacement);
        }
    }

    void RelaxConstraint(in RopeSegmentComponent segmentA, in RopeSegmentComponent segmentB, float desiredDistance) 
    {
        //offset is from B to A: so apply this positively to B, negatively to A
        Vector3 offset = segmentA.GetCurrentPosition() - segmentB.GetCurrentPosition();
        float distance = offset.magnitude;
        Vector3 offsetToAdd = (offset / distance) * ((distance - desiredDistance));

        //float segmentAOffsetMult = segmentA.GetMass() / (segmentA.GetMass() + segmentB.GetMass());
        //segmentA.AddToPosition(-offsetToAdd * segmentAOffsetMult);
        //segmentB.AddToPosition(offsetToAdd * (1 - segmentAOffsetMult));
    }

    void FinishRelaxation() 
    {
        RopeSegmentComponent startSegment = m_RopeSegments[0];
        RopeSegmentComponent endSegment = m_RopeSegments[m_RopeSegments.Count - 1];

        if (startSegment.IsEndPoint && endSegment.IsEndPoint) 
        {

        }

        float totalLength = 0.0f;

        for (int i = 0; i < m_RopeSegments.Count-1; i++) 
        {
            totalLength += (m_RopeSegments[i].GetCurrentPosition() - m_RopeSegments[i + 1].GetCurrentPosition()).magnitude;
        }

        float offsetLength = totalLength - m_fLength;

        if (offsetLength > 0) 
        {

            //float startSegmentOffsetMult = startSegment.GetMass() / (startSegment.GetMass() + endSegment.GetMass());
            //float endSegmentOffsetMult = 1 - startSegmentOffsetMult;

            Vector3 startSegmentOffset = m_RopeSegments[1].GetCurrentPosition() - startSegment.GetCurrentPosition();
            Vector3 endSegmentOffset = m_RopeSegments[m_RopeSegments.Count - 2].GetCurrentPosition() - endSegment.GetCurrentPosition();

            // set positions and change velocity of start/end component to account for 
        }
    }

    void Jakobsen() 
    {
        for (uint i = 0; i < m_ropeIterations; i++) 
        {
            for (int j = 1; j < m_RopeSegments.Count - 1; j++)
            {
                RelaxConstraint(m_RopeSegments[j], m_RopeSegments[j + 1], m_fDistBetweenSegments);
            }
        }
    }

    void RenderRope() 
    {
        m_RopeLineRenderer.positionCount = m_RopeSegments.Count;
        for (int i = 0; i < m_RopeSegments.Count; i++) 
        {
            m_RopeLineRenderer.SetPosition(i, m_RopeSegments[i].GetCurrentPosition());
        }
    }

    void FixedUpdate() 
    {
        ApplyVerletIntegration();
        Jakobsen();
        RenderRope();
    }
}
