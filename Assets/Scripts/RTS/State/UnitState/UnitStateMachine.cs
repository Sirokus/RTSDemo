using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStateMachine : StateMachine
{
    public UnitStateMachine(State currentState, Unit owner) : base(currentState, owner)
    {
    }

    public UnitIdleState idle;
    public UnitMoveState move;
    public UnitAttackState attack;
    public UnitDeadState dead;

    protected override void Awake() 
    {
        base.Awake();

        idle = new UnitIdleState(this);
        move = new UnitMoveState(this);
        attack = new UnitAttackState(this);
        dead = new UnitDeadState(this);
    }

    protected override void Start()
    {
        base.Start();


    }

    protected override void Update()
    {
        base.Update();


    }
}
