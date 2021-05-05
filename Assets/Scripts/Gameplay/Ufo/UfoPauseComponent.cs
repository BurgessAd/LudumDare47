using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoPauseComponent : PauseComponent
{
    [SerializeField] private UfoAnimationComponent animationComponent;
    [SerializeField] private UfoMain ufo;
    [SerializeField] private FlightComponent flightComponent;
    public override void Pause()
    {
        animationComponent.enabled = false;
        ufo.enabled = false;
        flightComponent.enabled = false;
    }

    public override void Unpause()
    {
        animationComponent.enabled = true;
        ufo.enabled = true;
        flightComponent.enabled = true;
    }
}
