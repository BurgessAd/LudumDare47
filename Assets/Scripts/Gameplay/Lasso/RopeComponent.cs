using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeComponent : MonoBehaviour
{

    [Range(0.0f, 2.0f)] [SerializeField] private float m_fGravityMultiplier;
    [SerializeField] private float m_fLength;
    [SerializeField] private uint m_ropeIterations;
    [SerializeField] private float m_RopeRadius;
    [SerializeField] private float m_fDistBetweenSegments;

    [SerializeField] private GameObject m_RopeElementPrefab;
    [SerializeField] private Transform m_RopeTransform;
    [SerializeField] private LineRenderer m_RopeLineRenderer;

    private readonly List<RopeSegmentComponent> m_RopeSegments = new List<RopeSegmentComponent>();
    private float m_fDistBetweenFirstSegments;
    private float m_fDistBetweenLastSegments;

    private void CreateRopeSegment() 
    {
        RopeSegmentComponent ropeObject = Instantiate(m_RopeElementPrefab, m_RopeTransform, true).GetComponent<RopeSegmentComponent>();

		// when we create a rope segment, add it to where our last segment is
        if (m_RopeSegments.Count >= 1) 
        {
            ropeObject.GetComponent<RopeSegmentComponent>().SetNewPosition(m_RopeSegments[m_RopeSegments.Count - 1].GetCurrentPosition);
        }

		m_RopeSegments.Add(ropeObject);
	}

    private void RemoveRopeSegment() 
    {
		Destroy(m_RopeSegments[m_RopeSegments.Count - 1]);

		m_RopeSegments.RemoveAt(m_RopeSegments.Count-1);	
    }

	private void OnValidate()
	{
        m_RopeLineRenderer.startWidth = m_RopeRadius * 2;
        m_RopeLineRenderer.endWidth = m_RopeRadius * 2;
        GenerateRopeSegments();
	}

	private void GenerateRopeSegments() 
    {
		// add 1 at the end for end node - consider distBetweenSegments > length
		// or when length < 2 * distBetweenSegments
		// essentially, we need to include the end segment
        int numSegments = (int)Mathf.Ceil(m_fLength / m_fDistBetweenSegments) + 1;

        // fill in rope segments if we don't have enough
        for (int i = m_RopeSegments.Count; i < numSegments; i++) 
        {
            CreateRopeSegment();
        }
        // remove rope segments if we have too many
        for (int i = m_RopeSegments.Count - 1; i >= numSegments; i--) 
        {
            RemoveRopeSegment();
        }

        m_fDistBetweenFirstSegments = m_fLength % m_fDistBetweenSegments;
    }

	//   void FinishRelaxation() 
	//   {
	//	RopeSegmentComponent startSegment = m_RopeSegments[0];
	//	RopeSegmentComponent endSegment = m_RopeSegments[m_RopeSegments.Count - 1];

	//	if (startSegment.IsEndPoint && endSegment.IsEndPoint)
	//	{

	//	}

	//	float totalLength = 0.0f;

	//	for (int i = 0; i < m_RopeSegments.Count - 1; i++)
	//	{
	//		totalLength += (m_RopeSegments[i].GetCurrentPosition() - m_RopeSegments[i + 1].GetCurrentPosition()).magnitude;
	//	}

	//	float offsetLength = totalLength - m_fLength;

	//	if (offsetLength > 0)
	//	{

	//		//float startSegmentOffsetMult = startSegment.GetMass() / (startSegment.GetMass() + endSegment.GetMass());
	//		//float endSegmentOffsetMult = 1 - startSegmentOffsetMult;

	//		Vector3 startSegmentOffset = m_RopeSegments[1].GetCurrentPosition() - startSegment.GetCurrentPosition();
	//		Vector3 endSegmentOffset = m_RopeSegments[m_RopeSegments.Count - 2].GetCurrentPosition() - endSegment.GetCurrentPosition();

	//		// set positions and change velocity of start/end component to account for 
	//	}
	//}


	void ApplyVerletIntegration()
	{
		Vector3 gravityDisplacement = Physics.gravity * m_fGravityMultiplier;
		for (int i = 0; i < m_RopeSegments.Count; i++)
		{
			m_RopeSegments[i].UpdateVerlet(gravityDisplacement);
		}
	}

	void Jakobsen()
	{
		if (m_RopeSegments.Count > 1)
		{
			for (uint i = 0; i < m_ropeIterations; i++)
			{
				RelaxConstraint(m_RopeSegments[0], m_RopeSegments[1], m_fDistBetweenFirstSegments);
				// since we're using j+1, only go up to m_RopeSegments.Count - 1
				for (int j = 1; j < m_RopeSegments.Count - 1; j++)
				{
					RelaxConstraint(m_RopeSegments[j], m_RopeSegments[j + 1], m_fDistBetweenSegments);
				}
			}
		}
	}

	void RelaxConstraint(in RopeSegmentComponent segmentA, in RopeSegmentComponent segmentB, float desiredDistance)
	{
		//offset is from B to A: so apply this positively to B, negatively to A
		Vector3 offset = segmentA.GetCurrentPosition - segmentB.GetCurrentPosition;
		float distance = offset.magnitude;
		Vector3 offsetToAdd = (offset / distance) * ((distance - desiredDistance));
		segmentA.AddToPosition(-offsetToAdd);
		segmentA.AddToPosition(offsetToAdd);

		// if I want to not be touching any object, apply that constraint here, too
	}

	void RenderRope() 
    {
        m_RopeLineRenderer.positionCount = m_RopeSegments.Count;
        for (int i = 0; i < m_RopeSegments.Count; i++) 
        {
            m_RopeLineRenderer.SetPosition(i, m_RopeSegments[i].GetCurrentPosition);
        }
    }

    void FixedUpdate() 
    {
        ApplyVerletIntegration();
        Jakobsen();
        RenderRope();
    }
}
