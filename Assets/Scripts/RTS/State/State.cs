using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    [Header("State Base")]
    protected StateMachine stateMachine;
    protected Unit owner;

    [Header("Timer")]
    protected float timer = 0;
    protected float coolDown = 1;

    public State(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        owner = stateMachine.owner;
    }

    public virtual void Enter()
    {
        timer = coolDown;
    }

    public virtual void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) OnTimerDone();
    }

    public virtual void Exit() 
    { 
        
    }

    public virtual void OnTimerDone()
    {
        timer = coolDown;
    }

}
