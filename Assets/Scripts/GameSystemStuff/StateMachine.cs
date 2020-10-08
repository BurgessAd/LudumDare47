using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class StateMachine 
{
    public IState m_CurrentState;
    private List<IState> m_States = new List<IState>();

    private Dictionary<Type, List<StateTransition>> m_StateTransitions = new Dictionary<Type, List<StateTransition>>();

    private List<StateTransition> m_AnyTransitions = new List<StateTransition>();
    private List<StateTransition> m_SpecificTransitions;

    private static List<StateTransition> m_EmptyList = new List<StateTransition>();


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

    public void AddTransition(Type from, Type to, Func<bool> transition) 
    {
        List<StateTransition> transitions;
        if (!m_StateTransitions.TryGetValue(from, out transitions))
        {
            transitions = new List<StateTransition>();
            m_StateTransitions.Add(from, transitions);
        }
        transitions.Add(new StateTransition(to, transition));
    }

    public void AddAnyTransition(Type to, Func<bool> transition) 
    {
        m_AnyTransitions.Add(new StateTransition(to, transition));
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
                if (m_StateTransitions.TryGetValue(newState, out List<StateTransition> newList))
                {
                    m_SpecificTransitions = newList;
                }
                else 
                {
                    m_SpecificTransitions = m_EmptyList;
                }

                m_CurrentState.OnExit();
                m_CurrentState = m_States[i];
                m_CurrentState.OnEnter(data);
                break;
            }
        }
    }

    public void Tick()
    {
        for (int i = 0; i < m_AnyTransitions.Count; i++) 
        {
            if (m_AnyTransitions[i].CanTransition)
            {
                RequestTransition(m_AnyTransitions[i].TypeToTransitionTo);
                m_CurrentState.Tick();
                return;
            }
        }

        for (int i = 0; i < m_SpecificTransitions.Count; i++) 
        {
            if (m_SpecificTransitions[i].CanTransition) 
            {
                RequestTransition(m_SpecificTransitions[i].TypeToTransitionTo);
                m_CurrentState.Tick();
                return;
            }
        }

        m_CurrentState.Tick();
    }
}

public class StateTransition 
{
    private Func<bool> m_StateTransition;
    private Type m_ToState;
    public StateTransition(Type toState, Func<bool> stateTransition) 
    {
        this.m_ToState = toState;
        this.m_StateTransition = stateTransition;
    }
    public bool CanTransition => m_StateTransition();

    public Type TypeToTransitionTo => m_ToState;

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


