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

        m_StateMachine = new StateMachine();
        m_StateMachine.AddState(new UfoSearch(this));
        m_StateMachine.AddState(new UfoSwooping(this));
        m_StateMachine.AddState(new UfoAbduct(this));
        m_StateMachine.AddState(new UfoReturnSweep(this));
        m_StateMachine.AddState(new UfoStaggered(this));
        m_StateMachine.AddState(new UfoDeath(this));
        m_StateMachine.AddState(new UfoIdle(this));
        m_StateMachine.SetInitialState(typeof(UfoIdle));
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
            
            FindObjectOfType<AudioManager>().PlayAt("Hover", stateManager.ufoMain.transform.position);
        }

        public override void OnExit()
        {

            FindObjectOfType<AudioManager>().stop("Hover");
        }

        public override void Tick()
        {
           RequestTransition<UfoSearch>();
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

            FindObjectOfType<AudioManager>().PlayAt("Hover", stateManager.ufoMain.transform.position);
        }

        public override void OnExit()
        {

            FindObjectOfType<AudioManager>().stop("Hover");
        }

        public override void Tick()
        {

            //if (stateManager.gameManager.cows.Count > 0) { 
            //    GameObject cow = stateManager.gameManager.cows[0];
            //    stateManager.ufoMain.setTarget(cow);
            //    RequestTransition<UfoSwooping>();
            //}
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

            FindObjectOfType<AudioManager>().PlayAt("Hover", stateManager.ufoMain.transform.position);
        }

        public override void OnExit()
        {

            FindObjectOfType<AudioManager>().stop("Hover");
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
            
            FindObjectOfType<AudioManager>().PlayAt("Abduction", stateManager.ufoMain.transform.position);
            stateManager.ufoMain.abductCow();
        }

        public override void OnExit()
        {

            FindObjectOfType<AudioManager>().stop("Abduction");
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

            FindObjectOfType<AudioManager>().PlayAt("Hover", stateManager.ufoMain.transform.position);
        }

        public override void OnExit()
        {

            FindObjectOfType<AudioManager>().stop("Hover");
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
            FindObjectOfType<AudioManager>().PlayAt("Stagger", stateManager.ufoMain.transform.position);
            start = Time.time;
            this.lastState = lastState as string;
        }
        public override void Tick()
        {
            stateManager.ufoMain.wobble();
            if(Time.time - start > 3)
            {
                if (lastState.Equals("UfoStateManager+UfoSearch"))      { RequestTransition<UfoSearch>(); }
                else if (lastState.Equals("UfoStateManager+UfoSwooping"))    { RequestTransition<UfoSwooping>(); }
                else if (lastState.Equals("UfoStateManager+UfoAbduct"))      { RequestTransition<UfoAbduct>(); }
                else if (lastState.Equals("UfoStateManager+UfoReturnSweep")) { RequestTransition<UfoReturnSweep>(); }
                else if (lastState.Equals("UfoStateManager+UfoDeath"))       { RequestTransition<UfoDeath>(); }
                else { RequestTransition<UfoIdle>(); }
            }
        }
        public override void OnExit()
        {
            FindObjectOfType<AudioManager>().stop("Stagger");
            stateManager.ufoMain.resetRotation();
        }

    }

    public class UfoDeath : IState
    {
        private float start;

        private UfoStateManager stateManager;
        public override void OnEnter()
        {
            FindObjectOfType<AudioManager>().PlayAt("Dead", stateManager.ufoMain.transform.position);
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
        public override void OnExit()
        {
            FindObjectOfType<AudioManager>().stop("Dead");
        }
    }
}
