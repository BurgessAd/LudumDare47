using UnityEngine;
using System;

[RequireComponent(typeof(ObjectPositionAnimator))]
public class CameraStartEndAnimator : MonoBehaviour
{
    private ObjectPositionAnimator m_Animator;
    [SerializeField]
    private Transform m_StartEndTransform;
    [SerializeField]
    private Transform m_DefaultCameraTransform;
    [SerializeField]
    private Rotator m_Rotator;
    private Transform m_CurrentTransform = null;
    [SerializeField]
    private float m_AnimDuration;

    [SerializeField]
    private AnimationCurve m_PositionAnimCurve;

    [SerializeField]
    private AnimationCurve m_RotationAnimCurve;

    // Start is called before the first frame update
    void Awake()
    {
        m_Animator = GetComponent<ObjectPositionAnimator>();
        m_CurrentTransform = GetComponent<Transform>();
    }

    public void AnimateOut() 
    {
        m_Animator.OnAnimComplete = null;
        m_CurrentTransform.SetParent(null);
        m_Animator.SetRotationCurve(m_RotationAnimCurve, m_CurrentTransform.rotation, m_StartEndTransform.rotation);
        m_Animator.SetPositionCurves(m_PositionAnimCurve, m_CurrentTransform.position, m_StartEndTransform.position);
        m_Animator.StartAnimatingPositionAndRotation(m_AnimDuration);
        m_Animator.OnAnimComplete += OnAnimOutFinished;
    }

    private void OnAnimOutFinished() 
    {
        m_Animator.OnAnimComplete -= OnAnimOutFinished;
        m_CurrentTransform.SetParent(m_StartEndTransform);
        m_Rotator.enabled = true;
    }

    public void AddOnCompleteCallback(Action OnCompleteAction) 
    {
        m_Animator.OnAnimComplete += OnCompleteAction;
    }

    public void AnimateIn()
    {
        m_Animator.OnAnimComplete = null;
        m_CurrentTransform.SetParent(null);
        m_Rotator.enabled = false;
        m_Animator.SetRotationCurve(m_RotationAnimCurve, m_CurrentTransform.rotation, m_DefaultCameraTransform.rotation);
        m_Animator.SetPositionCurves(m_PositionAnimCurve, m_CurrentTransform.position, m_DefaultCameraTransform.position);
        m_Animator.StartAnimatingPositionAndRotation(m_AnimDuration);
        m_Animator.OnAnimComplete += OnAnimInFinished;
    }

    private void OnAnimInFinished() 
    {
        m_Animator.OnAnimComplete -= OnAnimInFinished;
        m_CurrentTransform.SetParent(m_DefaultCameraTransform);
    }
}
