using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoStateManager : MonoBehaviour
{

    // setting up and adding states to state machine
    private StateMachine m_StateMachine;
    public UfoMain ufoMain;
    public GameObject selectedCow;
    public void Start()
    {
        ufoMain = gameObject.AddComponent(typeof(UfoMain)) as UfoMain;

                m_StateMachine = new StateMachine(new UfoIdle(this));
                m_StateMachine.AddState(new UfoSearch(this));
                m_StateMachine.AddState(new UfoSwooping(this));
                m_StateMachine.AddState(new UfoAbduct(this));
    }


    // state machine update
    public void Update()
    {
       // m_StateMachine.Tick();
    }

    public class UfoIdle : IState
    {
        private UfoStateManager stateManager;

        public UfoIdle(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void Tick()
        {
            // RequestTransition<UfoIdle>();
        }

    }
    
    public class UfoSearch : IState
    {
        private UfoStateManager stateManager;

        public UfoSearch(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void Tick()
        {
            stateManager.selectedCow = stateManager.ufoMain.FindCow();
            RequestTransition<UfoSwooping>();
        }

    }
  
    public class UfoSwooping : IState
    {
        private UfoStateManager stateManager;

        public UfoSwooping(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }


        public override void Tick()
        {
            if (!stateManager.ufoMain.inSwoop) { RequestTransition<UfoAbduct>(); }
            
        }
        public override void OnEnter()
        {
            stateManager.ufoMain.SwoopTo(stateManager.selectedCow);

        }
    }

    public class UfoAbduct : IState
    {
        private UfoStateManager stateManager;

        public UfoAbduct(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void Tick()
        {
            // RequestTransition<UfoIdle>();
        }

    }

    public class UfoReturnSweep : IState
    {
        private UfoStateManager stateManager;

        public UfoReturnSweep(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void Tick()
        {
            // RequestTransition<UfoIdle>();
        }

    }

    public class UfoStaggered : IState
    {
        private UfoStateManager stateManager;

        public UfoStaggered(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void Tick()
        {
            // RequestTransition<UfoIdle>();
        }

    }

    public class UfoDeath : IState
    {
        private UfoStateManager stateManager;

        public UfoDeath(UfoStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public override void Tick()
        {
            // RequestTransition<UfoIdle>();
        }

    }
}
