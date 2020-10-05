using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UfoStateManager : MonoBehaviour
{

    // setting up and adding states to state machine
    public StateMachine m_StateMachine;
    public UfoMain ufoMain;
    public CowGameManager gameManager;
    public void Start()
    {
        ufoMain = gameObject.AddComponent(typeof(UfoMain)) as UfoMain;

        m_StateMachine = new StateMachine(new UfoIdle(this));
        m_StateMachine.AddState(new UfoSearch(this));
        m_StateMachine.AddState(new UfoSwooping(this));
        m_StateMachine.AddState(new UfoAbduct(this));
        m_StateMachine.AddState(new UfoReturnSweep(this));
        m_StateMachine.AddState(new UfoStaggered(this));
        m_StateMachine.AddState(new UfoDeath(this));

        ufoMain.setStateManager(this);


    }


    // state machine update
    public void Update()
    {
       m_StateMachine.Tick();
    }

    public class UfoIdle : IState
    {
        private UfoStateManager stateManager;

        public UfoIdle(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void OnEnter()
        {
            Debug.Log("Idle");
        }

        public override void Tick()
        {
           //  RequestTransition<UfoSearch>();
        }

    }
    
    public class UfoSearch : IState
    {
        private UfoStateManager stateManager;

        public UfoSearch(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void OnEnter()
        {
            Debug.Log("Search");
        }

        public override void Tick()
        {

            if (stateManager.gameManager.cows.Count > 0) { 
                GameObject cow = stateManager.gameManager.cows[0];
                stateManager.ufoMain.setTarget(cow);
                RequestTransition<UfoSwooping>();
            }
        }

    }
  
    public class UfoSwooping : IState
    {
        private UfoStateManager stateManager;

        public UfoSwooping(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void OnEnter()
        {
            Debug.Log("Swoop");
        }
        public override void Tick()
        {
            stateManager.ufoMain.swoopDownTick();
        }

    }

    public class UfoAbduct : IState
    {
        private UfoStateManager stateManager;

        public UfoAbduct(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;

        }
        public override void OnEnter()
        {
            Debug.Log("Abduct");
            stateManager.ufoMain.abductCow();
        }

        public override void Tick()
        {
            stateManager.ufoMain.abductTick();

        }

    }

    public class UfoReturnSweep : IState
    {
        private UfoStateManager stateManager;
        private bool t = true;
        public UfoReturnSweep(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;

        }

        public override void OnEnter()
        {
            Debug.Log("Return");
            if (t) { t = false; stateManager.ufoMain.hit(); }
        }
        public override void Tick()
        {

            stateManager.ufoMain.swoopUpTick();


        }

    }

    public class UfoStaggered : IState
    {
        private UfoStateManager stateManager;
        private float start;
        private string lastState;
        private float wobbletime = 3f;
        public UfoStaggered(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }
        public override void OnEnter(object lastState)
        {

            start = Time.time;
            this.lastState = lastState as string;
        }
        public override void Tick()
        {
            stateManager.ufoMain.wobble();
            if(Time.time - start > 3)
            {
                if (lastState.Contains("UfoSearch"))      { RequestTransition<UfoSearch>(); }
                if (lastState.Contains("UfoSwooping"))    { RequestTransition<UfoSwooping>(); }
                if (lastState.Contains("UfoAbduct"))      { RequestTransition<UfoAbduct>(); }
                if (lastState.Contains("UfoReturnSweep")) { RequestTransition<UfoReturnSweep>(); }
                if (lastState.Contains("UfoDeath"))       { RequestTransition<UfoDeath>(); }
                else { RequestTransition<UfoIdle>(); }
            }
        }
        public override void OnExit()
        {
            stateManager.ufoMain.resetRotation();
        }

    }

    public class UfoDeath : IState
    {
        private float start;

        private UfoStateManager stateManager;
        public override void OnEnter()
        {

            start = Time.time;
        }
        public UfoDeath(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }


        public override void Tick()
        {
            if (Time.time - start > 3)
            {
                Destroy(stateManager.ufoMain.gameObject);
            }
            else
            {
                stateManager.ufoMain.deathTick();
            }
        }

    }
}
