using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeComponent : MonoBehaviour
{
    [SerializeField]
    private ComputeShader m_RopeComputeShader;


    [SerializeField]
    // sets the spring constant of each rope segment
    private float m_fSpringConstant;

    [SerializeField]
    // sets the total length of the rope
    private float m_fLength;

    [SerializeField]
    // sets the amount of rigidbodies we use to similate along the length
    private float m_fSimulationDensity;

    [SerializeField]
    // sets the number of points we have until the force from one point decays entirely
    private uint m_ForceDecayLength;

    [SerializeField]
    // sets the max length, at which the rope is no longer increased in length
    private float m_fMaxLength;

    [SerializeField]
    // sets the min length, at which the rope is no longer decreased in length
    private float m_fMinLength;

    private ComputeBuffer m_RopeSegment;

    public void ApplyForceAtStart(in float Force) 
    {

    }

    public void ApplyForceAtEnd(in float Force) 
    {

    }

    void SetNewRopeLength() 
    {

    }

    private void OnValidate()
    {
        SetNewRopeLength();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
