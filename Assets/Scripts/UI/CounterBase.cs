using UnityEngine;
using UnityEngine.UI;
using System;

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

    public string GetBindingString => m_BindingString;

    public event Action OnCounterCapped;

    public event Action OnCounterUncapped;

    protected virtual void Start()
    {
        m_GameManager.RegisterCounter(this);
        SetText();
    }

    public void IncrementCounter() 
    {
        m_CounterVal++;
        if (m_CounterVal == m_CounterMaxVal) 
        {
            OnCounterCapped();
        }
        SetText();
    }

    public void DecrementCounter() 
    {
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
