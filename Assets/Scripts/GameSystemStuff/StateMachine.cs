using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;


public class StateGroup : IStateGroup
{
    private readonly Action m_OnEnterGroup;
    private readonly Action m_OnExitGroup;
    private readonly Type[] m_StatesInGroup;

    public static StateGroup CreateWithOnEnter(Action OnEnter, params Type[] typesInGroup) { return new StateGroup(OnEnter, null, typesInGroup); }

    public static StateGroup CreateWithOnExit(Action OnExit, params Type[] typesInGroup) { return new StateGroup(null, OnExit, typesInGroup); }

    public static StateGroup CreateWithOnEnterAndExit(Action OnEnter, Action OnExit, params Type[] typesInGroup) { return new StateGroup(OnEnter, OnExit, typesInGroup); }
    
    private StateGroup(Action OnEnter, Action OnExit, Type[] typesInGroup)
    {
        m_StatesInGroup = typesInGroup;
        m_OnEnterGroup = OnEnter;
        m_OnExitGroup = OnExit;
    }

    public bool CheckIfShouldEnterStateGroupForTransition(Type toState) 
    {
        foreach(Type type in m_StatesInGroup) 
        {
            if (toState == type) 
            {
                m_OnEnterGroup?.Invoke();
                return true;
            }
        }
        return false;
    }
    public bool CheckIfShouldExitStateGroupForTransition(Type toState) 
    {
        foreach (Type type in m_StatesInGroup)
        {
            if (toState == type)
            {
                return false;
            }
        }
        m_OnExitGroup?.Invoke();
        return true;
    }    
}

public class BaseStateGroup : IStateGroup
{
	public bool CheckIfShouldEnterStateGroupForTransition(Type toState)
	{
        return false;
	}

	public bool CheckIfShouldExitStateGroupForTransition(Type toState)
	{
        return false;
	}
}

public interface IStateGroup 
{
    bool CheckIfShouldEnterStateGroupForTransition(Type toState);

    bool CheckIfShouldExitStateGroupForTransition(Type toState);
}

public class StateMachine 
{
    private readonly List<AStateBase> m_States = new List<AStateBase>();
    private readonly List<IStateGroup> m_StateGroups = new List<IStateGroup>();
    private readonly Dictionary<Type, List<IStateTransition>> m_StateTransitions = new Dictionary<Type, List<IStateTransition>>();
    private readonly Dictionary<string, object> m_StateMachineParams = new Dictionary<string, object>();
    private readonly Dictionary<string, Action> m_StateMachineCallbacks = new Dictionary<string, Action>();
    private readonly List<IStateTransition> m_AnyTransitions = new List<IStateTransition>();

    private readonly Queue<IStateGroup> m_CurrentStateGroupQueue = new Queue<IStateGroup>(new[] { new BaseStateGroup() });
    private static readonly List<IStateTransition> m_EmptyTransitionsList = new List<IStateTransition>();

    private AStateBase m_CurrentState;
    private List<IStateTransition> m_SpecificTransitions;

    public void SetCallback(string paramIdentifier, Action callback) 
    {
        if (!m_StateMachineCallbacks.ContainsKey(paramIdentifier))
        {
            m_StateMachineCallbacks.Add(paramIdentifier, callback);
        }
        else 
        {
            m_StateMachineCallbacks[paramIdentifier] = callback;
        }
    }

    public void TriggerCallback(string paramIdentifier) 
    {
        m_StateMachineCallbacks[paramIdentifier].Invoke();
    }

    public void SetParam<T>(string paramIdentifier, T val) 
    {
        if (m_StateMachineParams.ContainsKey(paramIdentifier)) 
        {
            m_StateMachineParams[paramIdentifier] = val;
        }
        else 
        {
            m_StateMachineParams.Add(paramIdentifier, val);
        }
    }

    public T GetParam<T>(string paramIdentifier) 
    {
        return (T)m_StateMachineParams[paramIdentifier];
    }

    public void AddState<T>(T newState) where T : AStateBase 
    {
        m_States.Add(newState);
        newState.SetParent(this);
    }

    public Type GetCurrentState() 
    {
        return m_CurrentState.GetType();
    }

    public void AddStateGroup(StateGroup newStateGroup) 
    {
        if (newStateGroup.CheckIfShouldEnterStateGroupForTransition(m_CurrentState.GetType())) 
        {
            m_CurrentStateGroupQueue.Enqueue(newStateGroup);
        }
		else 
        {
            m_StateGroups.Add(newStateGroup);
        }
    }

    public StateMachine(AStateBase initialState) 
    {
        m_SpecificTransitions = m_EmptyTransitionsList;
        m_States.Add(initialState);
        m_CurrentState = initialState;
    }

