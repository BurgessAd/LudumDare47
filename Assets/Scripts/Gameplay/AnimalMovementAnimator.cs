using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalMovementAnimator : MonoBehaviour
{

    [Header("Animation Params")]
    [SerializeField]
    private AnimationCurve m_HopAnimationCurve;
    [SerializeField]
    private AnimationCurve m_TiltAnimationCurve;
    [SerializeField]
    float m_TotalAnimationTime;
    [SerializeField]
    float m_TiltSizeMultiplier = 1.0f;
    [SerializeField]
    float m_HopHeightMultiplier = 1.0f;
    [SerializeField]
    float m_HorizontalMovementMultiplier = 1.0f;
    [SerializeField]
    float m_WindupTime = 1.0f;
    [SerializeField]
    float m_Phase = 1.0f;
    [Header("Object references")]
    [SerializeField]
    private Transform m_tBodyTransform;
    [SerializeField]
    private Transform m_tParentObjectTransform;

    private float m_fCurrentWindup;
    private float m_CurrentAnimationTime;
    private Vector3 m_vInitialPosition;
    private bool m_bCanHop;

    void Start()
    {
        m_CurrentAnimationTime += Random.Range(0.0f, m_TotalAnimationTime);
        m_vInitialPosition = m_tBodyTransform.localPosition;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // function animates cow hopping constantly when it's moving somewhere.
    void Update()
    {
        m_CurrentAnimationTime = (m_CurrentAnimationTime + Time.deltaTime) % m_TotalAnimationTime;

        float time = m_CurrentAnimationTime / m_TotalAnimationTime;
        float hopHeight = m_HopAnimationCurve.Evaluate((time + m_Phase) % 1);
        float tiltSize = m_TiltAnimationCurve.Evaluate(time);
        float speed = GetComponent<Rigidbody>().velocity.magnitude;
        float multiplier;
        if (m_bCanHop) 
        {
            multiplier = -1;
        }
        else 
        {
            multiplier = Mathf.Sign(speed - 0.4f);
        }
        m_fCurrentWindup = Mathf.Clamp(m_fCurrentWindup + multiplier * Time.deltaTime, 0.0f, m_WindupTime);
        float bounceMult = m_fCurrentWindup / m_WindupTime;
        m_tBodyTransform.localRotation = Quaternion.Euler(0, 180, bounceMult * m_TiltSizeMultiplier * tiltSize);
        m_tBodyTransform.localPosition = m_vInitialPosition + new Vector3(bounceMult * m_HorizontalMovementMultiplier * tiltSize, bounceMult * m_HopHeightMultiplier * hopHeight, 0);
    }
    public class AnimalStaggeredAnimationState : IState
    {
        private float tiltSizeMultiplier;
        private float hopHeightMultiplier;
        private float horizontalMovementMultiplier;
        private float walkWindupTime;
        private float walkPhase;
        private Transform bodyTransform;

        private float currentWindup;
        private float currentAnimation;
        private Vector3 initialPosition;
        private bool canHop;
    }

    public class AnimalWalkingAnimationState : IState 
    {

    }



    public void SetCanHop() 
    {
        m_bCanHop = true;
    }

    public void SetCantHop() 
    {
        m_bCanHop = false;
    }
}
