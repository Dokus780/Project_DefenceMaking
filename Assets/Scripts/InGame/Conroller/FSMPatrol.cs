using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMPatrol : FSMSingleton<FSMPatrol>, CharState<Battler>
{
    /*excute : ���չ��� ���� Ž������
    out case
    0. hp�� 0���Ϸ� ����������� -> Dead��
    1.����Ÿ���� ��Ÿ��
    1.1 ��Ÿ� �ȿ� ���� ���� ��� -> Attack����, curTarget�� �� ����
    1.2 ���� ������ ������ ���ݹ��� ��� -> Chase��, curTarget�� �� ����
    2.����Ÿ���� ��Ÿ�� -> Room
    3.����Ÿ���� ���չ� -> KingAttack */

    private bool NeedChange(Battler e)
    {
        if (e.curHp <= 0)
        {
            e.ChangeState(FSMDead.Instance);
            return true;
        }

        //if()

        return false;
    }

    public void Enter(Battler e)
    {

    }

    public void Excute(Battler e)
    {
        if (NeedChange(e))
            return;
    }

    public void Exit(Battler e)
    {

    }
}
