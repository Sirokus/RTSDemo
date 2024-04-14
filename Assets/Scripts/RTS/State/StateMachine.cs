using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine : MonoBehaviour
{
    public State currentState;
    public Unit owner;

    public StateMachine(State currentState, Unit owner)
    {
        this.currentState = currentState;
        this.owner = owner;
    }

    protected virtual void Awake() { }
    

    protected virtual void Start()
    {
        currentState.Enter();
    }
    
    protected virtual void Update()
    {
        currentState.Update();
    }

    public void EnterState(State state)
    {
        if(currentState != null) currentState.Exit();
        currentState = state;
        currentState.Enter();
    }
}
