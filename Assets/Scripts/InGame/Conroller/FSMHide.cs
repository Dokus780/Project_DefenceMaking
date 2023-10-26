using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMHide : FSMSingleton<FSMHide>, CharState<Battler>
{
    /*excute : �������ֱ�
    out case
    0. hp�� 0���Ϸ� ����������� -> Dead��
    1. ��Ÿ� �ȿ� ���� ���� ��� -> Attack����
    1.1 ��Ÿ� ���� ������ CC(���) �ο� */

    private bool NeedChange(Battler e)
    {
        if (e.curHp <= 0)
        {
            e.ChangeState(FSMDead.Instance);
            return true;
        }

        return false;
    }

    private bool AttackCheck(Battler e)
    {
        //1.1
        Battler curTarget = e.BattleCheck();
        if (curTarget != null)
        {
            e.ChangeState(FSMPatrol.Instance);
            return true;
        }

        //1.2
        if (e.chaseTarget != null)
        {
            e.ChangeState(FSMChase.Instance);
            return true;
        }
        return false;
    }

    public void Enter(Battler e)
    {
        
    }

    public void Excute(Battler e)
    {
        if (GameManager.Instance.timeScale == 0)
            return;

        if (NeedChange(e))
            return;

        if (AttackCheck(e))
            return;
    }

    public void Exit(Battler e)
    {
        e._Animator.SetTrigger("Activated");
        List<Battler> rangedTargets = e.GetRangedTargets(e.transform.position, e.attackRange + 0.2f);
        foreach(Battler target in rangedTargets)
        {
            target.GetCC(e, 1f);
            target.ChangeState(FSMCC.Instance);
        }
    }
}
