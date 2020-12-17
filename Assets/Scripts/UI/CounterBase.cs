using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioManager))]
public class CounterBase : MonoBehaviour
{
    [SerializeField]
    private string m_BindingString = "";
    [SerializeField]
    private Text m_CounterText = null;
    [SerializeField]
    protected CowGameManager m_GameManager;

    private uint m_CounterVal = 0u;
    [SerializeField]
    private uint m_CounterMaxVal = 0u;

    private Animator m_Animator;
    private AudioManager m_AudioManager;

    public string GetBindingString => m_BindingString;

    public event Action OnCounterCapped;

    public event Action OnCounterUncapped;

    protected virtual void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_AudioManager = GetComponent<AudioManager>();
        m_GameManager.RegisterCounter(this);
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