    public void InitializeStateMachine() 
    {
        m_CurrentState.OnEnter();
    }

    public void AddTransition(Type from, Type to, Func<bool> transition) 
    {
        if (!m_StateTransitions.TryGetValue(from, out List<IStateTransition> transitions))
        {
            transitions = new List<IStateTransition>();
            m_StateTransitions.Add(from, transitions);
            if (m_CurrentState.GetType() == from) 
            {
                m_SpecificTransitions = transitions;
            }
        }
        transitions.Add(new StateTransition(to, transition));

    }

    public void AddTransition(Type from, Type to, Func<bool> transition, Action delegateOnTransition) 
    {
        if (!m_StateTransitions.TryGetValue(from, out List<IStateTransition> transitions))
        {
            transitions = new List<IStateTransition>();
            m_StateTransitions.Add(from, transitions);
        }
        transitions.Add(new StateTransitionWithCallback(to, transition, delegateOnTransition));
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
                if (m_StateTransitions.TryGetValue(newState, out List<IStateTransition> newList))
                {
                    m_SpecificTransitions = newList;
                }
                else
                {
                    m_SpecificTransitions = m_EmptyTransitionsList;
                }
                for(int j = 0; j < m_StateGroups.Count; j++) 
                {
                    IStateGroup stateGroup = m_StateGroups[j];
                    if (stateGroup.CheckIfShouldEnterStateGroupForTransition(newState)) 
                    {
                        m_CurrentStateGroupQueue.Enqueue(stateGroup);
                        m_StateGroups.RemoveAt(j);
                        break;
                    }
                }
                while (true)
                {
                    if (m_CurrentStateGroupQueue.Peek().CheckIfShouldExitStateGroupForTransition(newState))
                    {
                        m_StateGroups.Add(m_CurrentStateGroupQueue.Dequeue());
                        continue;
                    }
                    break;
                }
                m_CurrentState.OnExit();
                m_CurrentState = m_States[i];
                m_CurrentState.OnEnter();
                break;
            }
        }
    }

    public void Tick()
    {
        for (int i = 0; i < m_AnyTransitions.Count; i++) 
        {
            if (m_AnyTransitions[i].TypeToTransitionTo != m_CurrentState.GetType() && m_AnyTransitions[i].AttemptTransition)
            {
                RequestTransition(m_AnyTransitions[i].TypeToTransitionTo);
                m_CurrentState.Tick();
                return;
            }
        }

        for (int i = 0; i < m_SpecificTransitions.Count; i++) 
        {
            if (m_SpecificTransitions[i].AttemptTransition) 
            {
                RequestTransition(m_SpecificTransitions[i].TypeToTransitionTo);
                m_CurrentState.Tick();
                return;
            }
        }

        m_CurrentState.Tick();
    }
}

public abstract class IStateTransition 
{
    protected Func<bool> m_StateTransition;
    protected Type m_ToState;

    public void OverrideFunc(bool val) 
    {
        m_StateTransition = () => val;
    }

    public abstract bool AttemptTransition { get; }

    public  Type TypeToTransitionTo { get => m_ToState; }

}

public class StateTransition : IStateTransition
{
    public StateTransition(Type toState, Func<bool> stateTransition)
    {
        this.m_ToState = toState;
        this.m_StateTransition = stateTransition;
    }
    public override bool AttemptTransition { get => m_StateTransition(); }

    
}

public class StateTransitionWithCallback : IStateTransition
{
    private Action m_TransitionAction;
    public StateTransitionWithCallback(Type toState, Func<bool> stateTransition, Action transitionAction)
    {
        this.m_ToState = toState;
        this.m_StateTransition = stateTransition;
        this.m_TransitionAction = transitionAction;
    }
    public override bool AttemptTransition { get { if (m_StateTransition()) { m_TransitionAction(); return true; } return false; } }

}

public abstract class AStateBase
{
    private StateMachine m_ParentStateMachine;
    public void SetParent(in StateMachine parentStateMachine) 
    {
        m_ParentStateMachine = parentStateMachine;
    }
    protected void RequestTransition<T>()
    {
        m_ParentStateMachine.RequestTransition(typeof(T));
    }

    public void TriggerCallback(string paramIdentifier)
    {
        m_ParentStateMachine.TriggerCallback(paramIdentifier);
    }

    public void SetParam<T>(string paramIdentifier, T val)
    {
        m_ParentStateMachine.SetParam(paramIdentifier, val);
    }

    public T GetParam<T>(string paramIdentifier)
    {
        return m_ParentStateMachine.GetParam<T>(paramIdentifier);
    }

    public virtual void Tick() { }
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
}

