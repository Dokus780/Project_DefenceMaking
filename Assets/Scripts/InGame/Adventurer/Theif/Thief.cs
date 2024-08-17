using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thief : Adventurer, IHide
{
    public virtual bool canAttackbyTrap { get => true; }

    public void HideAction()
    {
        if(CurTile == NodeManager.Instance.endPoint)
            ChangeState(FSMPatrol.Instance);
        else
            Patrol();
    }

    public override void Init()
    {
        base.Init();
        ChangeState(FSMHide.Instance);
    }
}
