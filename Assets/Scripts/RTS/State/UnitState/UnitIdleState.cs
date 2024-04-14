using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIdleState : State
{
    public UnitIdleState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        owner.SetVelocity(new Vector3(0, owner.GetVelocityV().y, 0));
    }
    public override void Update()
    {
        base.Update();

    }

    public override void Exit()
    {
        base.Exit();
    }

}
