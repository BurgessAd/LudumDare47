using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioManager))]
public class CounterBase : MonoBehaviour
{
    [SerializeField] private Image m_CounterFirstImage;
    [SerializeField] private Image m_CounterSecondImage;
    [SerializeField] private Text m_CounterText = null;
    [SerializeField] protected CowGameManager m_GameManager;
    [SerializeField] private uint m_CounterMaxVal = 0u;
    [SerializeField] private CounterType m_CounterType;
    [SerializeField] private EntityInformation m_EntityCounterType;
    [SerializeField] private CounterAssociationParams m_CounterAssociationParams;

    private uint m_CounterVal = 0u;
    private Animator m_Animator;
    private AudioManager m_AudioManager;

    public event Action OnCounterCapped;
    public event Action OnCounterUncapped;

    protected virtual void Start()
    {
        m_CounterAssociationParams.GetTextureFromAnimalType(m_EntityCounterType, out Sprite firstTex);
        m_CounterAssociationParams.GetTextureFromGoalType(m_CounterType, out Sprite secondTex);
        m_CounterFirstImage.sprite = firstTex;
        m_CounterFirstImage.preserveAspect = true;
        m_CounterSecondImage.sprite = secondTex;
        m_CounterSecondImage.preserveAspect = true;
        m_Animator = GetComponent<Animator>();
        m_AudioManager = GetComponent<AudioManager>();
        m_GameManager.RegisterCounter(this, m_CounterType, m_EntityCounterType);
        SetText();
    }

    public void IncrementCounter() 
    {
        m_Animator.Play("Base Layer.DefeatGoalAnimation");
        m_AudioManager.Play("IncrementSound");
        m_CounterVal++;
        if (m_CounterVal == m_CounterMaxVal) 
        {
            OnCounterCapped();
        }
        SetText();
    }

    public void DecrementCounter() 
    {
        m_Animator.Play("Base Layer.RemoveGoalAnimation");
        m_AudioManager.Play("DecrementSound");
        if (m_CounterVal == m_CounterMaxVal) 
        {
            OnCounterUncapped();
        }
        m_CounterVal--;
        SetText();
    }

    private void SetText() 
    {
        m_CounterText.text = m_CounterVal.ToString() + " / " + m_CounterMaxVal.ToString();
    }
}
public enum CounterType
{
    Goal,
    Destroyed
}