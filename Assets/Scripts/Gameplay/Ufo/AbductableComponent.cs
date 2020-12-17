using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public abstract class AbductableComponent : MonoBehaviour
{
    [SerializeField]
    private CowGameManager m_Manager;

    [SerializeField]
    private float m_fAbductRotateSpeed;

    private Rigidbody m_Body;

    private Transform m_Transform;

    private IEnumerator m_AbductionRotate;

    Vector3 m_ChosenRotationAxis;

    public Transform GetTransform => m_Transform;
    public Rigidbody GetBody => m_Body;
    public virtual void OnBeginAbducting() 
    {
        StartCoroutine(m_AbductionRotate);
    }

    private IEnumerator TractorBeamRotation() 
    {
        m_ChosenRotationAxis = UnityEngine.Random.onUnitSphere;
        while (true) 
        {
            // accelerate/decellerate to desired rotational axis
            // using angular rotations
            yield return null;
        }
    }

    public virtual void OnEndAbducting() 
    {
        StopCoroutine(m_AbductionRotate);
    }
}
