using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Monster : Battler
{
    private int monsterIndex = -1;

    protected override void DirectPass()
    {
        //����Ÿ�Ϸ� �̵�����
        if (nextNode == null || nextNode.curTile == null)
        {
            List<TileNode> path = PathFinder.Instance.FindPath(curTile, lastCrossRoad);
            if (path != null && path.Count > 0)
                nextNode = path[0];
            else
            {
                crossedNodes = new List<TileNode>();
                lastCrossRoad = null;
                prevTile = null;
                directPass = false;
                return;
            }
        }

        ExcuteMove(nextNode);

        // nextNode���� �̵��Ϸ�
        if (Vector3.Distance(transform.position, nextNode.transform.position) < 0.001f)
            NodeAction(nextNode);
    }

    protected override TileNode FindNextNode(TileNode curNode)
    {
        //startNode���� roomDirection�̳� pathDirection�� �ִ� ������ �̿���带 �޾ƿ�
        List<TileNode> nextNodes = UtilHelper.GetConnectedNodes(curNode);
        //�ش� ��忡�� ���� ���� ���� ����
        nextNodes.Remove(prevTile);
        nextNodes.Remove(NodeManager.Instance.startPoint);
        nextNodes.Remove(NodeManager.Instance.endPoint);
        if (crossedNodes != null)
        {
            foreach (TileNode node in crossedNodes)
                nextNodes.Remove(node);
        }

        if (nextNodes.Count == 0)
            return null;

        //�������ϰ�� ����
        if (nextNodes.Count > 1)
            lastCrossRoad = curNode;

        TileNode nextNode = nextNodes[UnityEngine.Random.Range(0, nextNodes.Count)];
        return nextNode;
    }

    public void SetStartPoint(TileNode tile)
    {
        curTile = tile;
    }

    public override void Init()
    {
        base.Init();

        monsterIndex = UtilHelper.Find_Data_Index(battlerID, DataManager.Instance.Battler_Table, "id");
        if(monsterIndex != -1)
        {
            maxHp = Convert.ToInt32(DataManager.Instance.Battler_Table[monsterIndex]["hp"]);
            maxHp += PassiveManager.Instance.monsterHp_Weight;

            curHp = maxHp;
            damage = Convert.ToInt32(DataManager.Instance.Battler_Table[monsterIndex]["attackPower"]);
            float.TryParse(DataManager.Instance.Battler_Table[monsterIndex]["attackSpeed"].ToString(), out attackSpeed);
            armor = Convert.ToInt32(DataManager.Instance.Battler_Table[monsterIndex]["armor"]);
            float.TryParse(DataManager.Instance.Battler_Table[monsterIndex]["moveSpeed"].ToString(), out moveSpeed);

            float.TryParse(DataManager.Instance.Battler_Table[monsterIndex]["attackRange"].ToString(), out attackRange);
        }

        InitState(this, FSMPatrol.Instance);
    }

    public override void Update()
    {
        base.Update();

        if (animator != null)
            animator.SetFloat("AttackSpeed", attackSpeed * GameManager.Instance.timeScale);

        //bool isBattleOn = BattleCheck();
        //if (battleState)
        //    AttackEndCheck();
        //else if (isBattleOn)
        //    ExcuteBattle();
    }
}
