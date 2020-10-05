using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class StateMachine 
{
    public IState m_CurrentState;
    private List<IState> m_States = new List<IState>();
    public StateMachine(IState InitializedState)
    {
        AddState(InitializedState);
        m_CurrentState = InitializedState;
    }


    public void AddState<T>(T newState) where T : IState 
    {
        m_States.Add(newState);
        newState.StateTransitionRequest += RequestTransition;
    }

    public void RequestTransition(Type newState)
    {
        for (int i = 0; i < m_States.Count; i++)
        {
            if (m_States[i].GetType() == newState && m_CurrentState.GetType() != newState)
            {
                m_CurrentState.OnExit();
                m_CurrentState = m_States[i];
                m_CurrentState.OnEnter();
                break;
            }
        }
    }

    public void RequestTransition(Type newState, object data)
    {
        for (int i = 0; i < m_States.Count; i++)
        {
            if (m_States[i].GetType() == newState && m_CurrentState.GetType() != newState)
            {
                m_CurrentState.OnExit();
                m_CurrentState = m_States[i];
                m_CurrentState.OnEnter(data);
                break;
            }
        }
    }

    public void Tick()
    {
        m_CurrentState.Tick();
    }
}

public abstract class IState
{
    public event Action<Type> StateTransitionRequest;

    protected void RequestTransition<T>()
    {
        StateTransitionRequest(typeof(T));
    }
    public virtual void Tick() { }
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnEnter(object data) { }

}


